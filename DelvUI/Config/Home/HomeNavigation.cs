using DelvUI.Config.Navigation;
using DelvUI.Config.Tree;
using System;
using System.Collections.Generic;

namespace DelvUI.Config.Home
{
    public static class HomeNavigation
    {
        private readonly record struct NavigationTarget(string SectionName, string SubTabName);

        private static readonly Dictionary<HomeEditTargetId, NavigationTarget> Targets = new()
        {
            [HomeEditTargetId.PlayerParameterOrb] = new("Player Parameter Orb", "Player Parameter Orb"),
            [HomeEditTargetId.BuffsAndDebuffs] = new("Buffs and Debuffs", "Player Buffs"),
            [HomeEditTargetId.JobSpecificBars] = new("Job Specific Bars", "General"),
            [HomeEditTargetId.UnitFrames] = new("Unit Frames", "Player"),
            [HomeEditTargetId.ManaBars] = new("Mana Bars", "Player"),
            [HomeEditTargetId.CastBars] = new("Castbars", "Player"),
            [HomeEditTargetId.ExperienceBar] = new("Other Elements", "Experience Bar"),
            [HomeEditTargetId.GcdIndicator] = new("Other Elements", "GCD Indicator"),
            [HomeEditTargetId.PullTimer] = new("Other Elements", "Pull Timer"),
            [HomeEditTargetId.LimitBreak] = new("Other Elements", "Limit Break"),
            [HomeEditTargetId.MpTicker] = new("Other Elements", "MP Ticker"),
            [HomeEditTargetId.Nameplates] = new("Nameplates", "General"),
            [HomeEditTargetId.PartyFrames] = new("Party Frames", "General"),
            [HomeEditTargetId.PartyCooldowns] = new("Party Cooldowns", "General"),
            [HomeEditTargetId.EnemyList] = new("Enemy List", "General"),
            [HomeEditTargetId.Minimap] = new("Minimap", "General"),
            [HomeEditTargetId.ActionCamera] = new("Action Camera", "General")
        };

        public static HomeEditTargetId GetEditTargetForFeature(FeatureId featureId)
        {
            return featureId switch
            {
                FeatureId.PlayerParameterOrb => HomeEditTargetId.PlayerParameterOrb,
                FeatureId.BuffsAndDebuffs => HomeEditTargetId.BuffsAndDebuffs,
                FeatureId.JobSpecificBars => HomeEditTargetId.JobSpecificBars,
                FeatureId.Nameplates => HomeEditTargetId.Nameplates,
                FeatureId.PartyFrames => HomeEditTargetId.PartyFrames,
                FeatureId.PartyCooldowns => HomeEditTargetId.PartyCooldowns,
                FeatureId.EnemyList => HomeEditTargetId.EnemyList,
                FeatureId.Minimap => HomeEditTargetId.Minimap,
                FeatureId.ActionCamera => HomeEditTargetId.ActionCamera,
                _ => throw new ArgumentOutOfRangeException(nameof(featureId), featureId, "Feature has no Home edit target.")
            };
        }

        public static void NavigateTo(HomeEditTargetId target)
        {
            if (!Targets.TryGetValue(target, out NavigationTarget navTarget))
            {
                return;
            }

            NavigateToSection(navTarget.SectionName, navTarget.SubTabName);
        }

        public static void NavigateToProfilesImport()
        {
            NavigateToSection(NavigationConstants.ProfilesImport);
        }

        public static void NavigateToAdvancedColors()
        {
            NavigateToSection("Colors");
        }

        public static void NavigateToSection(string sectionName, string? subTabName = null)
        {
            BaseNode baseNode = ConfigurationManager.Instance.ConfigBaseNode;
            SectionNode? section = FindSection(baseNode, sectionName);
            if (section == null)
            {
                return;
            }

            baseNode.SelectedOptionName = sectionName;
            baseNode.RefreshSelectedNode();

            if (subTabName != null)
            {
                section.ForceSelectedTabName = subTabName;
            }
        }

        private static SectionNode? FindSection(BaseNode baseNode, string sectionName)
        {
            foreach (Node node in baseNode.Sections)
            {
                if (node is SectionNode sectionNode && sectionNode.Name == sectionName)
                {
                    return sectionNode;
                }
            }

            return null;
        }
    }
}
