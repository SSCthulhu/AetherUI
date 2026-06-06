using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui.NamePlate;
using FFXIVHudPlugin.AetherPlates.Data;

namespace FFXIVHudPlugin.AetherPlates.Core;

internal static class NameplateCategoryResolver
{
    public static NameplateManager.NameplateCategory ResolveForTrackedObject(
        TrackedObject obj,
        ulong localPlayerId,
        NamePlateKind? nativeKind,
        IReadOnlyDictionary<ulong, TrackedObject> ownerLookup)
    {
        if (obj.Kind == ObjectKind.Companion)
        {
            return NameplateManager.NameplateCategory.Minion;
        }

        if (obj.Kind is ObjectKind.GatheringPoint or ObjectKind.Treasure or ObjectKind.EventObj)
        {
            return NameplateManager.NameplateCategory.Object;
        }

        if (obj.Kind == ObjectKind.HousingEventObject)
        {
            return NameplateManager.NameplateCategory.HousingFurniture;
        }

        var subKind = (BattleNpcSubKind)obj.SubKind;
        if (subKind == BattleNpcSubKind.NpcPartyMember)
        {
            // Duty Support / Trust actors should follow NPC category mapping.
            return NameplateManager.NameplateCategory.Npc;
        }

        if (obj.IsPlayerCharacter)
        {
            return NameplateManager.NameplateCategory.Self;
        }

        if (obj.IsPartyMember)
        {
            return NameplateManager.NameplateCategory.Party;
        }

        if (obj.IsAllianceMember)
        {
            return NameplateManager.NameplateCategory.Alliance;
        }

        if (obj.IsFriend)
        {
            return NameplateManager.NameplateCategory.Friend;
        }

        if (nativeKind.HasValue)
        {
            switch (nativeKind.Value)
            {
                case NamePlateKind.PlayerCharacter:
                    return NameplateManager.NameplateCategory.OtherPc;
                case NamePlateKind.BattleNpcEnemy:
                    return NameplateManager.NameplateCategory.Enemy;
                case NamePlateKind.BattleNpcFriendly:
                    return ResolveFriendlyBattleNpcCategory(obj, localPlayerId, ownerLookup);
                case NamePlateKind.EventObject:
                case NamePlateKind.GatheringPoint:
                case NamePlateKind.Treasure:
                    return NameplateManager.NameplateCategory.Object;
                default:
                    return NameplateManager.NameplateCategory.UnknownFriendly;
            }
        }

        if (obj.IsHostile)
        {
            return NameplateManager.NameplateCategory.Enemy;
        }

        return ResolveFriendlyBattleNpcCategory(obj, localPlayerId, ownerLookup);
    }

    public static NameplateManager.NameplateCategory ResolveForNativeHandler(
        NamePlateKind nativeKind,
        ulong gameObjectId,
        IGameObject? gameObject,
        ICharacter? playerCharacter,
        ulong localPlayerId,
        uint localPlayerEntityId,
        nint localPlayerAddress,
        IReadOnlyDictionary<ulong, TrackedObject> ownerLookup)
    {
        if (localPlayerId != 0 && gameObjectId == localPlayerId)
        {
            return NameplateManager.NameplateCategory.Self;
        }

        if (playerCharacter is not null)
        {
            if ((localPlayerId != 0 && playerCharacter.GameObjectId == localPlayerId) ||
                (localPlayerEntityId != 0 && playerCharacter.EntityId == localPlayerEntityId))
            {
                return NameplateManager.NameplateCategory.Self;
            }
        }

        if (localPlayerAddress != nint.Zero)
        {
            if ((gameObject is not null && gameObject.Address == localPlayerAddress) ||
                (playerCharacter is not null && playerCharacter.Address == localPlayerAddress))
            {
                return NameplateManager.NameplateCategory.Self;
            }
        }

        var tracked = ownerLookup.TryGetValue(gameObjectId, out var fromLookup)
            ? fromLookup
            : null;

        if (tracked is not null)
        {
            return ResolveForTrackedObject(tracked, localPlayerId, nativeKind, ownerLookup);
        }

        if (nativeKind == NamePlateKind.PlayerCharacter)
        {
            return NameplateManager.NameplateCategory.OtherPc;
        }

        if (nativeKind == NamePlateKind.BattleNpcEnemy)
        {
            return NameplateManager.NameplateCategory.Enemy;
        }

        if (nativeKind == NamePlateKind.BattleNpcFriendly)
        {
            return NameplateManager.NameplateCategory.Npc;
        }

        return NameplateManager.NameplateCategory.UnknownFriendly;
    }

    private static NameplateManager.NameplateCategory ResolveFriendlyBattleNpcCategory(
        TrackedObject obj,
        ulong localPlayerId,
        IReadOnlyDictionary<ulong, TrackedObject> ownerLookup)
    {
        var subKind = (BattleNpcSubKind)obj.SubKind;
        if (subKind == BattleNpcSubKind.NpcPartyMember)
        {
            return NameplateManager.NameplateCategory.Npc;
        }

        if (obj.OwnerId != 0 && ownerLookup.TryGetValue(obj.OwnerId, out var owner))
        {
            if (owner.IsPlayerCharacter || owner.ObjectId == localPlayerId)
            {
                return subKind == BattleNpcSubKind.Buddy
                    ? NameplateManager.NameplateCategory.SelfCompanion
                    : NameplateManager.NameplateCategory.SelfPet;
            }

            if (owner.IsPartyMember)
            {
                return subKind == BattleNpcSubKind.Buddy
                    ? NameplateManager.NameplateCategory.PartyCompanion
                    : NameplateManager.NameplateCategory.PartyPet;
            }

            if (owner.IsAllianceMember)
            {
                return NameplateManager.NameplateCategory.AlliancePet;
            }

            if (owner.IsFriend)
            {
                return subKind == BattleNpcSubKind.Buddy
                    ? NameplateManager.NameplateCategory.FriendCompanion
                    : NameplateManager.NameplateCategory.FriendPet;
            }

            return subKind == BattleNpcSubKind.Buddy
                ? NameplateManager.NameplateCategory.OtherCompanion
                : NameplateManager.NameplateCategory.OtherPet;
        }

        if (obj.OwnerId != 0)
        {
            if (obj.OwnerId == localPlayerId)
            {
                return subKind == BattleNpcSubKind.Buddy
                    ? NameplateManager.NameplateCategory.SelfCompanion
                    : NameplateManager.NameplateCategory.SelfPet;
            }

            return subKind == BattleNpcSubKind.Buddy
                ? NameplateManager.NameplateCategory.OtherCompanion
                : NameplateManager.NameplateCategory.OtherPet;
        }

        return NameplateManager.NameplateCategory.Npc;
    }
}
