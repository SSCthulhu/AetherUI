using DelvUI.Config;
using DelvUI.Config.Home;
using DelvUI.Config.Home.Widgets;
using DelvUI.Config.Navigation;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.Jobs;
using DelvUI.Interface.EnemyList;
using DelvUI.Interface.Party;
using DelvUI.Interface.StatusEffects;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace DelvUI.Config.Presets
{
    public enum AetherPreset
    {
        Minimal = 0,
        MmoModern = 1,
        RaidFocused = 2,
        ActionCombat = 3
    }

    public static class PresetManager
    {
        private static int _pendingTabIndex = 4;
        private static bool _pendingInitialized = false;
        private static int _confirmTabIndex = -1;
        private static bool _confirmRequested = false;
        private static string? _applyErrorMessage = null;
        private static bool _suppressCustomPresetMark = false;

        public static void MarkCustomIfNeeded()
        {
            if (_suppressCustomPresetMark)
            {
                return;
            }

            HomeFeatureSettingsConfig? settings = ConfigurationManager.Instance.GetConfigObject<HomeFeatureSettingsConfig>();
            if (settings == null)
            {
                return;
            }

            if (settings.ActivePreset == ActivePresetSelection.Custom)
            {
                _pendingTabIndex = 4;
                return;
            }

            settings.ActivePreset = ActivePresetSelection.Custom;
            _pendingTabIndex = 4;
        }

        public static void DrawPresetTabBar(float sectionWidth)
        {
            HomeFeatureSettingsConfig? settings = ConfigurationManager.Instance.GetConfigObject<HomeFeatureSettingsConfig>();
            if (settings == null)
            {
                return;
            }

            PresetHudLayoutSettingsConfig? hudLayoutSettings =
                ConfigurationManager.Instance.GetConfigObject<PresetHudLayoutSettingsConfig>();

            if (!_pendingInitialized)
            {
                SyncPendingFromActive(settings);
                _pendingInitialized = true;
            }

            using (HomeBorderedSectionScope section = HomeBorderedSectionScope.Begin("HUD Preset", sectionWidth))
            {
                float scale = ImGuiHelpers.GlobalScale;
                float innerWidth = section.InnerWidth;
                float tabSpacing = 12f * scale;
                int tabCount = HomePresetTabBar.TabCount;
                float tabHeight = 62f * scale;
                float rowTargetWidth = innerWidth * 0.94f;
                float tabWidth = (rowTargetWidth - tabSpacing * (tabCount - 1)) / tabCount;
                float rowWidth = tabWidth * tabCount + tabSpacing * (tabCount - 1);
                float sectionBodyHeight = tabHeight + 4f * scale;

                if (ImGui.BeginChild("##hudPresetTabs", new Vector2(innerWidth, sectionBodyHeight), false, ImGuiWindowFlags.NoScrollbar))
                {
                    float childWidth = ImGui.GetContentRegionAvail().X;
                    float childHeight = ImGui.GetContentRegionAvail().Y;
                    float rowOffsetX = Math.Max(0f, (childWidth - rowWidth) * 0.5f);
                    float rowOffsetY = Math.Max(0f, (childHeight - tabHeight) * 0.5f);

                    ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPosX() + rowOffsetX, ImGui.GetCursorPosY() + rowOffsetY));

                    ImGui.BeginGroup();
                    bool hudLayoutSettingsChanged = false;
                    for (int i = 0; i < tabCount; i++)
                    {
                        if (i > 0)
                        {
                            ImGui.SameLine(0f, tabSpacing);
                        }

                        bool attachHudEnabled = false;
                        int hudLayout = 0;
                        if (hudLayoutSettings != null && i < 4)
                        {
                            PresetHudLayoutBinding binding = hudLayoutSettings.GetBinding((AetherPreset)i);
                            attachHudEnabled = binding.AttachHudEnabled;
                            hudLayout = binding.HudLayout;
                        }

                        PresetTabInteraction interaction = HomePresetTabBar.DrawTab(
                            i,
                            _pendingTabIndex,
                            tabWidth,
                            tabHeight,
                            attachHudEnabled,
                            hudLayout);

                        if (interaction == PresetTabInteraction.GearClicked)
                        {
                            PresetHudLayoutPopup.Open((AetherPreset)i);
                            continue;
                        }

                        if (interaction != PresetTabInteraction.TabClicked)
                        {
                            continue;
                        }

                        if (i == 4)
                        {
                            _pendingTabIndex = 4;
                            continue;
                        }

                        _confirmTabIndex = i;
                        _confirmRequested = true;
                    }

                    ImGui.EndGroup();
                    PresetHudLayoutPopup.Draw(ref hudLayoutSettingsChanged);
                    if (hudLayoutSettingsChanged)
                    {
                        ConfigurationManager.Instance.ForceNeedsSave();
                    }
                }

                ImGui.EndChild();
            }

            if (_applyErrorMessage != null)
            {
                if (ImGuiHelper.DrawErrorModal(_applyErrorMessage))
                {
                    _applyErrorMessage = null;
                }
            }

            if (_confirmRequested)
            {
                string presetName = HomePresetTabBar.GetTabLabel(_confirmTabIndex);
                string[] lines =
                {
                    $"Apply {presetName}?",
                    "This will replace your current HUD settings.",
                    "Your saved profiles will not be changed."
                };

                var (didConfirm, didClosed) = ImGuiHelper.DrawConfirmationModal($"Apply {presetName}?", lines);

                if (didConfirm)
                {
                    if (!Apply((AetherPreset)_confirmTabIndex))
                    {
                        _applyErrorMessage = "Failed to apply preset. Check the Dalamud log for details.";
                    }
                    else
                    {
                        _pendingTabIndex = _confirmTabIndex;
                    }
                }

                if (didConfirm || didClosed)
                {
                    _confirmRequested = false;
                    _confirmTabIndex = -1;
                }
            }
        }

        private static void SyncPendingFromActive(HomeFeatureSettingsConfig settings)
        {
            _pendingTabIndex = settings.ActivePreset == ActivePresetSelection.Custom
                ? 4
                : (int)settings.ActivePreset;
        }

        public static bool Apply(AetherPreset preset)
        {
            _suppressCustomPresetMark = true;

            try
            {
                string? importString = LoadPresetString(preset);
                if (importString == null)
                {
                    Plugin.Logger.Error($"Failed to load preset data for {preset}.");
                    return false;
                }

                if (!ConfigurationManager.Instance.ApplyPresetImport(importString))
                {
                    return false;
                }

                switch (preset)
                {
                    case AetherPreset.Minimal:
                        ApplyMinimalHudLayout();
                        ApplyMinimalMiscSettings();
                        ApplyMinimalJobBarPositions();
                        ApplyMinimalBuffDebuffSettings();
                        break;
                    case AetherPreset.MmoModern:
                        break;
                    case AetherPreset.RaidFocused:
                        break;
                    case AetherPreset.ActionCombat:
                        break;
                }

                HomeFeatureSettingsConfig? settings = ConfigurationManager.Instance.GetConfigObject<HomeFeatureSettingsConfig>();
                if (settings != null)
                {
                    FeatureRegistry.SyncHomeSettingsFromConfigs(ConfigurationManager.Instance.ConfigBaseNode, settings);
                    ApplyPresetHomePillOverrides(preset, settings);

                    settings.ActivePreset = (ActivePresetSelection)preset;
                    _pendingTabIndex = (int)preset;
                }

                // Preset import replaces FontsConfig; rebuild handles so FontID indices match the new registry.
                FontsManager.Instance.BuildFonts();

                ConfigurationManager.Instance.SaveConfigurations(forced: true, saveProfile: false);

                PresetHudLayoutSettingsConfig? hudLayoutSettings =
                    ConfigurationManager.Instance.GetConfigObject<PresetHudLayoutSettingsConfig>();
                if (hudLayoutSettings != null)
                {
                    PresetHudLayoutBinding binding = hudLayoutSettings.GetBinding(preset);
                    NativeHudLayoutHelper.ApplyAttachedHudLayout(binding.AttachHudEnabled, binding.HudLayout);
                }

                return true;
            }
            finally
            {
                _suppressCustomPresetMark = false;
            }
        }

        private static string? LoadPresetString(AetherPreset preset)
        {
            string fileName = preset switch
            {
                AetherPreset.Minimal => "Minimal.delvui",
                AetherPreset.MmoModern => "MmoModern.delvui",
                AetherPreset.RaidFocused => "RaidFocused.delvui",
                AetherPreset.ActionCombat => "ActionCombat.delvui",
                _ => "Default.delvui"
            };

            string path = Path.Combine(Plugin.AssemblyLocation, "Media", "Presets", fileName);
            if (!File.Exists(path))
            {
                path = Path.Combine(Plugin.AssemblyLocation, "Media", "Profiles", fileName);
            }

            if (!File.Exists(path))
            {
                return null;
            }

            return File.ReadAllText(path);
        }

        private static void ApplyMinimalHudLayout()
        {
            PlayerParameterOrbConfig? orb =
                ConfigurationManager.Instance.GetConfigObject<PlayerParameterOrbConfig>();
            if (orb != null)
            {
                orb.Position = new Vector2(-730f, 564f);
            }

            MinimapConfig? minimap = ConfigurationManager.Instance.GetConfigObject<MinimapConfig>();
            if (minimap != null)
            {
                minimap.Position = new Vector2(729.5f, 563.5f);
            }

            PartyFramesConfig? partyFrames =
                ConfigurationManager.Instance.GetConfigObject<PartyFramesConfig>();
            if (partyFrames != null)
            {
                partyFrames.Position = new Vector2(-1230f, 375f);
            }

            EnemyListConfig? enemyList =
                ConfigurationManager.Instance.GetConfigObject<EnemyListConfig>();
            if (enemyList != null)
            {
                enemyList.Position = new Vector2(1005f, 395f);
            }
        }

        private static void ApplyMinimalMiscSettings()
        {
            HUDOptionsConfig? hudOptions = ConfigurationManager.Instance.GetConfigObject<HUDOptionsConfig>();
            if (hudOptions != null)
            {
                hudOptions.HideDefaultJobGauges = true;
            }
        }

        private static readonly Type[] MinimalJobBarConfigTypes =
        {
            typeof(PaladinConfig),
            typeof(WarriorConfig),
            typeof(DarkKnightConfig),
            typeof(GunbreakerConfig),
            typeof(WhiteMageConfig),
            typeof(ScholarConfig),
            typeof(AstrologianConfig),
            typeof(SageConfig),
            typeof(MonkConfig),
            typeof(DragoonConfig),
            typeof(NinjaConfig),
            typeof(SamuraiConfig),
            typeof(ReaperConfig),
            typeof(ViperConfig),
            typeof(BardConfig),
            typeof(MachinistConfig),
            typeof(DancerConfig),
            typeof(BlackMageConfig),
            typeof(SummonerConfig),
            typeof(RedMageConfig),
            typeof(BlueMageConfig),
            typeof(PictomancerConfig),
        };

        private static void ApplyMinimalJobBarPositions()
        {
            foreach (Type type in MinimalJobBarConfigTypes)
            {
                PluginConfigObject? config = ConfigurationManager.Instance.GetConfigObjectForType(type);
                if (config is JobConfig jobConfig)
                {
                    jobConfig.Position = new Vector2(0f, 499.5f);
                }
            }
        }

        private static void ApplyMinimalBuffDebuffSettings()
        {
            SetStatusEffectsList<PlayerBuffsListConfig>(enabled: true, x: -450f, y: 500f);
            SetStatusEffectsList<PlayerDebuffsListConfig>(enabled: true, x: 450f, y: 500f);

            SetStatusEffectsList<TargetBuffsListConfig>(enabled: false);
            SetStatusEffectsList<TargetDebuffsListConfig>(enabled: false);
            SetStatusEffectsList<FocusTargetBuffsListConfig>(enabled: false);
            SetStatusEffectsList<FocusTargetDebuffsListConfig>(enabled: false);
            SetStatusEffectsList<CustomEffectsListConfig>(enabled: false);
        }

        private static void SetStatusEffectsList<T>(bool enabled, float x = 0f, float y = 0f)
            where T : StatusEffectsListConfig
        {
            T? config = ConfigurationManager.Instance.GetConfigObject<T>();
            if (config == null)
            {
                return;
            }

            if (config.Disableable)
            {
                config.Enabled = enabled;
            }

            if (enabled)
            {
                config.Position = new Vector2(x, y);
            }
        }

        private static void ApplyPresetHomePillOverrides(AetherPreset preset, HomeFeatureSettingsConfig settings)
        {
            switch (preset)
            {
                case AetherPreset.Minimal:
                    // Shipped Minimal.delvui is the source of truth; pills mirror maintainer Home toggles only.
                    settings.PlayerParameterOrb = true;
                    settings.IndividualFramesMaster = false;
                    settings.UnitFrames = false;
                    settings.ManaBars = false;
                    settings.CastBars = false;
                    settings.BuffsAndDebuffs = true;
                    settings.OtherElements = false;
                    settings.ExperienceBar = false;
                    settings.GcdIndicator = false;
                    settings.PullTimer = false;
                    settings.LimitBreak = false;
                    settings.MpTicker = false;
                    settings.Nameplates = true;
                    settings.PartyFrames = true;
                    settings.PartyCooldowns = false;
                    settings.EnemyList = true;
                    settings.JobSpecificBars = true;
                    settings.Minimap = true;
                    settings.ActionCamera = false;
                    break;
                case AetherPreset.MmoModern:
                    // Shipped MmoModern.delvui is the source of truth; pills mirror maintainer Home toggles only.
                    settings.PlayerParameterOrb = false;
                    settings.IndividualFramesMaster = true;
                    settings.UnitFrames = true;
                    settings.ManaBars = true;
                    settings.CastBars = true;
                    settings.BuffsAndDebuffs = true;
                    settings.OtherElements = true;
                    settings.ExperienceBar = false;
                    settings.GcdIndicator = false;
                    settings.PullTimer = false;
                    settings.LimitBreak = true;
                    settings.MpTicker = false;
                    settings.Nameplates = true;
                    settings.PartyFrames = true;
                    settings.PartyCooldowns = false;
                    settings.EnemyList = true;
                    settings.JobSpecificBars = false;
                    settings.Minimap = true;
                    settings.ActionCamera = false;
                    break;
                case AetherPreset.RaidFocused:
                    // Shipped RaidFocused.delvui is the source of truth; pills mirror maintainer Home toggles only.
                    settings.PlayerParameterOrb = false;
                    settings.IndividualFramesMaster = true;
                    settings.UnitFrames = true;
                    settings.ManaBars = true;
                    settings.CastBars = true;
                    settings.BuffsAndDebuffs = true;
                    settings.OtherElements = true;
                    settings.ExperienceBar = false;
                    settings.GcdIndicator = false;
                    settings.PullTimer = true;
                    settings.LimitBreak = true;
                    settings.MpTicker = false;
                    settings.Nameplates = true;
                    settings.PartyFrames = true;
                    settings.PartyCooldowns = true;
                    settings.EnemyList = true;
                    settings.JobSpecificBars = true;
                    settings.Minimap = true;
                    settings.ActionCamera = false;
                    break;
                case AetherPreset.ActionCombat:
                    // Shipped ActionCombat.delvui is the source of truth; pills mirror maintainer Home toggles only.
                    settings.PlayerParameterOrb = true;
                    settings.IndividualFramesMaster = false;
                    settings.UnitFrames = false;
                    settings.ManaBars = false;
                    settings.CastBars = false;
                    settings.BuffsAndDebuffs = true;
                    settings.OtherElements = false;
                    settings.ExperienceBar = false;
                    settings.GcdIndicator = false;
                    settings.PullTimer = false;
                    settings.LimitBreak = false;
                    settings.MpTicker = false;
                    settings.Nameplates = true;
                    settings.PartyFrames = true;
                    settings.PartyCooldowns = false;
                    settings.EnemyList = true;
                    settings.JobSpecificBars = false;
                    settings.Minimap = true;
                    settings.ActionCamera = true;
                    break;
            }
        }
    }
}
