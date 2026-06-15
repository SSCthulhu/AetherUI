using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using DelvUI.Config.Home;
using System;
using System.Numerics;

namespace DelvUI.Config.Home.Widgets
{
    public static class HomePillToggle
    {
        private const float EditIconGap = 6f;
        private const float EditIconPad = 4f;

        public static bool Draw(
            string id,
            ref bool value,
            Vector2 size,
            HomeEditTargetId? editTarget = null)
        {
            Vector2 pillCursor = ImGui.GetCursorScreenPos();
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            if (value && editTarget.HasValue)
            {
                DrawEditIcon(drawList, pillCursor, size.Y, id + "_edit", editTarget.Value);
            }

            bool isHovered = ImGui.IsMouseHoveringRect(pillCursor, pillCursor + size);

            Vector4 fill = value ? HomeUiStyle.PillOn : HomeUiStyle.PillOff;
            if (isHovered)
            {
                fill = value
                    ? new Vector4(
                        Math.Min(fill.X + 0.08f, 1f),
                        Math.Min(fill.Y + 0.08f, 1f),
                        Math.Min(fill.Z + 0.08f, 1f),
                        fill.W)
                    : new Vector4(HomeUiStyle.Accent.X, HomeUiStyle.Accent.Y, HomeUiStyle.Accent.Z, 0.12f);
            }

            Vector4 border = isHovered ? HomeUiStyle.AccentGlow : HomeUiStyle.PanelBorder;
            float borderThickness = isHovered ? 1.5f : 1f;

            drawList.AddRectFilled(pillCursor, pillCursor + size, ImGui.ColorConvertFloat4ToU32(fill), size.Y * 0.5f);
            drawList.AddRect(
                pillCursor,
                pillCursor + size,
                ImGui.ColorConvertFloat4ToU32(border),
                size.Y * 0.5f,
                ImDrawFlags.RoundCornersAll,
                borderThickness);

            string label = value ? "ON" : "OFF";
            Vector2 textSize = ImGui.CalcTextSize(label);
            float labelSlotWidth = Math.Max(ImGui.CalcTextSize("ON").X, ImGui.CalcTextSize("OFF").X);
            Vector2 textPos = new Vector2(
                pillCursor.X + (size.X - labelSlotWidth) * 0.5f + (labelSlotWidth - textSize.X) * 0.5f,
                pillCursor.Y + (size.Y - textSize.Y) * 0.5f);
            drawList.AddText(textPos, ImGui.ColorConvertFloat4ToU32(Vector4.One), label);

            ImGui.SetCursorScreenPos(pillCursor);
            ImGui.InvisibleButton(id, size);
            bool changed = false;
            if (ImGui.IsItemClicked())
            {
                value = !value;
                changed = true;
            }

            return changed;
        }

        private static void DrawEditIcon(
            ImDrawListPtr drawList,
            Vector2 pillCursor,
            float pillHeight,
            string id,
            HomeEditTargetId editTarget)
        {
            float scale = ImGuiHelpers.GlobalScale;
            float iconGap = EditIconGap * scale;
            float iconPad = EditIconPad * scale;

            ImGui.PushFont(UiBuilder.IconFont);
            string icon = FontAwesomeIcon.PencilAlt.ToIconString();
            Vector2 iconSize = ImGui.CalcTextSize(icon);
            Vector2 iconPos = new Vector2(
                pillCursor.X - iconGap - iconSize.X,
                pillCursor.Y + (pillHeight - iconSize.Y) * 0.5f);
            Vector2 hitMin = iconPos - new Vector2(iconPad, iconPad);
            Vector2 hitSize = iconSize + new Vector2(iconPad * 2f, iconPad * 2f);
            Vector2 hitMax = hitMin + hitSize;
            bool isHovered = ImGui.IsMouseHoveringRect(hitMin, hitMax);

            if (isHovered)
            {
                Vector4 fill = new Vector4(
                    Math.Min(HomeUiStyle.Accent.X + 0.08f, 1f),
                    Math.Min(HomeUiStyle.Accent.Y + 0.08f, 1f),
                    Math.Min(HomeUiStyle.Accent.Z + 0.08f, 1f),
                    HomeUiStyle.Accent.W);
                float radius = hitSize.Y * 0.5f;
                drawList.AddRectFilled(hitMin, hitMax, ImGui.ColorConvertFloat4ToU32(fill), radius);
                drawList.AddRect(
                    hitMin,
                    hitMax,
                    ImGui.ColorConvertFloat4ToU32(HomeUiStyle.AccentGlow),
                    radius,
                    ImDrawFlags.RoundCornersAll,
                    1.5f);
            }

            Vector4 iconColor = isHovered ? Vector4.One : HomeUiStyle.Accent;
            drawList.AddText(iconPos, ImGui.ColorConvertFloat4ToU32(iconColor), icon);
            ImGui.PopFont();

            ImGui.SetCursorScreenPos(hitMin);
            ImGui.InvisibleButton(id, hitSize);
            if (ImGui.IsItemClicked())
            {
                HomeNavigation.NavigateTo(editTarget);
            }
        }
    }
}
