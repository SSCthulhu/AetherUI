namespace DelvUI.Config.Navigation
{
    public static class SidebarNavLabels
    {
        public static string GetDisplayLabel(string sectionName)
        {
            return sectionName switch
            {
                FeatureNavGroups.PlayerParameterOrbSection => "Player Orb",
                _ => sectionName
            };
        }
    }
}
