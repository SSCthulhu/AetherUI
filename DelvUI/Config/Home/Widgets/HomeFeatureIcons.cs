using Dalamud.Interface;
using DelvUI.Config.Navigation;

namespace DelvUI.Config.Home.Widgets
{
    public static class HomeFeatureIcons
    {
        public static FontAwesomeIcon GetIcon(FeatureId featureId)
        {
            return featureId switch
            {
                FeatureId.PlayerParameterOrb => FontAwesomeIcon.Circle,
                FeatureId.UnitFrames => FontAwesomeIcon.ThLarge,
                FeatureId.ManaBars => FontAwesomeIcon.Tint,
                FeatureId.CastBars => FontAwesomeIcon.Magic,
                FeatureId.BuffsAndDebuffs => FontAwesomeIcon.Star,
                FeatureId.OtherElements => FontAwesomeIcon.EllipsisH,
                FeatureId.Nameplates => FontAwesomeIcon.IdBadge,
                FeatureId.PartyFrames => FontAwesomeIcon.Users,
                FeatureId.PartyCooldowns => FontAwesomeIcon.Clock,
                FeatureId.EnemyList => FontAwesomeIcon.Skull,
                FeatureId.JobSpecificBars => FontAwesomeIcon.Briefcase,
                FeatureId.Minimap => FontAwesomeIcon.Map,
                FeatureId.ActionCamera => FontAwesomeIcon.Video,
                _ => FontAwesomeIcon.Square
            };
        }

        public static FontAwesomeIcon GetPresetIcon(int tabIndex)
        {
            return tabIndex switch
            {
                0 => FontAwesomeIcon.Circle,
                1 => FontAwesomeIcon.Desktop,
                2 => FontAwesomeIcon.ShieldAlt,
                3 => FontAwesomeIcon.Crosshairs,
                4 => FontAwesomeIcon.SlidersH,
                _ => FontAwesomeIcon.Square
            };
        }
    }
}
