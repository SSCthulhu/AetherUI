using Dalamud.Interface;
using DelvUI.Config.Home.Widgets;
using DelvUI.Config.Navigation;
using System;
using System.Numerics;

namespace DelvUI.Config.Navigation
{
    public static class SidebarNavIcons
    {
        public static FontAwesomeIcon GetGroupIcon(string groupLabel)
        {
            return groupLabel switch
            {
                FeatureNavGroups.PlayerTarget => FontAwesomeIcon.User,
                FeatureNavGroups.PartyRaid => FontAwesomeIcon.Users,
                FeatureNavGroups.IndividualFrames => FontAwesomeIcon.ThLarge,
                NavigationConstants.AdvancedOptions => FontAwesomeIcon.SlidersH,
                _ => FontAwesomeIcon.Folder
            };
        }

        public static FontAwesomeIcon GetTopLevelIcon(string label)
        {
            return label switch
            {
                NavigationConstants.Home => FontAwesomeIcon.Home,
                NavigationConstants.ProfilesImport => FontAwesomeIcon.FolderOpen,
                _ => FontAwesomeIcon.Square
            };
        }

        public static FontAwesomeIcon GetSectionIcon(string sectionName)
        {
            return sectionName switch
            {
                FeatureNavGroups.PlayerParameterOrbSection => FontAwesomeIcon.Circle,
                "Unit Frames" => FontAwesomeIcon.Heart,
                "Mana Bars" => FontAwesomeIcon.Tint,
                "Castbars" => FontAwesomeIcon.Magic,
                "Buffs and Debuffs" => FontAwesomeIcon.Star,
                "Job Specific Bars" => FontAwesomeIcon.Briefcase,
                "Other Elements" => FontAwesomeIcon.EllipsisH,
                "Nameplates" => FontAwesomeIcon.IdBadge,
                "Party Frames" => FontAwesomeIcon.Users,
                "Party Cooldowns" => FontAwesomeIcon.Clock,
                "Enemy List" => FontAwesomeIcon.Skull,
                "Minimap" => FontAwesomeIcon.Map,
                "Action Camera" => FontAwesomeIcon.Video,
                "Colors" => FontAwesomeIcon.Palette,
                "Customization" => FontAwesomeIcon.PaintBrush,
                "Visibility" => FontAwesomeIcon.Eye,
                "Misc" => FontAwesomeIcon.Cog,
                _ => FontAwesomeIcon.ChevronRight
            };
        }
    }
}
