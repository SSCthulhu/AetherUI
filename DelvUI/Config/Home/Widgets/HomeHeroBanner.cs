using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using DelvUI;
using System.Numerics;

namespace DelvUI.Config.Home.Widgets
{
    public static class HomeHeroBanner
    {
        private const float FallbackHeight = 72f;
        private const float TopInset = 10f;
        private const float CornerRadius = 6f;
        private const float BorderThickness = 1f;

        public static void Draw(float width)
        {
            ImGui.Dummy(new Vector2(0f, TopInset * ImGuiHelpers.GlobalScale));

            Vector2 cursor = ImGui.GetCursorScreenPos();
            float height = GetBannerHeight(width);
            Vector2 size = new Vector2(width, height);
            Vector2 max = cursor + size;
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            IDalamudTextureWrap? texture = Plugin.HomeHeroBannerTexture?.GetWrapOrDefault();
            if (texture != null)
            {
                drawList.AddImageRounded(
                    texture.Handle,
                    cursor,
                    max,
                    Vector2.Zero,
                    Vector2.One,
                    ImGui.ColorConvertFloat4ToU32(Vector4.One),
                    CornerRadius);
            }
            else
            {
                DrawFallback(drawList, cursor, size);
            }

            drawList.AddRect(
                cursor,
                max,
                ImGui.ColorConvertFloat4ToU32(HomeUiStyle.PanelBorder),
                CornerRadius,
                ImDrawFlags.RoundCornersAll,
                BorderThickness);

            ImGui.Dummy(size);
            ImGui.SetCursorScreenPos(cursor + new Vector2(0f, height + 6f * ImGuiHelpers.GlobalScale));
        }

        public static float GetBannerHeight(float width)
        {
            IDalamudTextureWrap? texture = Plugin.HomeHeroBannerTexture?.GetWrapOrDefault();
            if (texture == null || texture.Width <= 0)
            {
                return FallbackHeight * ImGuiHelpers.GlobalScale;
            }

            return width * ((float)texture.Height / texture.Width);
        }

        private static void DrawFallback(ImDrawListPtr drawList, Vector2 cursor, Vector2 size)
        {
            Vector2 max = cursor + size;
            uint topColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.02f, 0.08f, 0.14f, 0.95f));
            uint bottomColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.04f, 0.04f, 0.04f, 0.15f));
            drawList.AddRectFilledMultiColor(cursor, max, topColor, topColor, bottomColor, bottomColor);

            ImGui.SetCursorScreenPos(cursor + new Vector2(20f, 14f));
            ImGui.PushStyleColor(ImGuiCol.Text, HomeUiStyle.Accent);
            ImGui.SetWindowFontScale(1.25f);
            ImGui.Text("WELCOME TO AETHER UI");
            ImGui.SetWindowFontScale(1f);
            ImGui.PopStyleColor();

            ImGui.SetCursorScreenPos(cursor + new Vector2(20f, 40f));
            ImGui.PushStyleColor(ImGuiCol.Text, HomeUiStyle.Gold);
            ImGui.Text("CONFIGURE YOUR HUD IN MINUTES");
            ImGui.PopStyleColor();
        }
    }
}
