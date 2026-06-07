using FFXIVHudPlugin.AetherPlates.Configuration;
using FFXIVHudPlugin.AetherPlates.Data;
using FFXIVHudPlugin.AetherPlates.Services;
using Dalamud.Game.Gui.NamePlate;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Plugin.Services;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using StructsGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;
using StructsVector3 = FFXIVClientStructs.FFXIV.Common.Math.Vector3;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

namespace FFXIVHudPlugin.AetherPlates.Core;

public sealed class NameplateManager
{
    public enum NameplateCategory
    {
        Self,
        SelfCompanion,
        SelfPet,
        Party,
        PartyCompanion,
        PartyPet,
        Alliance,
        AlliancePet,
        Friend,
        FriendCompanion,
        FriendPet,
        OtherPc,
        OtherCompanion,
        OtherPet,
        EnemyUnengaged,
        EnemyEngaged,
        EnemyClaimed,
        EnemyUnclaimed,
        EnemyFeast,
        EnemyFeastPet,
        Boss,
        Npc,
        Object,
        Minion,
        HousingFurniture,
        HousingField,
        UnknownFriendly,
    }

    private readonly NameplateTracker tracker;
    private readonly NameplateRenderer renderer;
    private readonly IProjectionService projectionService;
    private readonly NativeNameplateAnchorService nativeAnchorService;
    private readonly ITextureProvider textureProvider;
    private readonly PluginConfiguration configuration;
    private readonly Dictionary<ulong, ActorStateCacheEntry> actorCache = new();
    private Dictionary<ulong, TrackedObject> currentObjectsById = new();
    private long frameCounter;

    private sealed class ActorStateCacheEntry
    {
        public required TrackedObject Tracked { get; init; }
        public required Vector2 LastAnchor { get; set; }
        public required NameplateCategory LastCategory { get; set; }
        public required long LastSeenFrame { get; set; }
        public required bool HadNativeAnchor { get; set; }
        public float CalibratedScreenYOffset { get; set; }
    }

    internal IReadOnlyDictionary<ulong, TrackedObject> CurrentObjectsById => this.currentObjectsById;

    public NameplateManager(
        NameplateTracker tracker,
        NameplateRenderer renderer,
        IProjectionService projectionService,
        NativeNameplateAnchorService nativeAnchorService,
        ITextureProvider textureProvider,
        PluginConfiguration configuration)
    {
        this.tracker = tracker;
        this.renderer = renderer;
        this.projectionService = projectionService;
        this.nativeAnchorService = nativeAnchorService;
        this.textureProvider = textureProvider;
        this.configuration = configuration;
    }

    public void UpdateAndDraw()
    {
        this.frameCounter++;
        if (!this.configuration.Enabled)
        {
            return;
        }

        if (!this.configuration.CategoryVisibility.IsAnyEnabled())
        {
            return;
        }

        this.tracker.Update();
        var tracked = this.tracker.CurrentFrame.Objects;
        var profile = this.configuration.GetActiveProfile();
        var offset = new Vector3(0f, this.configuration.VerticalOffset, 0f);
        var localPlayerId = 0UL;
        var ownerLookup = new Dictionary<ulong, TrackedObject>(tracked.Count);
        for (var i = 0; i < tracked.Count; i++)
        {
            var trackedObj = tracked[i];
            ownerLookup[trackedObj.ObjectId] = trackedObj;
            if (trackedObj.IsPlayerCharacter)
            {
                localPlayerId = trackedObj.ObjectId;
            }
        }
        this.currentObjectsById = ownerLookup;
        var primaryBossObjectId = this.ResolvePrimaryBossObjectId(tracked, localPlayerId, ownerLookup);
        var bossEncounterActive = primaryBossObjectId != 0;

        for (var i = 0; i < tracked.Count; i++)
        {
            var obj = tracked[i];
            if (!ShouldRenderObject(obj))
            {
                continue;
            }

            var nativeKind = this.nativeAnchorService.TryGetKind(obj.ObjectId, out var plateKind) ? plateKind : (NamePlateKind?)null;
            var category = NameplateCategoryResolver.ResolveForTrackedObject(obj, localPlayerId, nativeKind, ownerLookup);
            var isBossCandidate = bossEncounterActive && IsBossCategoryCandidate(obj, category);
            if (isBossCandidate)
            {
                if (obj.ObjectId != primaryBossObjectId)
                {
                    // During boss encounters, boss-type enemies are only rendered via Bosses (Target Bar),
                    // never through the normal enemy categories.
                    continue;
                }

                category = NameplateCategory.Boss;
            }
            if (!this.IsCategoryEnabled(category))
            {
                continue;
            }

            if (this.RequiresNativePresence(category) && !this.nativeAnchorService.IsInCurrentNativeSet(obj.ObjectId))
            {
                // NPC/minion/object-like categories should only be rendered when the game currently has
                // an active native nameplate for that actor; this prevents phantom labels from cache/projection.
                continue;
            }

            var categoryVisual = this.configuration.GetVisualSettingsForCategory(category);
            var isHostile = category is NameplateCategory.EnemyUnengaged
                or NameplateCategory.EnemyEngaged
                or NameplateCategory.EnemyClaimed
                or NameplateCategory.EnemyUnclaimed
                or NameplateCategory.EnemyFeast
                or NameplateCategory.EnemyFeastPet
                or NameplateCategory.Boss;
            var isFriendly = !isHostile && category != NameplateCategory.Self;

            if (this.configuration.EnableDistanceCulling &&
                !this.IsInRangeForCategory(obj, category, isHostile, isFriendly))
            {
                continue;
            }

            if (!this.TryResolveAnchor(obj, category, offset, out var screen))
            {
                continue;
            }

            this.actorCache[obj.ObjectId] = new ActorStateCacheEntry
            {
                Tracked = obj,
                LastAnchor = screen,
                LastCategory = category,
                LastSeenFrame = this.frameCounter,
                HadNativeAnchor = this.nativeAnchorService.IsInCurrentNativeSet(obj.ObjectId),
                CalibratedScreenYOffset = this.actorCache.TryGetValue(obj.ObjectId, out var existingCache)
                    ? existingCache.CalibratedScreenYOffset
                    : 0f,
            };

            var context = new NameplateContext(
                obj,
                profile,
                categoryVisual,
                this.textureProvider,
                screen,
                this.configuration.TemporaryGlobalScale,
                obj.IsTarget,
                obj.IsFocusTarget,
                category == NameplateCategory.Boss,
                obj.IsPartyMember,
                obj.IsAllianceMember,
                isHostile,
                isFriendly,
                obj.Distance,
                this.configuration.ResolveFontFamilyId(categoryVisual));

            this.renderer.DrawNameplate(context, categoryVisual.EnabledWidgetIdsSet);
        }

        this.TrimActorCache();
    }

    private bool IsInRangeForCategory(
        TrackedObject obj,
        NameplateCategory category,
        bool isHostile,
        bool isFriendly)
    {
        var inCombatRangeActive = this.configuration.EnableDynamicCombatRange &&
                                  this.IsCombatRelevant(obj, category, isHostile);
        var enemyRange = inCombatRangeActive
            ? this.configuration.CombatEnemyMaxDistanceYalms
            : this.configuration.EnemyMaxDistanceYalms;
        var friendlyRange = inCombatRangeActive
            ? this.configuration.CombatFriendlyMaxDistanceYalms
            : this.configuration.FriendlyMaxDistanceYalms;

        if (category == NameplateCategory.Self)
        {
            return obj.Distance <= Math.Max(1f, this.configuration.PlayerMaxDistanceYalms);
        }

        if (isHostile)
        {
            return obj.Distance <= Math.Max(1f, enemyRange);
        }

        if (isFriendly || category is NameplateCategory.Party or NameplateCategory.Alliance or NameplateCategory.Friend or NameplateCategory.OtherPc or NameplateCategory.Self)
        {
            return obj.Distance <= Math.Max(1f, friendlyRange);
        }

        return true;
    }

    private bool IsCombatRelevant(TrackedObject obj, NameplateCategory category, bool isHostile)
    {
        if (category is NameplateCategory.EnemyUnengaged
            or NameplateCategory.EnemyEngaged
            or NameplateCategory.EnemyClaimed
            or NameplateCategory.EnemyUnclaimed
            or NameplateCategory.EnemyFeast
            or NameplateCategory.EnemyFeastPet
            or NameplateCategory.Boss || isHostile)
        {
            return true;
        }

        if (obj.CastInfo.IsCasting)
        {
            return true;
        }

        return obj.IsTarget;
    }

    private bool IsCategoryEnabled(NameplateCategory category)
    {
        var c = this.configuration.CategoryVisibility;
        return category switch
        {
            NameplateCategory.Self => c.Self,
            NameplateCategory.SelfCompanion => c.SelfCompanion,
            NameplateCategory.SelfPet => c.SelfPet,
            NameplateCategory.Party => c.PartyMember,
            NameplateCategory.PartyCompanion => c.PartyCompanion,
            NameplateCategory.PartyPet => c.PartyPet,
            NameplateCategory.Alliance => c.AllianceMember,
            NameplateCategory.AlliancePet => c.AlliancePet,
            NameplateCategory.Friend => c.Friend,
            NameplateCategory.FriendCompanion => c.FriendCompanion,
            NameplateCategory.FriendPet => c.FriendPet,
            NameplateCategory.OtherPc => c.OtherPc,
            NameplateCategory.OtherCompanion => c.OtherCompanion,
            NameplateCategory.OtherPet => c.OtherPet,
            NameplateCategory.EnemyUnengaged => c.EnemyUnengaged,
            NameplateCategory.EnemyEngaged => c.EnemyEngaged,
            NameplateCategory.EnemyClaimed => c.EnemyClaimed,
            NameplateCategory.EnemyUnclaimed => c.EnemyUnclaimed,
            NameplateCategory.EnemyFeast => c.EnemyFeast,
            NameplateCategory.EnemyFeastPet => c.EnemyFeastPet,
            NameplateCategory.Boss => c.Boss,
            NameplateCategory.Npc => c.Npc,
            NameplateCategory.Object => c.Object,
            NameplateCategory.Minion => c.Minion,
            NameplateCategory.HousingFurniture => c.HousingFurniture,
            NameplateCategory.HousingField => c.HousingField,
            // Unknown classifications should never be implicitly shown; this avoids random uncategorized NPC plates.
            NameplateCategory.UnknownFriendly => false,
            _ => false,
        };
    }

    private bool TryResolveAnchor(TrackedObject obj, NameplateCategory category, Vector3 offset, out Vector2 screen)
    {
        screen = default;
        if (category == NameplateCategory.Boss)
        {
            screen = ResolveScreenCenterAnchor(this.configuration.BossTargetBarAnchorOffset);
            return this.IsValidAnchor(screen);
        }

        // Highest-priority non-boss anchor: game's own nameplate world position for this actor.
        if (TryGetNativeWorldNameplatePosition(obj, out var nativeWorldPos) &&
            this.projectionService.WorldToScreen(nativeWorldPos, out var nativeWorldScreen) &&
            this.IsValidAnchor(nativeWorldScreen))
        {
            screen = nativeWorldScreen;
            return true;
        }

        // For all non-boss categories, follow the game's native nameplate anchor directly.
        // This guarantees our plate sits where the native plate sits.
        if (this.nativeAnchorService.TryGetAnchor(obj.ObjectId, out var nativeOnlyScreen) &&
            this.IsValidAnchor(nativeOnlyScreen))
        {
            screen = nativeOnlyScreen;
            return true;
        }

        var projectionY = this.GetAutoProjectionYOffset(obj, offset);
        var projectionOffset = new Vector3(0f, projectionY, 0f);

        var hasProjectedAnchor = this.projectionService.WorldToScreen(obj.Position + projectionOffset, out var projectedScreen);
        var projectedAnchorIsValid = hasProjectedAnchor && this.IsValidAnchor(projectedScreen);
        var hasNativeAnchor = this.nativeAnchorService.TryGetAnchor(obj.ObjectId, out var nativeScreen) && this.IsValidAnchor(nativeScreen);
        if (hasNativeAnchor)
        {
            this.UpdateProjectionCalibration(obj, nativeScreen, offset);

            if (projectedAnchorIsValid)
            {
                // Keep X centered on the model/projection so X offset of 0 aligns with actor center.
                screen = new Vector2(projectedScreen.X, nativeScreen.Y);
                return true;
            }

            screen = nativeScreen;
            return true;
        }

        if (hasProjectedAnchor)
        {
            if (this.actorCache.TryGetValue(obj.ObjectId, out var cache))
            {
                projectedScreen = new Vector2(projectedScreen.X, projectedScreen.Y + cache.CalibratedScreenYOffset);
            }

            if (!this.IsValidAnchor(projectedScreen))
            {
                // continue to cached fallback path
            }
            else
            {
                screen = projectedScreen;
                return true;
            }
        }

        if (this.actorCache.TryGetValue(obj.ObjectId, out var cached) &&
            this.frameCounter - cached.LastSeenFrame <= 30 &&
            this.IsValidAnchor(cached.LastAnchor))
        {
            screen = cached.LastAnchor;
            return true;
        }

        return false;
    }

    private static unsafe bool TryGetNativeWorldNameplatePosition(TrackedObject obj, out Vector3 worldPosition)
    {
        worldPosition = default;
        if (obj.Address == nint.Zero)
        {
            return false;
        }

        var nativeObject = (StructsGameObject*)obj.Address;
        if (nativeObject == null)
        {
            return false;
        }

        var world = default(StructsVector3);
        nativeObject->GetNamePlateWorldPosition(&world);
        worldPosition = new Vector3(world.X, world.Y, world.Z);
        return float.IsFinite(worldPosition.X) &&
               float.IsFinite(worldPosition.Y) &&
               float.IsFinite(worldPosition.Z);
    }

    private unsafe bool TryResolveTargetBarAnchor(out Vector2 screen)
    {
        screen = default;
        var stage = AtkStage.Instance();
        if (stage is null)
        {
            return false;
        }

        var addon = stage->RaptureAtkUnitManager->GetAddonByName("_TargetInfo", 1);
        if (addon is null || addon->RootNode is null || !addon->IsVisible)
        {
            return false;
        }

        var root = addon->RootNode;
        var width = root->Width * MathF.Max(root->ScaleX, 0.01f);
        var x = root->GetXFloat();
        var y = root->GetYFloat();
        if (!float.IsFinite(x) || !float.IsFinite(y) || width <= 1f)
        {
            return false;
        }

        screen = new Vector2(x + (width * 0.5f), y - 18f);
        return this.IsValidAnchor(screen);
    }

    private static Vector2 ResolveScreenCenterAnchor(Vector2 offsetFromCenter)
    {
        var viewport = Dalamud.Bindings.ImGui.ImGui.GetMainViewport();
        if (viewport.Size.X <= 0f || viewport.Size.Y <= 0f)
        {
            return offsetFromCenter;
        }

        var center = viewport.Pos + (viewport.Size * 0.5f);
        return center + offsetFromCenter;
    }

    private float GetAutoProjectionYOffset(TrackedObject obj, Vector3 offset)
    {
        var baseModelOffset = GetBaseModelProjectionYOffset(obj);
        var fallbackBias = MathF.Max(0f, offset.Y);
        // Auto-Y priority: model-derived head anchor with fallback bias.
        return MathF.Max(baseModelOffset, fallbackBias);
    }

    private ulong ResolvePrimaryBossObjectId(
        IReadOnlyList<TrackedObject> tracked,
        ulong localPlayerId,
        IReadOnlyDictionary<ulong, TrackedObject> ownerLookup)
    {
        TrackedObject? best = null;
        var secondBestMaxHp = 0u;
        for (var i = 0; i < tracked.Count; i++)
        {
            var obj = tracked[i];
            if (!ShouldRenderObject(obj))
            {
                continue;
            }

            var nativeKind = this.nativeAnchorService.TryGetKind(obj.ObjectId, out var plateKind) ? plateKind : (NamePlateKind?)null;
            var category = NameplateCategoryResolver.ResolveForTrackedObject(obj, localPlayerId, nativeKind, ownerLookup);
            if (!IsBossCategoryCandidate(obj, category))
            {
                continue;
            }

            if (best is null)
            {
                best = obj;
                continue;
            }

            if (obj.IsTarget && !best.IsTarget)
            {
                secondBestMaxHp = Math.Max(secondBestMaxHp, best.MaxHp);
                best = obj;
                continue;
            }

            if (obj.IsTarget == best.IsTarget)
            {
                var objCombatWeight = GetEnemyStatePriority(obj.EnemyState);
                var bestCombatWeight = GetEnemyStatePriority(best.EnemyState);
                if (objCombatWeight > bestCombatWeight)
                {
                    secondBestMaxHp = Math.Max(secondBestMaxHp, best.MaxHp);
                    best = obj;
                    continue;
                }

                if (objCombatWeight == bestCombatWeight)
                {
                    if (obj.MaxHp > best.MaxHp)
                    {
                        secondBestMaxHp = Math.Max(secondBestMaxHp, best.MaxHp);
                        best = obj;
                        continue;
                    }

                    if (obj.MaxHp == best.MaxHp && obj.Height > best.Height)
                    {
                        best = obj;
                    }
                }
            }

            if (best is not null && obj.ObjectId != best.ObjectId)
            {
                secondBestMaxHp = Math.Max(secondBestMaxHp, obj.MaxHp);
            }
        }

        if (best is null)
        {
            return 0;
        }

        if (IsConfirmedBossByStats(best))
        {
            return best.ObjectId;
        }

        // Fallback: allow boss routing when a clearly dominant target exists over surrounding enemies.
        if (best.IsTarget &&
            best.MaxHp >= 1_300_000 &&
            best.MaxHp >= (uint)Math.Ceiling(Math.Max(1u, secondBestMaxHp) * 2.0))
        {
            return best.ObjectId;
        }

        return 0;
    }

    private static int GetEnemyStatePriority(EnemyNameplateState state)
    {
        return state switch
        {
            EnemyNameplateState.Engaged => 3,
            EnemyNameplateState.Claimed => 2,
            EnemyNameplateState.Unclaimed => 2,
            EnemyNameplateState.Unengaged => 1,
            EnemyNameplateState.Feast => 1,
            EnemyNameplateState.FeastPet => 0,
            _ => 0,
        };
    }

    private static bool IsBossCategoryCandidate(TrackedObject obj, NameplateCategory initialCategory)
    {
        if (initialCategory is not NameplateCategory.EnemyUnengaged
            and not NameplateCategory.EnemyEngaged
            and not NameplateCategory.EnemyClaimed
            and not NameplateCategory.EnemyUnclaimed
            and not NameplateCategory.EnemyFeast
            and not NameplateCategory.EnemyFeastPet)
        {
            return false;
        }

        if (!obj.Targetable || obj.CurrentHp == 0 || obj.MaxHp == 0)
        {
            return false;
        }

        // Prefer native game-provided classification: nameplate icon IDs are used for special enemy markers.
        // Non-zero here is a much stronger signal than HP/height alone.
        if (obj.NameplateIconId != 0)
        {
            return true;
        }

        // Broad pre-filter; final confirmation is done by ResolvePrimaryBossObjectId().
        return obj.MaxHp >= 900_000 || obj.Height >= 5.5f;
    }

    private static bool IsConfirmedBossByStats(TrackedObject obj)
    {
        if (obj.NameplateIconId != 0)
        {
            return true;
        }

        // Conservative thresholds to avoid dungeon trash being treated as bosses.
        if (obj.MaxHp >= 2_000_000)
        {
            return true;
        }

        return obj.Height >= 6.5f && obj.MaxHp >= 900_000;
    }

    private void UpdateProjectionCalibration(TrackedObject obj, Vector2 nativeScreen, Vector3 offset)
    {
        var baseModelOffset = GetBaseModelProjectionYOffset(obj);
        var fallbackBias = MathF.Max(0f, offset.Y);
        var currentProjectionY = MathF.Max(baseModelOffset, fallbackBias);
        if (!this.projectionService.WorldToScreen(obj.Position + new Vector3(0f, currentProjectionY, 0f), out var projectedScreen) ||
            !this.IsValidAnchor(projectedScreen))
        {
            return;
        }

        var deltaY = nativeScreen.Y - projectedScreen.Y;
        if (!float.IsFinite(deltaY))
        {
            return;
        }

        if (!this.actorCache.TryGetValue(obj.ObjectId, out var cache))
        {
            return;
        }

        // Smooth calibration to avoid noisy single-frame spikes.
        cache.CalibratedScreenYOffset = (cache.CalibratedScreenYOffset * 0.85f) + (deltaY * 0.15f);
    }

    private static float GetBaseModelProjectionYOffset(TrackedObject obj)
    {
        var isBattleNpc = obj.Kind == ObjectKind.BattleNpc;
        if (obj.Height <= 0.01f)
        {
            return isBattleNpc ? 2.8f : 2.0f;
        }

        // Battle NPCs tend to report lower capsule/height values relative to visual head position.
        var multiplier = isBattleNpc ? 2.85f : 2.2f;
        var projected = obj.Height * multiplier;
        if (isBattleNpc)
        {
            projected = MathF.Max(projected, 3.0f);
        }

        return projected;
    }

    private static bool ShouldRenderObject(TrackedObject obj)
    {
        if (obj.Targetable)
        {
            return true;
        }

        return obj.Kind is ObjectKind.Companion or ObjectKind.EventNpc;
    }

    private bool RequiresNativePresence(NameplateCategory category)
    {
        return category is NameplateCategory.Npc
            or NameplateCategory.Minion
            or NameplateCategory.Object
            or NameplateCategory.HousingFurniture
            or NameplateCategory.HousingField;
    }

    private void TrimActorCache()
    {
        if (this.actorCache.Count == 0)
        {
            return;
        }

        var staleIds = new List<ulong>();
        foreach (var pair in this.actorCache)
        {
            if (this.frameCounter - pair.Value.LastSeenFrame > 180)
            {
                staleIds.Add(pair.Key);
            }
        }

        for (var i = 0; i < staleIds.Count; i++)
        {
            this.actorCache.Remove(staleIds[i]);
        }
    }

    private bool IsValidAnchor(Vector2 screen)
    {
        if (!float.IsFinite(screen.X) || !float.IsFinite(screen.Y))
        {
            return false;
        }

        if (screen.X <= 1f || screen.Y <= 1f)
        {
            return false;
        }

        var viewport = Dalamud.Bindings.ImGui.ImGui.GetMainViewport();
        if (viewport.Size.X <= 0f || viewport.Size.Y <= 0f)
        {
            return true;
        }

        return screen.X >= viewport.Pos.X - 64f &&
               screen.Y >= viewport.Pos.Y - 64f &&
               screen.X <= viewport.Pos.X + viewport.Size.X + 64f &&
               screen.Y <= viewport.Pos.Y + viewport.Size.Y + 64f;
    }
}
