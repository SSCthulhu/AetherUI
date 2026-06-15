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
using System.Reflection;

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

        public static void ApplyHomeSettingsToConfigs(BaseNode node, HomeFeatureSettingsConfig settings)
        {
            foreach (KeyValuePair<FeatureId, Type[]> entry in ConfigTypes)
            {
                if (entry.Key == FeatureId.OtherElements)
                {
                    ApplyOtherElementsToConfigs(node, settings);
                    continue;
                }

                bool enabled = IsFeatureEnabled(settings, entry.Key);

                foreach (Type configType in entry.Value)
                {
                    PluginConfigObject? config = node.GetConfigObjectForType(configType);
                    if (config == null || !config.Disableable)
                    {
                        continue;
                    }

                    config.Enabled = enabled;
                }
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
            PluginConfigObject? config = node.GetConfigObjectForType(typeof(T));
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
            PluginConfigObject? config = node.GetConfigObjectForType(typeof(T));
            return config == null || config.Enabled;
        }

        public static void SyncHomeSettingsFromConfigs(BaseNode node, HomeFeatureSettingsConfig settings)
        {
            foreach (KeyValuePair<FeatureId, Type[]> entry in ConfigTypes)
            {
                if (entry.Key == FeatureId.OtherElements)
                {
                    SyncOtherElementSubsFromConfigs(node, settings);
                    SyncOtherElementsMaster(settings);
                    continue;
                }

                bool enabled = true;

                foreach (Type configType in entry.Value)
                {
                    PluginConfigObject? config = node.GetConfigObjectForType(configType);
                    if (config == null || !config.Disableable)
                    {
                        continue;
                    }

                    if (!config.Enabled)
                    {
                        enabled = false;
                        break;
                    }
                }

                switch (entry.Key)
                {
                    case FeatureId.PlayerParameterOrb:
                        settings.PlayerParameterOrb = enabled;
                        break;
                    case FeatureId.UnitFrames:
                        settings.UnitFrames = enabled;
                        break;
                    case FeatureId.ManaBars:
                        settings.ManaBars = enabled;
                        break;
                    case FeatureId.CastBars:
                        settings.CastBars = enabled;
                        break;
                    case FeatureId.BuffsAndDebuffs:
                        settings.BuffsAndDebuffs = enabled;
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

            settings.IndividualFramesMaster = settings.UnitFrames || settings.ManaBars || settings.CastBars;
        }

        private static PluginConfigObject? GetConfigObjectForType(this BaseNode node, Type type)
        {
            MethodInfo? genericMethod = node.GetType().GetMethod("GetConfigObject");
            MethodInfo? method = genericMethod?.MakeGenericMethod(type);
            return (PluginConfigObject?)method?.Invoke(node, null);
        }
    }
}
