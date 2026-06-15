using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using System.Numerics;

namespace DelvUI.Config.Home.Widgets
{
    public static class HomeCtaClusterBorder
    {
        private const float CornerRadius = 8f;
        private const float BorderThickness = 2f;

        public static void Draw(Vector2 outerMin, Vector2 outerMax)
        {
            if (outerMax.X <= outerMin.X || outerMax.Y <= outerMin.Y)
            {
                return;
            }

            float inset = BorderThickness * 0.5f;
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            uint borderColor = ImGui.ColorConvertFloat4ToU32(HomeUiStyle.Accent);

            drawList.AddRect(
                new Vector2(outerMin.X + inset, outerMin.Y + inset),
                new Vector2(outerMax.X - inset, outerMax.Y - inset),
                borderColor,
                CornerRadius,
                ImDrawFlags.RoundCornersAll,
                BorderThickness);
        }

        public static Vector2 GetOuterMin(float localLeft, float localTop)
        {
            Vector2 windowPos = ImGui.GetWindowPos();
            return new Vector2(windowPos.X + localLeft, windowPos.Y + localTop);
        }

        public static Vector2 GetOuterMax(float localLeft, float localBottom, float width)
        {
            Vector2 windowPos = ImGui.GetWindowPos();
            return new Vector2(windowPos.X + localLeft + width, windowPos.Y + localBottom);
        }
    }
}
