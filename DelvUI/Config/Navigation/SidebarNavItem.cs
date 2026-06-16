using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using DelvUI.Config.Home.Widgets;
using System;
using System.Numerics;

namespace DelvUI.Config.Navigation
{
    public static class SidebarNavItem
    {
        private const float ItemHeight = 32f;
        private const float ItemSpacing = 2f;
        private const float LeftBarWidth = 4f;
        private const float IconColumnWidth = 28f;
        private const float HorizontalPadding = 10f;
        private const float ChevronColumnWidth = 18f;

        public static void DrawSectionHeader(string label)
        {
            float scale = ImGuiHelpers.GlobalScale;
            ImGui.PushStyleColor(ImGuiCol.Text, HomeUiStyle.Gold);
            ImGui.Text(label.ToUpperInvariant());
            ImGui.PopStyleColor();
            ImGui.Dummy(new Vector2(0f, 4f * scale));
        }

        public static bool DrawCollapsibleSectionHeader(
            string id,
            string label,
            ref bool expanded,
            bool childActive)
        {
            float scale = ImGuiHelpers.GlobalScale;
            float height = 28f * scale;
            float spacing = ItemSpacing * scale;
            float chevronColumn = ChevronColumnWidth * scale;
            float padding = HorizontalPadding * scale;

            Vector2 cursorStart = ImGui.GetCursorPos();
            float width = Math.Max(0f, ImGui.GetContentRegionAvail().X);
            Vector2 screenPos = ImGui.GetCursorScreenPos();
            Vector2 size = new Vector2(width, height);

            bool hovered = ImGui.IsMouseHoveringRect(screenPos, screenPos + size);
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            if (hovered || (childActive && !expanded))
            {
                float alpha = hovered ? 0.1f : 0.06f;
                Vector4 bg = new Vector4(HomeUiStyle.Accent.X, HomeUiStyle.Accent.Y, HomeUiStyle.Accent.Z, alpha);
                drawList.AddRectFilled(screenPos, screenPos + size, ImGui.ColorConvertFloat4ToU32(bg), 4f);
            }

            string displayLabel = label.ToUpperInvariant();
            Vector4 textColor = hovered || childActive ? HomeUiStyle.Accent : HomeUiStyle.Gold;
            Vector2 labelSize = ImGui.CalcTextSize(displayLabel);
            drawList.AddText(
                new Vector2(screenPos.X + padding, screenPos.Y + (height - labelSize.Y) * 0.5f),
                ImGui.ColorConvertFloat4ToU32(textColor),
                displayLabel);

            ImGui.PushFont(UiBuilder.IconFont);
            FontAwesomeIcon chevronIcon = expanded ? FontAwesomeIcon.ChevronDown : FontAwesomeIcon.ChevronRight;
            string chevron = chevronIcon.ToIconString();
            Vector2 chevronSize = ImGui.CalcTextSize(chevron);
            drawList.AddText(
                new Vector2(screenPos.X + size.X - chevronColumn - chevronSize.X * 0.5f, screenPos.Y + (height - chevronSize.Y) * 0.5f),
                ImGui.ColorConvertFloat4ToU32(HomeUiStyle.Gold),
                chevron);
            ImGui.PopFont();

            ImGui.SetCursorPos(cursorStart);
            ImGui.InvisibleButton(id, size);
            bool clicked = ImGui.IsItemClicked();
            if (clicked)
            {
                expanded = !expanded;
            }

            ImGui.SetCursorPos(cursorStart + new Vector2(0f, height + spacing));
            return clicked;
        }

        public static bool Draw(
            string id,
            string label,
            FontAwesomeIcon icon,
            bool selected,
            float indent = 0f,
            bool groupHeader = false)
        {
            return DrawInternal(id, label, icon, selected, indent, groupHeader, expanded: true, drawChevron: false);
        }

        public static bool DrawGroupHeader(
            string id,
            string label,
            FontAwesomeIcon icon,
            ref bool expanded,
            bool childActive,
            float indent = 0f)
        {
            bool selected = childActive && !expanded;
            bool clicked = DrawInternal(id, label, icon, selected, indent, groupHeader: true, expanded, drawChevron: true);
            if (clicked)
            {
                expanded = !expanded;
            }

            return clicked;
        }

        private static bool DrawInternal(
            string id,
            string label,
            FontAwesomeIcon icon,
            bool selected,
            float indent,
            bool groupHeader,
            bool expanded,
            bool drawChevron)
        {
            float scale = ImGuiHelpers.GlobalScale;
            float height = ItemHeight * scale;
            float spacing = ItemSpacing * scale;
            float leftBar = LeftBarWidth * scale;
            float iconColumn = IconColumnWidth * scale;
            float padding = HorizontalPadding * scale;
            float chevronColumn = ChevronColumnWidth * scale;

            Vector2 cursorStart = ImGui.GetCursorPos();
            float width = Math.Max(0f, ImGui.GetContentRegionAvail().X - indent);
            Vector2 screenPos = ImGui.GetCursorScreenPos();
            screenPos.X += indent;
            Vector2 size = new Vector2(width, height);

            bool hovered = ImGui.IsMouseHoveringRect(screenPos, screenPos + size);
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            if (selected)
            {
                drawList.AddRectFilled(
                    screenPos,
                    screenPos + new Vector2(leftBar, height),
                    ImGui.ColorConvertFloat4ToU32(HomeUiStyle.Accent));
            }

            float contentInset = selected ? leftBar : 0f;
            Vector4 bg = selected
                ? new Vector4(HomeUiStyle.Accent.X, HomeUiStyle.Accent.Y, HomeUiStyle.Accent.Z, 0.2f)
                : hovered
                    ? new Vector4(HomeUiStyle.Accent.X, HomeUiStyle.Accent.Y, HomeUiStyle.Accent.Z, 0.1f)
                    : Vector4.Zero;

            if (bg.W > 0f)
            {
                drawList.AddRectFilled(
                    screenPos + new Vector2(contentInset, 0f),
                    screenPos + size,
                    ImGui.ColorConvertFloat4ToU32(bg),
                    4f);
            }

            Vector4 iconColor = selected || hovered
                ? HomeUiStyle.Accent
                : groupHeader
                    ? HomeUiStyle.Gold
                    : HomeUiStyle.TextMuted;

            Vector4 textColor = selected
                ? Vector4.One
                : hovered
                    ? HomeUiStyle.Accent
                    : groupHeader
                        ? HomeUiStyle.Gold
                        : new Vector4(0.82f, 0.82f, 0.82f, 1f);

            ImGui.PushFont(UiBuilder.IconFont);
            string iconText = icon.ToIconString();
            Vector2 iconSize = ImGui.CalcTextSize(iconText);
            Vector2 iconPos = new Vector2(
                screenPos.X + contentInset + padding,
                screenPos.Y + (height - iconSize.Y) * 0.5f);
            drawList.AddText(iconPos, ImGui.ColorConvertFloat4ToU32(iconColor), iconText);
            ImGui.PopFont();

            float labelX = iconPos.X + iconColumn;
            Vector2 labelSize = ImGui.CalcTextSize(label);
            drawList.AddText(
                new Vector2(labelX, screenPos.Y + (height - labelSize.Y) * 0.5f),
                ImGui.ColorConvertFloat4ToU32(textColor),
                label);

            if (drawChevron)
            {
                ImGui.PushFont(UiBuilder.IconFont);
                FontAwesomeIcon chevronIcon = expanded ? FontAwesomeIcon.ChevronDown : FontAwesomeIcon.ChevronRight;
                string chevron = chevronIcon.ToIconString();
                Vector2 chevronSize = ImGui.CalcTextSize(chevron);
                drawList.AddText(
                    new Vector2(screenPos.X + size.X - chevronColumn - chevronSize.X * 0.5f, screenPos.Y + (height - chevronSize.Y) * 0.5f),
                    ImGui.ColorConvertFloat4ToU32(groupHeader ? HomeUiStyle.Gold : HomeUiStyle.TextMuted),
                    chevron);
                ImGui.PopFont();
            }

            ImGui.SetCursorPos(cursorStart);
            if (indent > 0f)
            {
                ImGui.SetCursorPosX(cursorStart.X + indent);
            }

            ImGui.InvisibleButton(id, size);
            bool clicked = ImGui.IsItemClicked();

            ImGui.SetCursorPos(cursorStart + new Vector2(0f, height + spacing));
            return clicked;
        }
    }
}
