using System;

namespace DelvUI.Config.Navigation
{
    public static class FeatureNavGroups
    {
        public const string PlayerTarget = "Player / Target";
        public const string PartyRaid = "Party / Raid";
        public const string IndividualFrames = "Individual Frames";

        public static readonly string[] PlayerTargetAfterFramesSections =
        {
            "Buffs and Debuffs",
            "Job Specific Bars",
            "Other Elements"
        };

        public static readonly string[] IndividualFrameSections =
        {
            "Unit Frames",
            "Mana Bars",
            "Castbars"
        };

        public static readonly string[] PartyRaidSections =
        {
            "Nameplates",
            "Party Frames",
            "Party Cooldowns",
            "Enemy List"
        };

        public static readonly string[] UtilitySections =
        {
            "Minimap",
            "Action Camera"
        };

        public const string PlayerParameterOrbSection = "Player Parameter Orb";

        public static bool IsPlayerTargetSection(string? sectionName)
        {
            if (string.IsNullOrEmpty(sectionName))
            {
                return false;
            }

            if (sectionName == PlayerParameterOrbSection)
            {
                return true;
            }

            return IsIndividualFramesSection(sectionName)
                || Array.Exists(PlayerTargetAfterFramesSections, s => s == sectionName);
        }

        public static bool IsPartyRaidSection(string? sectionName) =>
            !string.IsNullOrEmpty(sectionName) && Array.Exists(PartyRaidSections, s => s == sectionName);

        public static bool IsIndividualFramesSection(string? sectionName) =>
            !string.IsNullOrEmpty(sectionName) && Array.Exists(IndividualFrameSections, s => s == sectionName);

        public static bool IsUtilitySection(string? sectionName) =>
            !string.IsNullOrEmpty(sectionName) && Array.Exists(UtilitySections, s => s == sectionName);

        public static bool IsFeaturesNavSection(string? sectionName) =>
            IsPlayerTargetSection(sectionName)
            || IsPartyRaidSection(sectionName)
            || IsUtilitySection(sectionName);
    }
}
