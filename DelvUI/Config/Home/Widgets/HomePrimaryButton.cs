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
            string? buttonId = null,
            bool advanceLayout = true)
        {
            Vector2 startPos = ImGui.GetCursorPos();
            Vector2 screenPos = ImGui.GetCursorScreenPos();
            string hitId = buttonId ?? $"##homePrimaryCta_{label}";

            if (handleInput)
            {
                ImGui.InvisibleButton(hitId, size);
            }
            else
            {
                ImGui.Dummy(size);
            }

            bool isHovered = handleInput && ImGui.IsItemHovered();
            DrawVisual(screenPos, label, subtitle, icon, size, ImGui.GetWindowDrawList(), isHovered);

            bool clicked = handleInput && ImGui.IsItemClicked();
            if (isHovered && tooltip != null)
            {
                ImGuiHelper.SetTooltip(tooltip);
            }

            if (advanceLayout)
            {
                ImGui.SetCursorPos(startPos + new Vector2(0f, size.Y + 8f));
            }

            return clicked;
        }

        private static void DrawVisual(
            Vector2 screenPos,
            string label,
            string subtitle,
            FontAwesomeIcon icon,
            Vector2 size,
            ImDrawListPtr drawList,
            bool isHovered)
        {
            Vector4 bg = isHovered
                ? new Vector4(HomeUiStyle.Accent.X, HomeUiStyle.Accent.Y, HomeUiStyle.Accent.Z, 0.1f)
                : HomeUiStyle.PanelBg;
            Vector4 border = isHovered ? HomeUiStyle.Accent : HomeUiStyle.AccentGlow;
            float borderThickness = isHovered ? 2.5f : 2f;

            drawList.AddRectFilled(screenPos, screenPos + size, ImGui.ColorConvertFloat4ToU32(bg), 8f);
            drawList.AddRect(
                screenPos,
                screenPos + size,
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

            Vector2 contentOrigin = screenPos + new Vector2(
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
        }
    }
}
