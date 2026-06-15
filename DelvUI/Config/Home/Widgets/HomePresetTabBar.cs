using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Bindings.ImGui;
using System.Numerics;

namespace DelvUI.Config.Home.Widgets
{
    public static class HomePresetTabBar
    {
        private static readonly string[] TabLabels =
        {
            "Minimal",
            "MMO Modern",
            "Raid Focused",
            "Action Combat",
            "Custom"
        };

        public static bool DrawTab(int tabIndex, int selectedIndex, float tabWidth, float tabHeight)
        {
            Vector2 cursor = ImGui.GetCursorScreenPos();
            Vector2 size = new Vector2(tabWidth, tabHeight);
            bool isSelected = tabIndex == selectedIndex;
            bool isHovered = !isSelected && ImGui.IsMouseHoveringRect(cursor, cursor + size);
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            Vector4 bg = isSelected
                ? new Vector4(HomeUiStyle.Accent.X, HomeUiStyle.Accent.Y, HomeUiStyle.Accent.Z, 0.18f)
                : isHovered
                    ? new Vector4(HomeUiStyle.Accent.X, HomeUiStyle.Accent.Y, HomeUiStyle.Accent.Z, 0.1f)
                    : HomeUiStyle.PanelBg;

            Vector4 border = isSelected
                ? HomeUiStyle.Accent
                : isHovered
                    ? HomeUiStyle.AccentGlow
                    : HomeUiStyle.PanelBorder;

            float borderThickness = isSelected ? 2f : isHovered ? 1.5f : 1f;

            drawList.AddRectFilled(cursor, cursor + size, ImGui.ColorConvertFloat4ToU32(bg), 6f);
            drawList.AddRect(cursor, cursor + size, ImGui.ColorConvertFloat4ToU32(border), 6f, ImDrawFlags.RoundCornersAll, borderThickness);

            FontAwesomeIcon icon = HomeFeatureIcons.GetPresetIcon(tabIndex);
            ImGui.PushFont(UiBuilder.IconFont);
            Vector2 iconSize = ImGui.CalcTextSize(icon.ToIconString());
            Vector4 iconColor = isSelected || isHovered ? HomeUiStyle.Accent : HomeUiStyle.TextMuted;
            drawList.AddText(
                cursor + new Vector2((tabWidth - iconSize.X) * 0.5f, 12f),
                ImGui.ColorConvertFloat4ToU32(iconColor),
                icon.ToIconString());
            ImGui.PopFont();

            string label = TabLabels[tabIndex].ToUpperInvariant();
            Vector2 labelSize = ImGui.CalcTextSize(label);
            Vector4 labelColor = isSelected ? Vector4.One : isHovered ? HomeUiStyle.Accent : HomeUiStyle.TextMuted;
            drawList.AddText(
                cursor + new Vector2((tabWidth - labelSize.X) * 0.5f, tabHeight - 26f),
                ImGui.ColorConvertFloat4ToU32(labelColor),
                label);

            ImGui.InvisibleButton($"##presetTab{tabIndex}", size);
            return ImGui.IsItemClicked();
        }

        public static string GetTabLabel(int tabIndex) => TabLabels[tabIndex];

        public static int TabCount => TabLabels.Length;
    }
}
