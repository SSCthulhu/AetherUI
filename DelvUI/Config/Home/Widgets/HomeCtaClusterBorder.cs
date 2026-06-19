using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using System.Numerics;

namespace DelvUI.Config.Home.Widgets
{
    public static class HomeCtaClusterBorder
    {
        private const float CornerRadius = 8f;
        public const float BorderThickness = 2f;

        public static float GetDrawInset(float scale) => BorderThickness * 0.5f * scale;

        public static void Draw(Vector2 outerMin, Vector2 outerMax, ImDrawListPtr? drawList = null)
        {
            if (outerMax.X <= outerMin.X || outerMax.Y <= outerMin.Y)
            {
                return;
            }

            float inset = GetDrawInset(ImGuiHelpers.GlobalScale);
            ImDrawListPtr list = drawList ?? ImGui.GetWindowDrawList();
            uint borderColor = ImGui.ColorConvertFloat4ToU32(HomeUiStyle.Accent);

            list.AddRect(
                new Vector2(outerMin.X + inset, outerMin.Y + inset),
                new Vector2(outerMax.X - inset, outerMax.Y - inset),
                borderColor,
                CornerRadius,
                ImDrawFlags.RoundCornersAll,
                BorderThickness);
        }
    }
}
