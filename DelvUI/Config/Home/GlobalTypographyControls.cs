using DelvUI.Config.Home.Widgets;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using System;
using System.Linq;
using System.Numerics;

namespace DelvUI.Config.Home
{
    public static class GlobalTypographyControls
    {
        private const float CompactComboWidthRatio = 0.82f;
        private const float CompactSectionSpacing = 10f;
        private const float CompactVerticalInset = 16f;

        public static float GetCompactLeftOffset(float availWidth)
        {
            float comboWidth = availWidth * CompactComboWidthRatio;
            return Math.Max(0f, (availWidth - comboWidth) * 0.5f);
        }

        public static float GetCompactContentStartOffset(float panelWidth)
        {
            float padding = ImGui.GetStyle().WindowPadding.X;
            float innerWidth = panelWidth - padding * 2f;
            return padding + GetCompactLeftOffset(innerWidth);
        }

        public static float EstimateHomePanelBodyHeight()
        {
            float scale = ImGuiHelpers.GlobalScale;
            float textLine = ImGui.GetTextLineHeight();
            float frame = ImGui.GetFrameHeight();
            float itemSpacing = ImGui.GetStyle().ItemSpacing.Y;
            float sectionSpacing = CompactSectionSpacing * scale;
            float fontRow = textLine + itemSpacing + frame;

            return fontRow + sectionSpacing
                + fontRow + sectionSpacing
                + frame + 10f * scale + itemSpacing;
        }

        public static float GetHomePanelHeight()
        {
            float scale = ImGuiHelpers.GlobalScale;
            float inset = CompactVerticalInset * scale;
            float windowPadding = ImGui.GetStyle().WindowPadding.Y * 2f;

            return EstimateHomePanelBodyHeight() + inset * 2f + windowPadding + 8f * scale;
        }

        public static bool DrawHomePanel(FontsConfig fontsConfig, ref bool changed)
        {
            if (fontsConfig.Fonts.Count == 0)
            {
                ImGui.TextWrapped("Fonts are not available. Check the Customization > Fonts page.");
                return false;
            }

            float inset = CompactVerticalInset * ImGuiHelpers.GlobalScale;
            ImGui.Dummy(new Vector2(0f, inset));

            bool panelChanged = Draw(fontsConfig, ref changed, compact: true);

            ImGui.Dummy(new Vector2(0f, inset));

            return panelChanged;
        }

        public static bool Draw(FontsConfig fontsConfig, ref bool changed, bool compact = false)
        {
            if (fontsConfig.Fonts.Count == 0)
            {
                ImGui.TextWrapped("Fonts are not available. Check the Customization > Fonts page.");
                return false;
            }

            if (!compact)
            {
                ImGui.TextWrapped("Global Font and Global Numeric Font apply to text labels across the HUD.");
                ImGui.Spacing();
            }

            string[] selectableFontStyles = BuildSelectableFontStyles(fontsConfig, out string[] styleNames);

            int globalFontIndex = ResolveFontIndex(fontsConfig.GlobalFontKey, fontsConfig, styleNames);
            int globalNumericFontIndex = ResolveFontIndex(fontsConfig.GlobalNumericFontKey, fontsConfig, styleNames);

            bool comboChanged = false;
            bool applyChanged;

            if (compact)
            {
                float sectionSpacing = CompactSectionSpacing * ImGuiHelpers.GlobalScale;

                comboChanged |= DrawCompactLabeledCombo("GLOBAL FONT", "##homeGlobalTextFont", ref globalFontIndex, selectableFontStyles);
                ImGui.Dummy(new Vector2(0f, sectionSpacing));
                comboChanged |= DrawCompactLabeledCombo("GLOBAL NUMERIC FONT", "##homeGlobalNumericFont", ref globalNumericFontIndex, selectableFontStyles);
                ImGui.Dummy(new Vector2(0f, sectionSpacing));
                applyChanged = fontsConfig.DrawApplyGlobalFontsButton(ref changed, centered: true, homeAccentStyle: true);
            }
            else
            {
                if (ImGui.Combo("Global Font##homeGlobalTextFont", ref globalFontIndex, selectableFontStyles, 12))
                {
                    comboChanged = true;
                }

                if (ImGui.Combo("Global Numeric Font##homeGlobalNumericFont", ref globalNumericFontIndex, selectableFontStyles, 12))
                {
                    comboChanged = true;
                }

                ImGui.Spacing();
                applyChanged = fontsConfig.DrawApplyGlobalFontsButton(ref changed);
            }

            if (comboChanged)
            {
                fontsConfig.GlobalFontKey = globalFontIndex == 0 ? null : selectableFontStyles[globalFontIndex];
                fontsConfig.GlobalNumericFontKey = globalNumericFontIndex == 0 ? null : selectableFontStyles[globalNumericFontIndex];
                changed = true;
            }

            return comboChanged | applyChanged;
        }

        private static string[] BuildSelectableFontStyles(FontsConfig fontsConfig, out string[] styleNames)
        {
            styleNames = fontsConfig.Fonts.Values
                .Select(fontData => fontData.Name)
                .Distinct()
                .OrderBy(name => name)
                .ToArray();

            string[] selectableFontStyles = new string[styleNames.Length + 1];
            selectableFontStyles[0] = "(None)";
            for (int i = 0; i < styleNames.Length; i++)
            {
                selectableFontStyles[i + 1] = styleNames[i];
            }

            return selectableFontStyles;
        }

        private static int ResolveFontIndex(string? fontKey, FontsConfig fontsConfig, string[] styleNames)
        {
            if (string.IsNullOrEmpty(fontKey))
            {
                return 0;
            }

            string style = fontKey;
            if (fontsConfig.Fonts.TryGetValue(fontKey, out FontData keyData))
            {
                style = keyData.Name;
            }

            int styleIndex = Array.IndexOf(styleNames, style);
            return styleIndex >= 0 ? styleIndex + 1 : 0;
        }

        private static bool DrawCompactLabeledCombo(string label, string id, ref int index, string[] options)
        {
            float availWidth = ImGui.GetContentRegionAvail().X;
            float comboWidth = availWidth * CompactComboWidthRatio;
            float startX = ImGui.GetCursorPosX() + GetCompactLeftOffset(availWidth);

            ImGui.SetCursorPosX(startX);
            ImGui.PushStyleColor(ImGuiCol.Text, HomeUiStyle.Gold);
            ImGui.Text(label);
            ImGui.PopStyleColor();

            ImGui.SetCursorPosX(startX);
            ImGui.PushItemWidth(comboWidth);
            PushHomeComboChrome();
            bool changed = ImGui.Combo(id, ref index, options, 12);
            PopHomeComboChrome();
            ImGui.PopItemWidth();

            return changed;
        }

        private static void PushHomeComboChrome()
        {
            Vector4 transparent = Vector4.Zero;
            float scale = ImGuiHelpers.GlobalScale;
            Vector2 framePadding = ImGui.GetStyle().FramePadding;
            ImGui.PushStyleColor(ImGuiCol.FrameBg, transparent);
            ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, transparent);
            ImGui.PushStyleColor(ImGuiCol.FrameBgActive, transparent);
            ImGui.PushStyleColor(ImGuiCol.Button, transparent);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, transparent);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, transparent);
            ImGui.PushStyleColor(ImGuiCol.Border, HomeUiStyle.PanelBorder);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1f);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 6f);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(10f * scale, framePadding.Y));
        }

        private static void PopHomeComboChrome()
        {
            ImGui.PopStyleVar(3);
            ImGui.PopStyleColor(7);
        }
    }
}
