using Dalamud.Bindings.ImGui;
using DelvUI.Config.Home.Widgets;
using System.Numerics;

namespace DelvUI.Config.Windows
{
    public static class ConfigWindowChrome
    {
        public const float BorderThickness = 1f;

        public static uint GetGoldBorderColor(float alpha = 1f)
        {
            Vector4 borderColor = HomeUiStyle.Gold with { W = alpha };
            return ImGui.ColorConvertFloat4ToU32(borderColor);
        }

        public static void DrawWindowBorder(float alpha = 1f)
        {
            Vector2 pos = ImGui.GetWindowPos();
            Vector2 size = ImGui.GetWindowSize();
            uint color = GetGoldBorderColor(alpha);
            ImDrawListPtr drawList = ImGui.GetForegroundDrawList();

            float half = BorderThickness * 0.5f;
            Vector2 min = pos + new Vector2(half, half);
            Vector2 max = pos + size - new Vector2(half, half);

            drawList.AddLine(min, new Vector2(max.X, min.Y), color, BorderThickness);
            drawList.AddLine(new Vector2(max.X, min.Y), max, color, BorderThickness);
            drawList.AddLine(max, new Vector2(min.X, max.Y), color, BorderThickness);
            drawList.AddLine(new Vector2(min.X, max.Y), min, color, BorderThickness);
        }

        public static void DrawSidebarDivider(float sidebarWidth, float alpha = 1f)
        {
            Vector2 windowPos = ImGui.GetWindowPos();
            Vector2 windowSize = ImGui.GetWindowSize();
            Vector2 contentOrigin = windowPos + ImGui.GetWindowContentRegionMin();
            float dividerX = contentOrigin.X + sidebarWidth;
            uint color = GetGoldBorderColor(alpha);

            ImGui.GetForegroundDrawList().AddLine(
                new Vector2(dividerX, windowPos.Y),
                new Vector2(dividerX, windowPos.Y + windowSize.Y),
                color,
                BorderThickness);
        }
    }
}
