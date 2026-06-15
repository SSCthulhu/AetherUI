using Dalamud.Interface;
using Dalamud.Interface.Utility;
using DelvUI.Config.Home;
using DelvUI.Config.Navigation;
using Dalamud.Bindings.ImGui;
using System;
using System.Numerics;

namespace DelvUI.Config.Home.Widgets
{
    public static class HomeFeatureTile
    {
        private const float SubRowIndent = 12f;

        public static bool Draw(
            string id,
            FontAwesomeIcon icon,
            string title,
            ref bool enabled,
            Vector2 size,
            HomeEditTargetId editTarget,
            Action? drawExpandedContent = null,
            bool advanceLayout = true)
        {
            bool changed = false;
            Vector2 startPos = ImGui.GetCursorPos();
            Vector2 cursor = ImGui.GetCursorScreenPos();
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            string displayTitle = title.ToUpperInvariant();

            drawList.AddRectFilled(cursor, cursor + size, ImGui.ColorConvertFloat4ToU32(HomeUiStyle.PanelBg), 8f);
            drawList.AddRect(cursor, cursor + size, ImGui.ColorConvertFloat4ToU32(HomeUiStyle.PanelBorder), 8f, ImDrawFlags.RoundCornersAll, 1f);

            ImGui.PushFont(UiBuilder.IconFont);
            drawList.AddText(cursor + new Vector2(12f, 12f), ImGui.ColorConvertFloat4ToU32(HomeUiStyle.Accent), icon.ToIconString());
            ImGui.PopFont();

            drawList.AddText(cursor + new Vector2(36f, 14f), ImGui.ColorConvertFloat4ToU32(Vector4.One), displayTitle);

            Vector2 pillSize = new Vector2(52f * ImGuiHelpers.GlobalScale, 22f * ImGuiHelpers.GlobalScale);
            ImGui.SetCursorScreenPos(cursor + new Vector2(size.X - pillSize.X - 10f, 10f));
            changed |= HomePillToggle.Draw(id + "_pill", ref enabled, pillSize, editTarget);

            if (drawExpandedContent != null && enabled)
            {
                ImGui.SetCursorScreenPos(cursor + new Vector2(10f, 40f));
                ImGui.PushID(id + "_expanded");
                drawExpandedContent();
                ImGui.PopID();
            }

            if (advanceLayout)
            {
                ImGui.SetCursorPos(startPos + new Vector2(0f, size.Y + 8f));
            }

            return changed;
        }

        public static bool DrawIndividualFramesGroup(
            string id,
            Vector2 size,
            ref bool unitFrames,
            ref bool manaBars,
            ref bool castBars)
        {
            bool changed = false;
            Vector2 startPos = ImGui.GetCursorPos();
            Vector2 cursor = ImGui.GetCursorScreenPos();
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(cursor, cursor + size, ImGui.ColorConvertFloat4ToU32(HomeUiStyle.PanelBg), 8f);
            drawList.AddRect(cursor, cursor + size, ImGui.ColorConvertFloat4ToU32(HomeUiStyle.PanelBorder), 8f, ImDrawFlags.RoundCornersAll, 1f);

            float padding = 10f;
            FontAwesomeIcon headerIcon = HomeFeatureIcons.GetIcon(FeatureId.UnitFrames);

            ImGui.PushFont(UiBuilder.IconFont);
            drawList.AddText(
                cursor + new Vector2(padding, 10f),
                ImGui.ColorConvertFloat4ToU32(HomeUiStyle.Accent),
                headerIcon.ToIconString());
            ImGui.PopFont();

            drawList.AddText(
                cursor + new Vector2(padding + 24f, 12f),
                ImGui.ColorConvertFloat4ToU32(Vector4.One),
                "INDIVIDUAL FRAMES");

            float rowWidth = size.X - padding * 2f;
            float headerOffset = 36f * ImGuiHelpers.GlobalScale;
            float bottomPadding = 8f * ImGuiHelpers.GlobalScale;
            float availableHeight = Math.Max(0f, size.Y - headerOffset - bottomPadding);
            const int subRowCount = 3;
            float rowStep = availableHeight / subRowCount;
            float pillHeight = Math.Min(20f * ImGuiHelpers.GlobalScale, Math.Max(16f * ImGuiHelpers.GlobalScale, rowStep - 2f));
            ImGui.SetCursorPos(startPos + new Vector2(padding, headerOffset));

            ImGui.PushID(id);
            changed |= DrawSubRow("sub_unit", "Unit Frames", FontAwesomeIcon.Heart, ref unitFrames, rowWidth, rowStep, pillHeight, HomeEditTargetId.UnitFrames);
            changed |= DrawSubRow("sub_mana", "Mana Bars", FontAwesomeIcon.Tint, ref manaBars, rowWidth, rowStep, pillHeight, HomeEditTargetId.ManaBars);
            changed |= DrawSubRow("sub_cast", "Cast Bars", FontAwesomeIcon.Magic, ref castBars, rowWidth, rowStep, pillHeight, HomeEditTargetId.CastBars);
            ImGui.PopID();

            return changed;
        }

        public static bool DrawOtherElementsGroup(
            string id,
            Vector2 size,
            ref bool experienceBar,
            ref bool gcdIndicator,
            ref bool pullTimer,
            ref bool limitBreak,
            ref bool mpTicker)
        {
            bool changed = false;
            Vector2 startPos = ImGui.GetCursorPos();
            Vector2 cursor = ImGui.GetCursorScreenPos();
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(cursor, cursor + size, ImGui.ColorConvertFloat4ToU32(HomeUiStyle.PanelBg), 8f);
            drawList.AddRect(cursor, cursor + size, ImGui.ColorConvertFloat4ToU32(HomeUiStyle.PanelBorder), 8f, ImDrawFlags.RoundCornersAll, 1f);

            float padding = 10f;
            FontAwesomeIcon headerIcon = HomeFeatureIcons.GetIcon(FeatureId.OtherElements);

            ImGui.PushFont(UiBuilder.IconFont);
            drawList.AddText(
                cursor + new Vector2(padding, 10f),
                ImGui.ColorConvertFloat4ToU32(HomeUiStyle.Accent),
                headerIcon.ToIconString());
            ImGui.PopFont();

            drawList.AddText(
                cursor + new Vector2(padding + 24f, 12f),
                ImGui.ColorConvertFloat4ToU32(Vector4.One),
                "OTHER ELEMENTS");

            float rowWidth = size.X - padding * 2f;
            float headerOffset = 36f * ImGuiHelpers.GlobalScale;
            float bottomPadding = 8f * ImGuiHelpers.GlobalScale;
            float availableHeight = Math.Max(0f, size.Y - headerOffset - bottomPadding);
            const int subRowCount = 5;
            float rowStep = availableHeight / subRowCount;
            float pillHeight = Math.Min(20f * ImGuiHelpers.GlobalScale, Math.Max(16f * ImGuiHelpers.GlobalScale, rowStep - 2f));
            ImGui.SetCursorPos(startPos + new Vector2(padding, headerOffset));

            ImGui.PushID(id);
            changed |= DrawSubRow("sub_experience", "Experience Bar", FontAwesomeIcon.ChartLine, ref experienceBar, rowWidth, rowStep, pillHeight, HomeEditTargetId.ExperienceBar);
            changed |= DrawSubRow("sub_gcd", "GCD Indicator", FontAwesomeIcon.Stopwatch, ref gcdIndicator, rowWidth, rowStep, pillHeight, HomeEditTargetId.GcdIndicator);
            changed |= DrawSubRow("sub_pull", "Pull Timer", FontAwesomeIcon.HourglassHalf, ref pullTimer, rowWidth, rowStep, pillHeight, HomeEditTargetId.PullTimer);
            changed |= DrawSubRow("sub_limit", "Limit Break", FontAwesomeIcon.Bolt, ref limitBreak, rowWidth, rowStep, pillHeight, HomeEditTargetId.LimitBreak);
            changed |= DrawSubRow("sub_mp", "MP Ticker", FontAwesomeIcon.Tint, ref mpTicker, rowWidth, rowStep, pillHeight, HomeEditTargetId.MpTicker);
            ImGui.PopID();

            return changed;
        }

        public static bool DrawSubRow(
            string id,
            string label,
            FontAwesomeIcon icon,
            ref bool enabled,
            float rowWidth,
            float rowStep,
            float pillHeight,
            HomeEditTargetId editTarget)
        {
            Vector2 baseStart = ImGui.GetCursorPos();
            float indent = SubRowIndent * ImGuiHelpers.GlobalScale;
            float pillWidth = 52f * ImGuiHelpers.GlobalScale;
            float contentOffsetY = Math.Max(0f, (rowStep - pillHeight) * 0.5f);

            ImGui.SetCursorPos(baseStart + new Vector2(indent, contentOffsetY));

            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.PushStyleColor(ImGuiCol.Text, HomeUiStyle.Accent);
            ImGui.Text(icon.ToIconString());
            ImGui.PopStyleColor();
            ImGui.PopFont();

            ImGui.SameLine();
            ImGui.Text(label);

            ImGui.SetCursorPos(new Vector2(baseStart.X + rowWidth - pillWidth, baseStart.Y + contentOffsetY));
            bool changed = HomePillToggle.Draw(id, ref enabled, new Vector2(pillWidth, pillHeight), editTarget);

            ImGui.SetCursorPos(baseStart + new Vector2(0f, rowStep));
            return changed;
        }
    }
}
