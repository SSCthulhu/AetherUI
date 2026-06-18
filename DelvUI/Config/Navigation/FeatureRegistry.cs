using DelvUI.Config;
using DelvUI.Config.Home;
using DelvUI.Config.Navigation;
using DelvUI.Config.Tree;
using DelvUI.Interface.ActionCamera;
using DelvUI.Interface.EnemyList;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.Jobs;
using DelvUI.Interface.Party;
using DelvUI.Interface.PartyCooldowns;
using DelvUI.Interface.StatusEffects;
using System;
using System.Collections.Generic;

namespace DelvUI.Config.Navigation
{
    public static class FeatureRegistry
    {
        private static readonly Dictionary<FeatureId, string> SectionNames = new()
        {
            [FeatureId.PlayerParameterOrb] = "Player Parameter Orb",
            [FeatureId.UnitFrames] = "Unit Frames",
            [FeatureId.ManaBars] = "Mana Bars",
            [FeatureId.CastBars] = "Castbars",
            [FeatureId.BuffsAndDebuffs] = "Buffs and Debuffs",
            [FeatureId.OtherElements] = "Other Elements",
            [FeatureId.Nameplates] = "Nameplates",
            [FeatureId.PartyFrames] = "Party Frames",
            [FeatureId.PartyCooldowns] = "Party Cooldowns",
            [FeatureId.EnemyList] = "Enemy List",
            [FeatureId.JobSpecificBars] = "Job Specific Bars",
            [FeatureId.Minimap] = "Minimap",
            [FeatureId.ActionCamera] = "Action Camera"
        };

        private static readonly Dictionary<FeatureId, Type[]> ConfigTypes = new()
        {
            [FeatureId.PlayerParameterOrb] = new[] { typeof(PlayerParameterOrbConfig) },
            [FeatureId.UnitFrames] = new[]
            {
                typeof(PlayerUnitFrameConfig),
                typeof(TargetUnitFrameConfig),
                typeof(TargetOfTargetUnitFrameConfig),
                typeof(FocusTargetUnitFrameConfig)
            },
            [FeatureId.ManaBars] = new[]
            {
                typeof(PlayerPrimaryResourceConfig),
                typeof(TargetPrimaryResourceConfig),
                typeof(TargetOfTargetPrimaryResourceConfig),
                typeof(FocusTargetPrimaryResourceConfig)
            },
            [FeatureId.CastBars] = new[]
            {
                typeof(PlayerCastbarConfig),
                typeof(TargetCastbarConfig),
                typeof(TargetOfTargetCastbarConfig),
                typeof(FocusTargetCastbarConfig)
            },
            [FeatureId.BuffsAndDebuffs] = new[]
            {
                typeof(PlayerBuffsListConfig),
                typeof(PlayerDebuffsListConfig),
                typeof(TargetBuffsListConfig),
                typeof(TargetDebuffsListConfig),
                typeof(FocusTargetBuffsListConfig),
                typeof(FocusTargetDebuffsListConfig),
                typeof(CustomEffectsListConfig)
            },
            [FeatureId.OtherElements] = new[]
            {
                typeof(ExperienceBarConfig),
                typeof(GCDIndicatorConfig),
                typeof(PullTimerConfig),
                typeof(LimitBreakConfig),
                typeof(MPTickerConfig)
            },
            [FeatureId.Nameplates] = new[]
            {
                typeof(NameplatesGeneralConfig),
                typeof(PlayerNameplateConfig),
                typeof(EnemyNameplateConfig),
                typeof(PartyMembersNameplateConfig),
                typeof(AllianceMembersNameplateConfig),
                typeof(FriendPlayerNameplateConfig),
                typeof(OtherPlayerNameplateConfig),
                typeof(PetNameplateConfig),
                typeof(NPCNameplateConfig),
                typeof(MinionNPCNameplateConfig),
                typeof(ObjectsNameplateConfig)
            },
            [FeatureId.PartyFrames] = new[]
            {
                typeof(PartyFramesConfig),
                typeof(PartyFramesHealthBarsConfig),
                typeof(PartyFramesManaBarConfig),
                typeof(PartyFramesCastbarConfig),
                typeof(PartyFramesIconsConfig),
                typeof(PartyFramesBuffsConfig),
                typeof(PartyFramesDebuffsConfig),
                typeof(PartyFramesTrackersConfig),
                typeof(PartyFramesCooldownListConfig)
            },
            [FeatureId.PartyCooldowns] = new[]
            {
                typeof(PartyCooldownsConfig),
                typeof(PartyCooldownsBarConfig),
                typeof(PartyCooldownsDataConfig)
            },
            [FeatureId.EnemyList] = new[]
            {
                typeof(EnemyListConfig),
                typeof(EnemyListHealthBarConfig),
                typeof(EnemyListEnmityIconConfig),
                typeof(EnemyListSignIconConfig),
                typeof(EnemyListCastbarConfig),
                typeof(EnemyListBuffsConfig),
                typeof(EnemyListDebuffsConfig)
            },
            [FeatureId.JobSpecificBars] = new[] { typeof(JobBarsGeneralConfig) },
            [FeatureId.Minimap] = new[] { typeof(MinimapConfig) },
            [FeatureId.ActionCamera] = new[] { typeof(ActionCameraConfig) }
        };

        // Home master toggles reflect root HUD configs only; sub-section Enabled flags stay independent.
        private static readonly Dictionary<FeatureId, Type> MasterConfigTypes = new()
        {
            [FeatureId.PlayerParameterOrb] = typeof(PlayerParameterOrbConfig),
            [FeatureId.Nameplates] = typeof(NameplatesGeneralConfig),
            [FeatureId.PartyFrames] = typeof(PartyFramesConfig),
            [FeatureId.PartyCooldowns] = typeof(PartyCooldownsConfig),
            [FeatureId.EnemyList] = typeof(EnemyListConfig),
            [FeatureId.JobSpecificBars] = typeof(JobBarsGeneralConfig),
            [FeatureId.Minimap] = typeof(MinimapConfig),
            [FeatureId.ActionCamera] = typeof(ActionCameraConfig)
        };

        public static string GetSectionName(FeatureId featureId) => SectionNames[featureId];

        public static bool IsNavExcludedSection(string sectionName)
        {
            return Array.Exists(NavigationConstants.NavExcludedSections, s => s == sectionName);
        }

        public static bool IsAdvancedSection(string sectionName)
        {
            return Array.Exists(NavigationConstants.AdvancedSections, s => s == sectionName);
        }

        public static bool IsFeatureSectionVisible(HomeFeatureSettingsConfig settings, string sectionName)
        {
            foreach (KeyValuePair<FeatureId, string> entry in SectionNames)
            {
                if (entry.Value != sectionName)
                {
                    continue;
                }

                return IsFeatureEnabled(settings, entry.Key);
            }

            return false;
        }

        public static bool IsFeatureEnabled(HomeFeatureSettingsConfig settings, FeatureId featureId)
        {
            return featureId switch
            {
                FeatureId.PlayerParameterOrb => settings.PlayerParameterOrb,
                FeatureId.UnitFrames => settings.IndividualFramesMaster && settings.UnitFrames,
                FeatureId.ManaBars => settings.IndividualFramesMaster && settings.ManaBars,
                FeatureId.CastBars => settings.IndividualFramesMaster && settings.CastBars,
                FeatureId.BuffsAndDebuffs => settings.BuffsAndDebuffs,
                FeatureId.OtherElements => IsAnyOtherElementEnabled(settings),
                FeatureId.Nameplates => settings.Nameplates,
                FeatureId.PartyFrames => settings.PartyFrames,
                FeatureId.PartyCooldowns => settings.PartyCooldowns,
                FeatureId.EnemyList => settings.EnemyList,
                FeatureId.JobSpecificBars => settings.JobSpecificBars,
                FeatureId.Minimap => settings.Minimap,
                FeatureId.ActionCamera => settings.ActionCamera,
                _ => true
            };
        }

        public static void SetFeatureEnabled(HomeFeatureSettingsConfig settings, FeatureId featureId, bool enabled)
        {
            switch (featureId)
            {
                case FeatureId.PlayerParameterOrb:
                    settings.PlayerParameterOrb = enabled;
                    break;
                case FeatureId.UnitFrames:
                    settings.UnitFrames = enabled;
                    if (enabled)
                    {
                        settings.IndividualFramesMaster = true;
                    }

                    break;
                case FeatureId.ManaBars:
                    settings.ManaBars = enabled;
                    if (enabled)
                    {
                        settings.IndividualFramesMaster = true;
                    }

                    break;
                case FeatureId.CastBars:
                    settings.CastBars = enabled;
                    if (enabled)
                    {
                        settings.IndividualFramesMaster = true;
                    }

                    break;
                case FeatureId.BuffsAndDebuffs:
                    settings.BuffsAndDebuffs = enabled;
                    break;
                case FeatureId.OtherElements:
                    settings.OtherElements = enabled;
                    if (!enabled)
                    {
                        settings.ExperienceBar = false;
                        settings.GcdIndicator = false;
                        settings.PullTimer = false;
                        settings.LimitBreak = false;
                        settings.MpTicker = false;
                    }

                    break;
                case FeatureId.Nameplates:
                    settings.Nameplates = enabled;
                    break;
                case FeatureId.PartyFrames:
                    settings.PartyFrames = enabled;
                    break;
                case FeatureId.PartyCooldowns:
                    settings.PartyCooldowns = enabled;
                    break;
                case FeatureId.EnemyList:
                    settings.EnemyList = enabled;
                    break;
                case FeatureId.JobSpecificBars:
                    settings.JobSpecificBars = enabled;
                    break;
                case FeatureId.Minimap:
                    settings.Minimap = enabled;
                    break;
                case FeatureId.ActionCamera:
                    settings.ActionCamera = enabled;
                    break;
            }
        }

        public static void SetIndividualFramesMaster(HomeFeatureSettingsConfig settings, bool enabled)
        {
            settings.IndividualFramesMaster = enabled;

            if (!enabled)
            {
                settings.UnitFrames = false;
                settings.ManaBars = false;
                settings.CastBars = false;
            }
            else
            {
                settings.UnitFrames = true;
                settings.ManaBars = true;
                settings.CastBars = true;
            }
        }

        public static void ApplyHomeSettingsToConfigs(
            BaseNode node,
            HomeFeatureSettingsConfig settings,
            HashSet<FeatureId>? onlyFeatures = null)
        {
            foreach (KeyValuePair<FeatureId, Type[]> entry in ConfigTypes)
            {
                if (onlyFeatures != null && !onlyFeatures.Contains(entry.Key))
                {
                    continue;
                }

                if (entry.Key == FeatureId.OtherElements)
                {
                    ApplyOtherElementsToConfigs(node, settings);
                    continue;
                }

                ApplyFeatureToConfigs(node, settings, entry.Key, entry.Value);
            }
        }

        private static void ApplyFeatureToConfigs(
            BaseNode node,
            HomeFeatureSettingsConfig settings,
            FeatureId featureId,
            Type[] configTypes)
        {
            bool enabled = IsFeatureEnabled(settings, featureId);

            foreach (Type configType in configTypes)
            {
                PluginConfigObject? config = GetConfigObjectForType(node, configType);
                if (config == null || !config.Disableable)
                {
                    continue;
                }

                config.Enabled = enabled;
            }
        }

        private static void ApplyOtherElementsToConfigs(BaseNode node, HomeFeatureSettingsConfig settings)
        {
            SetOtherElementConfigEnabled<ExperienceBarConfig>(node, settings, settings.ExperienceBar);
            SetOtherElementConfigEnabled<GCDIndicatorConfig>(node, settings, settings.GcdIndicator);
            SetOtherElementConfigEnabled<PullTimerConfig>(node, settings, settings.PullTimer);
            SetOtherElementConfigEnabled<LimitBreakConfig>(node, settings, settings.LimitBreak);
            SetOtherElementConfigEnabled<MPTickerConfig>(node, settings, settings.MpTicker);
        }

        private static void SetOtherElementConfigEnabled<T>(BaseNode node, HomeFeatureSettingsConfig settings, bool subEnabled)
            where T : PluginConfigObject
        {
            PluginConfigObject? config = GetConfigObjectForType(node, typeof(T));
            if (config == null || !config.Disableable)
            {
                return;
            }

            config.Enabled = subEnabled;
        }

        public static bool IsAnyOtherElementEnabled(HomeFeatureSettingsConfig settings)
        {
            return settings.ExperienceBar
                || settings.GcdIndicator
                || settings.PullTimer
                || settings.LimitBreak
                || settings.MpTicker;
        }

        public static void SyncOtherElementsMaster(HomeFeatureSettingsConfig settings)
        {
            settings.OtherElements = IsAnyOtherElementEnabled(settings);
        }

        private static void SyncOtherElementSubsFromConfigs(BaseNode node, HomeFeatureSettingsConfig settings)
        {
            settings.ExperienceBar = GetConfigEnabled<ExperienceBarConfig>(node);
            settings.GcdIndicator = GetConfigEnabled<GCDIndicatorConfig>(node);
            settings.PullTimer = GetConfigEnabled<PullTimerConfig>(node);
            settings.LimitBreak = GetConfigEnabled<LimitBreakConfig>(node);
            settings.MpTicker = GetConfigEnabled<MPTickerConfig>(node);
        }

        private static bool GetConfigEnabled<T>(BaseNode node) where T : PluginConfigObject
        {
            PluginConfigObject? config = GetConfigObjectForType(node, typeof(T));
            return config == null || config.Enabled;
        }

        private static bool GetFeatureEnabledFromConfigs(BaseNode node, FeatureId featureId, Type[] configTypes)
        {
            if (MasterConfigTypes.TryGetValue(featureId, out Type? masterType))
            {
                PluginConfigObject? masterConfig = GetConfigObjectForType(node, masterType);
                return masterConfig == null || masterConfig.Enabled;
            }

            if (featureId == FeatureId.BuffsAndDebuffs)
            {
                return IsAnyBuffDebuffListEnabled(node);
            }

            foreach (Type configType in configTypes)
            {
                PluginConfigObject? config = GetConfigObjectForType(node, configType);
                if (config == null || !config.Disableable)
                {
                    continue;
                }

                if (!config.Enabled)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsAnyBuffDebuffListEnabled(BaseNode node)
        {
            return IsConfigEnabled<PlayerBuffsListConfig>(node)
                || IsConfigEnabled<PlayerDebuffsListConfig>(node)
                || IsConfigEnabled<TargetBuffsListConfig>(node)
                || IsConfigEnabled<TargetDebuffsListConfig>(node)
                || IsConfigEnabled<FocusTargetBuffsListConfig>(node)
                || IsConfigEnabled<FocusTargetDebuffsListConfig>(node)
                || IsConfigEnabled<CustomEffectsListConfig>(node);
        }

        private static bool IsConfigEnabled<T>(BaseNode node) where T : PluginConfigObject
        {
            T? config = node.GetConfigObject<T>();
            return config != null && config.Enabled;
        }

        public static bool SyncHomeSettingsFromConfigs(BaseNode node, HomeFeatureSettingsConfig settings)
        {
            bool changed = false;

            foreach (KeyValuePair<FeatureId, Type[]> entry in ConfigTypes)
            {
                if (entry.Key == FeatureId.OtherElements)
                {
                    SyncOtherElementSubsFromConfigs(node, settings);
                    SyncOtherElementsMaster(settings);
                    continue;
                }

                bool enabled = GetFeatureEnabledFromConfigs(node, entry.Key, entry.Value);

                changed |= AssignIfDifferent(entry.Key, settings, enabled);
            }

            bool individualFramesMaster = settings.UnitFrames || settings.ManaBars || settings.CastBars;
            if (settings.IndividualFramesMaster != individualFramesMaster)
            {
                settings.IndividualFramesMaster = individualFramesMaster;
                changed = true;
            }

            return changed;
        }

        private static bool AssignIfDifferent(FeatureId featureId, HomeFeatureSettingsConfig settings, bool enabled)
        {
            switch (featureId)
            {
                case FeatureId.PlayerParameterOrb when settings.PlayerParameterOrb != enabled:
                    settings.PlayerParameterOrb = enabled;
                    return true;
                case FeatureId.UnitFrames when settings.UnitFrames != enabled:
                    settings.UnitFrames = enabled;
                    return true;
                case FeatureId.ManaBars when settings.ManaBars != enabled:
                    settings.ManaBars = enabled;
                    return true;
                case FeatureId.CastBars when settings.CastBars != enabled:
                    settings.CastBars = enabled;
                    return true;
                case FeatureId.BuffsAndDebuffs when settings.BuffsAndDebuffs != enabled:
                    settings.BuffsAndDebuffs = enabled;
                    return true;
                case FeatureId.Nameplates when settings.Nameplates != enabled:
                    settings.Nameplates = enabled;
                    return true;
                case FeatureId.PartyFrames when settings.PartyFrames != enabled:
                    settings.PartyFrames = enabled;
                    return true;
                case FeatureId.PartyCooldowns when settings.PartyCooldowns != enabled:
                    settings.PartyCooldowns = enabled;
                    return true;
                case FeatureId.EnemyList when settings.EnemyList != enabled:
                    settings.EnemyList = enabled;
                    return true;
                case FeatureId.JobSpecificBars when settings.JobSpecificBars != enabled:
                    settings.JobSpecificBars = enabled;
                    return true;
                case FeatureId.Minimap when settings.Minimap != enabled:
                    settings.Minimap = enabled;
                    return true;
                case FeatureId.ActionCamera when settings.ActionCamera != enabled:
                    settings.ActionCamera = enabled;
                    return true;
                default:
                    return false;
            }
        }

        private static PluginConfigObject? GetConfigObjectForType(BaseNode node, Type type)
        {
            return ConfigurationManager.Instance.GetConfigObjectForType(type);
        }
    }
}
