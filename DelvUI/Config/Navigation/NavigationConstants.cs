namespace DelvUI.Config.Navigation
{
    public static class NavigationConstants
    {
        public const string Home = "Home";
        public const string ProfilesImport = "Profiles & Import";
        public const string AdvancedOptions = "Advanced Options";

        public static readonly string[] AdvancedSections =
        {
            "Colors",
            "Customization",
            "Visibility",
            "Misc"
        };

        public static readonly string[] NavExcludedSections =
        {
            "Import",
            "Profiles",
            "Colors",
            "Customization",
            "Visibility",
            "Misc"
        };
    }
}
