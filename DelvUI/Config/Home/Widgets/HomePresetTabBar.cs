using Dalamud.Interface;
using Dalamud.Interface.Utility;
using DelvUI.Config.Home.Widgets;
using Dalamud.Bindings.ImGui;
using System;
using System.Numerics;

namespace DelvUI.Config.Home.Widgets
{
    public enum PresetTabInteraction
    {
        None,
        TabClicked,
        GearClicked
    }

    public static class HomePresetTabBar
    {
        private const float GearIconPad = 4f;

        private static readonly string[] TabLabels =
        {
            "Minimal",
            "MMO Modern",
            "Raid Focused",
            "Action Combat",
            "Custom"
        };

        public static PresetTabInteraction DrawTab(
            int tabIndex,
            int selectedIndex,
            float tabWidth,
            float tabHeight,
            bool attachHudEnabled,
            int hudLayout)
        {
            Vector2 cursor = ImGui.GetCursorScreenPos();
            Vector2 size = new Vector2(tabWidth, tabHeight);
            bool isSelected = tabIndex == selectedIndex;
            bool isHovered = !isSelected && ImGui.IsMouseHoveringRect(cursor, cursor + size);
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            float scale = ImGuiHelpers.GlobalScale;
            bool showGear = tabIndex < 4;

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

            if (showGear)
            {
                float gearPadding = 4f * scale;
                float gearIconPad = GearIconPad * scale;
                Vector2 gearSize = new Vector2(16f * scale, 16f * scale);
                Vector2 gearPos = cursor + new Vector2(gearPadding, gearPadding);
                Vector2 gearHitMin = gearPos - new Vector2(gearIconPad, gearIconPad);
                Vector2 gearHitMax = gearPos + gearSize + new Vector2(gearIconPad, gearIconPad);

                string? layoutLabel = attachHudEnabled && hudLayout > 0 ? hudLayout.ToString() : null;
                Vector2 layoutSize = Vector2.Zero;
                if (layoutLabel != null)
                {
                    layoutSize = ImGui.CalcTextSize(layoutLabel);
                    float layoutBottom = gearPos.Y + gearSize.Y + 1f * scale + layoutSize.Y;
                    gearHitMax.Y = layoutBottom + gearIconPad;
                }

                bool gearHovered = ImGui.IsMouseHoveringRect(gearHitMin, gearHitMax);
                if (gearHovered)
                {
                    Vector2 hitSize = gearHitMax - gearHitMin;
                    Vector4 fill = new Vector4(
                        Math.Min(HomeUiStyle.Accent.X + 0.08f, 1f),
                        Math.Min(HomeUiStyle.Accent.Y + 0.08f, 1f),
                        Math.Min(HomeUiStyle.Accent.Z + 0.08f, 1f),
                        HomeUiStyle.Accent.W);
                    float radius = hitSize.Y * 0.5f;
                    drawList.AddRectFilled(gearHitMin, gearHitMax, ImGui.ColorConvertFloat4ToU32(fill), radius);
                    drawList.AddRect(
                        gearHitMin,
                        gearHitMax,
                        ImGui.ColorConvertFloat4ToU32(HomeUiStyle.AccentGlow),
                        radius,
                        ImDrawFlags.RoundCornersAll,
                        1.5f);
                }

                ImGui.PushFont(UiBuilder.IconFont);
                string gearIcon = FontAwesomeIcon.Cog.ToIconString();
                Vector2 gearIconSize = ImGui.CalcTextSize(gearIcon);
                Vector4 gearColor = gearHovered
                    ? Vector4.One
                    : attachHudEnabled && hudLayout > 0
                        ? HomeUiStyle.Accent
                        : HomeUiStyle.TextMuted;
                drawList.AddText(
                    gearPos + (gearSize - gearIconSize) * 0.5f,
                    ImGui.ColorConvertFloat4ToU32(gearColor),
                    gearIcon);
                ImGui.PopFont();

                if (layoutLabel != null)
                {
                    Vector4 layoutColor = gearHovered ? Vector4.One : HomeUiStyle.Accent;
                    drawList.AddText(
                        gearPos + new Vector2((gearSize.X - layoutSize.X) * 0.5f, gearSize.Y + 1f * scale),
                        ImGui.ColorConvertFloat4ToU32(layoutColor),
                        layoutLabel);
                }
            }

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
            if (!ImGui.IsItemClicked())
            {
                return PresetTabInteraction.None;
            }

            if (showGear)
            {
                float gearPadding = 4f * scale;
                float gearIconPad = GearIconPad * scale;
                Vector2 gearSize = new Vector2(16f * scale, 16f * scale);
                Vector2 gearMin = cursor + new Vector2(gearPadding, gearPadding);
                Vector2 gearHitMin = gearMin - new Vector2(gearIconPad, gearIconPad);
                Vector2 gearHitMax = gearMin + gearSize + new Vector2(gearIconPad, gearIconPad);

                if (attachHudEnabled && hudLayout > 0)
                {
                    string layoutLabel = hudLayout.ToString();
                    Vector2 layoutSize = ImGui.CalcTextSize(layoutLabel);
                    float layoutBottom = gearMin.Y + gearSize.Y + 1f * scale + layoutSize.Y;
                    gearHitMax.Y = layoutBottom + gearIconPad;
                }

                Vector2 mousePos = ImGui.GetIO().MousePos;

                if (mousePos.X >= gearHitMin.X && mousePos.X <= gearHitMax.X
                    && mousePos.Y >= gearHitMin.Y && mousePos.Y <= gearHitMax.Y)
                {
                    return PresetTabInteraction.GearClicked;
                }
            }

            return PresetTabInteraction.TabClicked;
        }

        public static string GetTabLabel(int tabIndex) => TabLabels[tabIndex];

        public static int TabCount => TabLabels.Length;
    }
}
