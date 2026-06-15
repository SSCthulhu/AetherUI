using DelvUI.Config;
using DelvUI.Config.Home;
using DelvUI.Config.Home.Widgets;
using DelvUI.Config.Navigation;
using DelvUI.Helpers;
using DelvUI.Interface.ActionCamera;
using DelvUI.Interface.EnemyList;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.Jobs;
using DelvUI.Interface.Party;
using DelvUI.Interface.PartyCooldowns;
using DelvUI.Interface.StatusEffects;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using System;
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
                    for (int i = 0; i < tabCount; i++)
                    {
                        if (i > 0)
                        {
                            ImGui.SameLine(0f, tabSpacing);
                        }

                        if (!HomePresetTabBar.DrawTab(i, _pendingTabIndex, tabWidth, tabHeight))
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

                var (didConfirm, didClose) = ImGuiHelper.DrawConfirmationModal($"Apply {presetName}?", lines);

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

                if (didConfirm || didClose)
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
                        ApplyMinimalAdjustments();
                        break;
                    case AetherPreset.MmoModern:
                        ApplyMmoModernAdjustments();
                        break;
                    case AetherPreset.RaidFocused:
                        ApplyRaidFocusedAdjustments();
                        break;
                    case AetherPreset.ActionCombat:
                        ApplyActionCombatAdjustments();
                        break;
                }

                HomeFeatureSettingsConfig? settings = ConfigurationManager.Instance.GetConfigObject<HomeFeatureSettingsConfig>();
                if (settings != null)
                {
                    if (preset != AetherPreset.ActionCombat)
                    {
                        FeatureRegistry.SyncHomeSettingsFromConfigs(ConfigurationManager.Instance.ConfigBaseNode, settings);
                    }

                    settings.ActivePreset = (ActivePresetSelection)preset;
                    _pendingTabIndex = (int)preset;
                }

                ConfigurationManager.Instance.SaveConfigurations(forced: true, saveProfile: false);
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

        private static void ApplyMinimalAdjustments()
        {
            SetFeatureEnabled(false,
                FeatureId.OtherElements,
                FeatureId.Nameplates,
                FeatureId.PartyFrames,
                FeatureId.PartyCooldowns,
                FeatureId.EnemyList,
                FeatureId.JobSpecificBars,
                FeatureId.Minimap,
                FeatureId.ActionCamera,
                FeatureId.BuffsAndDebuffs);

            SetFeatureEnabled(true, FeatureId.PlayerParameterOrb);

            HomeFeatureSettingsConfig? settings = ConfigurationManager.Instance.GetConfigObject<HomeFeatureSettingsConfig>();
            if (settings != null)
            {
                FeatureRegistry.SetIndividualFramesMaster(settings, true);
                FeatureRegistry.SetFeatureEnabled(settings, FeatureId.UnitFrames, true);
                FeatureRegistry.SetFeatureEnabled(settings, FeatureId.ManaBars, true);
                FeatureRegistry.SetFeatureEnabled(settings, FeatureId.CastBars, true);
                FeatureRegistry.SetFeatureEnabled(settings, FeatureId.BuffsAndDebuffs, false);
                FeatureRegistry.ApplyHomeSettingsToConfigs(ConfigurationManager.Instance.ConfigBaseNode, settings);
            }

            DisableConfigs(
                typeof(TargetUnitFrameConfig),
                typeof(TargetOfTargetUnitFrameConfig),
                typeof(FocusTargetUnitFrameConfig),
                typeof(TargetPrimaryResourceConfig),
                typeof(TargetOfTargetPrimaryResourceConfig),
                typeof(FocusTargetPrimaryResourceConfig),
                typeof(TargetCastbarConfig),
                typeof(TargetOfTargetCastbarConfig),
                typeof(FocusTargetCastbarConfig));
        }

        private static void ApplyMmoModernAdjustments()
        {
            SetFeatureEnabled(true,
                FeatureId.PlayerParameterOrb,
                FeatureId.UnitFrames,
                FeatureId.ManaBars,
                FeatureId.CastBars,
                FeatureId.BuffsAndDebuffs,
                FeatureId.Nameplates,
                FeatureId.PartyFrames,
                FeatureId.Minimap);

            SetFeatureEnabled(false,
                FeatureId.PartyCooldowns,
                FeatureId.EnemyList,
                FeatureId.ActionCamera);

            SetFeatureEnabled(true, FeatureId.JobSpecificBars);
            SetFeatureEnabled(false, FeatureId.OtherElements);

            SyncHomeFromAppliedFeatures();
        }

        private static void ApplyActionCombatAdjustments()
        {
            EnableConfigs(
                typeof(PlayerBuffsListConfig),
                typeof(PlayerDebuffsListConfig),
                typeof(TargetBuffsListConfig),
                typeof(TargetDebuffsListConfig),
                typeof(FocusTargetBuffsListConfig),
                typeof(FocusTargetDebuffsListConfig),
                typeof(CustomEffectsListConfig),
                typeof(ActionCameraConfig));

            DisableConfigs(typeof(JobBarsGeneralConfig));

            SetFeatureEnabled(true, FeatureId.BuffsAndDebuffs, FeatureId.ActionCamera);
            SetFeatureEnabled(false, FeatureId.JobSpecificBars);
        }

        private static void ApplyRaidFocusedAdjustments()
        {
            SetFeatureEnabled(true,
                FeatureId.PlayerParameterOrb,
                FeatureId.UnitFrames,
                FeatureId.ManaBars,
                FeatureId.CastBars,
                FeatureId.BuffsAndDebuffs,
                FeatureId.Nameplates,
                FeatureId.PartyFrames,
                FeatureId.PartyCooldowns,
                FeatureId.EnemyList,
                FeatureId.JobSpecificBars,
                FeatureId.Minimap);

            SetFeatureEnabled(false,
                FeatureId.OtherElements,
                FeatureId.ActionCamera);

            SyncHomeFromAppliedFeatures();
        }

        private static void SyncHomeFromAppliedFeatures()
        {
            HomeFeatureSettingsConfig? settings = ConfigurationManager.Instance.GetConfigObject<HomeFeatureSettingsConfig>();
            if (settings == null)
            {
                return;
            }

            settings.IndividualFramesMaster = true;
            settings.UnitFrames = true;
            settings.ManaBars = true;
            settings.CastBars = true;
            FeatureRegistry.ApplyHomeSettingsToConfigs(ConfigurationManager.Instance.ConfigBaseNode, settings);
        }

        private static void SetFeatureEnabled(bool enabled, params FeatureId[] features)
        {
            HomeFeatureSettingsConfig? settings = ConfigurationManager.Instance.GetConfigObject<HomeFeatureSettingsConfig>();
            if (settings == null)
            {
                return;
            }

            foreach (FeatureId feature in features)
            {
                FeatureRegistry.SetFeatureEnabled(settings, feature, enabled);
            }

            FeatureRegistry.ApplyHomeSettingsToConfigs(ConfigurationManager.Instance.ConfigBaseNode, settings);
        }

        private static void DisableConfigs(params Type[] types)
        {
            foreach (Type type in types)
            {
                PluginConfigObject? config = ConfigurationManager.Instance.GetConfigObjectForType(type);
                if (config != null && config.Disableable)
                {
                    config.Enabled = false;
                }
            }
        }

        private static void EnableConfigs(params Type[] types)
        {
            foreach (Type type in types)
            {
                PluginConfigObject? config = ConfigurationManager.Instance.GetConfigObjectForType(type);
                if (config != null && config.Disableable)
                {
                    config.Enabled = true;
                }
            }
        }
    }
}
