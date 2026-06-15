using Dalamud.Interface;
using DelvUI.Helpers;
using Dalamud.Bindings.ImGui;
using System;
using System.Numerics;

namespace DelvUI.Config.Home.Widgets
{
    public static class HomePrimaryButton
    {
        private const float IconTextGap = 10f;
        private const float SubtitleGap = 4f;

        public static bool Draw(
            string label,
            string subtitle,
            FontAwesomeIcon icon,
            Vector2 size,
            string? tooltip = null,
            bool handleInput = true,
            string? buttonId = null)
        {
            Vector2 startPos = ImGui.GetCursorPos();
            Vector2 cursor = ImGui.GetCursorScreenPos();
            bool isHovered = ImGui.IsMouseHoveringRect(cursor, cursor + size);
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            Vector4 bg = isHovered
                ? new Vector4(HomeUiStyle.Accent.X, HomeUiStyle.Accent.Y, HomeUiStyle.Accent.Z, 0.1f)
                : HomeUiStyle.PanelBg;
            Vector4 border = isHovered ? HomeUiStyle.Accent : HomeUiStyle.AccentGlow;
            float borderThickness = isHovered ? 2.5f : 2f;

            drawList.AddRectFilled(cursor, cursor + size, ImGui.ColorConvertFloat4ToU32(bg), 8f);
            drawList.AddRect(
                cursor,
                cursor + size,
                ImGui.ColorConvertFloat4ToU32(border),
                8f,
                ImDrawFlags.RoundCornersAll,
                borderThickness);

            string title = label.ToUpperInvariant();
            bool hasSubtitle = !string.IsNullOrWhiteSpace(subtitle);
            string caption = hasSubtitle ? subtitle.ToUpperInvariant() : string.Empty;
            Vector2 titleSize = ImGui.CalcTextSize(title);
            Vector2 captionSize = hasSubtitle ? ImGui.CalcTextSize(caption) : Vector2.Zero;

            ImGui.PushFont(UiBuilder.IconFont);
            Vector2 iconSize = ImGui.CalcTextSize(icon.ToIconString());
            ImGui.PopFont();

            float textBlockWidth = hasSubtitle ? Math.Max(titleSize.X, captionSize.X) : titleSize.X;
            float contentWidth = iconSize.X + IconTextGap + textBlockWidth;
            float contentHeight = hasSubtitle
                ? titleSize.Y + SubtitleGap + captionSize.Y
                : titleSize.Y;

            Vector2 contentOrigin = cursor + new Vector2(
                (size.X - contentWidth) * 0.5f,
                (size.Y - contentHeight) * 0.5f);

            Vector4 accentColor = isHovered ? Vector4.One : HomeUiStyle.Accent;
            Vector4 captionColor = isHovered ? HomeUiStyle.Accent : HomeUiStyle.TextMuted;

            ImGui.PushFont(UiBuilder.IconFont);
            drawList.AddText(
                contentOrigin + new Vector2(0f, (contentHeight - iconSize.Y) * 0.5f),
                ImGui.ColorConvertFloat4ToU32(accentColor),
                icon.ToIconString());
            ImGui.PopFont();

            Vector2 textOrigin = contentOrigin + new Vector2(iconSize.X + IconTextGap, 0f);
            drawList.AddText(textOrigin, ImGui.ColorConvertFloat4ToU32(accentColor), title);
            if (hasSubtitle)
            {
                drawList.AddText(
                    textOrigin + new Vector2(0f, titleSize.Y + SubtitleGap),
                    ImGui.ColorConvertFloat4ToU32(captionColor),
                    caption);
            }

            string hitId = buttonId ?? $"##homePrimaryCta_{label}";
            ImGui.SetCursorPos(startPos);
            ImGui.InvisibleButton(hitId, size);

            if (handleInput)
            {
                bool clicked = ImGui.IsItemClicked();
                if (ImGui.IsItemHovered() && tooltip != null)
                {
                    ImGuiHelper.SetTooltip(tooltip);
                }

                ImGui.SetCursorPos(startPos + new Vector2(0f, size.Y + 8f));
                return clicked;
            }

            ImGui.SetCursorPos(startPos + new Vector2(0f, size.Y + 8f));
            return false;
        }
    }
}
