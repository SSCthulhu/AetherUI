using DelvUI.Config;
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
        private const float CompactSectionSpacing = 3f;
        private const float CompactVerticalInset = 4f;

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
                + frame + 6f * scale + sectionSpacing
                + (textLine + itemSpacing + frame);
        }

        public static float GetHomePanelHeight()
        {
            float scale = ImGuiHelpers.GlobalScale;
            float inset = CompactVerticalInset * scale;
            float windowPadding = ImGui.GetStyle().WindowPadding.Y * 2f;

            return EstimateHomePanelBodyHeight() + inset * 2f + windowPadding + 8f * scale;
        }

        public static bool DrawHomePanel(FontsConfig fontsConfig, HUDOptionsConfig hudOptionsConfig, ref bool changed)
        {
            if (fontsConfig.Fonts.Count == 0)
            {
                ImGui.TextWrapped("Fonts are not available. Check the Customization > Fonts page.");
                return false;
            }

            float inset = CompactVerticalInset * ImGuiHelpers.GlobalScale;
            ImGui.Dummy(new Vector2(0f, inset));

            bool panelChanged = Draw(fontsConfig, hudOptionsConfig, ref changed, compact: true);

            ImGui.Dummy(new Vector2(0f, inset));

            return panelChanged;
        }

        public static bool Draw(FontsConfig fontsConfig, HUDOptionsConfig hudOptionsConfig, ref bool changed, bool compact = false)
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
            bool applyChanged = false;
            bool panelChanged = false;

            if (compact)
            {
                float sectionSpacing = CompactSectionSpacing * ImGuiHelpers.GlobalScale;

                comboChanged |= DrawCompactLabeledCombo("GLOBAL FONT", "##homeGlobalTextFont", ref globalFontIndex, selectableFontStyles);
                ImGui.Dummy(new Vector2(0f, sectionSpacing));
                comboChanged |= DrawCompactLabeledCombo("GLOBAL NUMERIC FONT", "##homeGlobalNumericFont", ref globalNumericFontIndex, selectableFontStyles);
                ImGui.Dummy(new Vector2(0f, sectionSpacing));
                applyChanged = fontsConfig.DrawApplyGlobalFontsButton(ref changed, centered: true, homeAccentStyle: true);
                ImGui.Dummy(new Vector2(0f, sectionSpacing));
                panelChanged |= DrawCompactGlobalHudScale(hudOptionsConfig, ref changed);
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
                ImGui.Spacing();
                panelChanged |= DrawStandardGlobalHudScale(hudOptionsConfig, ref changed);
            }

            if (comboChanged)
            {
                fontsConfig.GlobalFontKey = globalFontIndex == 0 ? null : selectableFontStyles[globalFontIndex];
                fontsConfig.GlobalNumericFontKey = globalNumericFontIndex == 0 ? null : selectableFontStyles[globalNumericFontIndex];
                changed = true;
            }

            return comboChanged | applyChanged | panelChanged;
        }

        private static bool DrawCompactGlobalHudScale(HUDOptionsConfig hudOptionsConfig, ref bool changed)
        {
            float availWidth = ImGui.GetContentRegionAvail().X;
            float comboWidth = availWidth * CompactComboWidthRatio;
            float startX = ImGui.GetCursorPosX() + GetCompactLeftOffset(availWidth);
            float scale = hudOptionsConfig.GlobalHudScale;
            float uiScale = ImGuiHelpers.GlobalScale;
            const float inputWidth = 56f;
            float scaledInputWidth = inputWidth * uiScale;
            float itemSpacing = ImGui.GetStyle().ItemSpacing.X;
            float sliderWidth = Math.Max(0f, comboWidth - scaledInputWidth - itemSpacing);

            ImGui.SetCursorPosX(startX);
            ImGui.PushStyleColor(ImGuiCol.Text, HomeUiStyle.Gold);
            ImGui.Text("GLOBAL HUD SCALE");
            ImGui.PopStyleColor();

            ImGui.SetCursorPosX(startX);
            PushHomeSliderChrome();
            ImGui.PushItemWidth(sliderWidth);
            bool sliderChanged = ImGui.SliderFloat(
                "##homeGlobalHudScale",
                ref scale,
                GlobalHudScaleHelper.MinScale,
                GlobalHudScaleHelper.MaxScale,
                string.Empty);
            ImGui.PopItemWidth();
            PopHomeSliderChrome();

            ImGui.SameLine(0f, itemSpacing);
            PushHomeFieldChrome();
            ImGui.PushItemWidth(scaledInputWidth);
            bool inputChanged = ImGui.InputFloat("##homeGlobalHudScaleInput", ref scale, 0f, 0f, "%.2f");
            ImGui.PopItemWidth();
            PopHomeFieldChrome();

            return TryApplyGlobalHudScale(hudOptionsConfig, ref scale, sliderChanged || inputChanged, ref changed);
        }

        private static bool DrawStandardGlobalHudScale(HUDOptionsConfig hudOptionsConfig, ref bool changed)
        {
            float scale = hudOptionsConfig.GlobalHudScale;
            float uiScale = ImGuiHelpers.GlobalScale;
            const float inputWidth = 56f;
            float scaledInputWidth = inputWidth * uiScale;
            float itemSpacing = ImGui.GetStyle().ItemSpacing.X;
            float sliderWidth = Math.Max(0f, ImGui.GetContentRegionAvail().X - scaledInputWidth - itemSpacing);

            ImGui.PushItemWidth(sliderWidth);
            bool sliderChanged = ImGui.SliderFloat(
                "Global HUD Scale",
                ref scale,
                GlobalHudScaleHelper.MinScale,
                GlobalHudScaleHelper.MaxScale,
                string.Empty);
            ImGui.PopItemWidth();

            ImGui.SameLine(0f, itemSpacing);
            ImGui.PushItemWidth(scaledInputWidth);
            bool inputChanged = ImGui.InputFloat("##globalHudScaleInput", ref scale, 0f, 0f, "%.2f");
            ImGui.PopItemWidth();

            return TryApplyGlobalHudScale(hudOptionsConfig, ref scale, sliderChanged || inputChanged, ref changed);
        }

        private static bool TryApplyGlobalHudScale(
            HUDOptionsConfig hudOptionsConfig,
            ref float scale,
            bool edited,
            ref bool changed)
        {
            if (!edited)
            {
                return false;
            }

            scale = Math.Clamp(scale, GlobalHudScaleHelper.MinScale, GlobalHudScaleHelper.MaxScale);
            if (Math.Abs(hudOptionsConfig.GlobalHudScale - scale) < 0.0001f)
            {
                return false;
            }

            hudOptionsConfig.GlobalHudScale = scale;
            ConfigurationManager.Instance.ForceNeedsSave();
            return false;
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
            return DrawHomeFontCombo(id, ref index, options, comboWidth);
        }

        private static bool DrawHomeFontCombo(string comboId, ref int index, string[] options, float comboWidth)
        {
            if (options.Length == 0)
            {
                return false;
            }

            index = Math.Clamp(index, 0, options.Length - 1);
            string preview = options[index];

            const int maxVisibleItems = 12;
            float itemHeight = ImGui.GetTextLineHeightWithSpacing();
            int visibleItems = Math.Min(options.Length, maxVisibleItems);
            float popupHeight = visibleItems * itemHeight + ImGui.GetStyle().WindowPadding.Y * 2f;

            PushHomeComboChrome();
            ImGui.PushItemWidth(comboWidth);

            Vector2 comboScreenPos = ImGui.GetCursorScreenPos();
            float frameHeight = ImGui.GetFrameHeightWithSpacing();
            float windowBottom = ImGui.GetWindowPos().Y + ImGui.GetWindowContentRegionMax().Y;
            float spaceBelow = windowBottom - (comboScreenPos.Y + frameHeight);
            bool openUpward = spaceBelow < popupHeight;

            ImGui.SetNextWindowSize(new Vector2(comboWidth, popupHeight), ImGuiCond.Appearing);
            if (openUpward)
            {
                ImGui.SetNextWindowPos(comboScreenPos, ImGuiCond.Appearing, new Vector2(0f, 1f));
            }

            bool changed = false;
            if (ImGui.BeginCombo(comboId, preview))
            {
                for (int i = 0; i < options.Length; i++)
                {
                    bool isSelected = index == i;
                    if (ImGui.Selectable($"{options[i]}##{comboId}_{i}", isSelected))
                    {
                        index = i;
                        changed = true;
                    }

                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.PopItemWidth();
            PopHomeComboChrome();

            return changed;
        }

        private static void PushHomeFieldChrome()
        {
            Vector4 transparent = Vector4.Zero;
            float scale = ImGuiHelpers.GlobalScale;
            Vector2 framePadding = ImGui.GetStyle().FramePadding;
            ImGui.PushStyleColor(ImGuiCol.FrameBg, transparent);
            ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, transparent);
            ImGui.PushStyleColor(ImGuiCol.FrameBgActive, transparent);
            ImGui.PushStyleColor(ImGuiCol.Border, HomeUiStyle.PanelBorder);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1f);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 6f);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(10f * scale, framePadding.Y));
        }

        private static void PopHomeFieldChrome()
        {
            ImGui.PopStyleVar(3);
            ImGui.PopStyleColor(4);
        }

        private static void PushHomeComboChrome()
        {
            ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Vector4.Zero);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, Vector4.Zero);
            PushHomeFieldChrome();
        }

        private static void PopHomeComboChrome()
        {
            PopHomeFieldChrome();
            ImGui.PopStyleColor(3);
        }

        private static void PushHomeSliderChrome()
        {
            PushHomeFieldChrome();
            ImGui.PushStyleColor(ImGuiCol.SliderGrab, HomeUiStyle.Gold);
            ImGui.PushStyleColor(ImGuiCol.SliderGrabActive, HomeUiStyle.Gold);
        }

        private static void PopHomeSliderChrome()
        {
            ImGui.PopStyleColor(2);
            PopHomeFieldChrome();
        }
    }
}
