using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using DelvUI;
using System;
using System.Numerics;

namespace DelvUI.Config.Home.Widgets
{
    public static class HomeHeroBanner
    {
        private const float FallbackHeight = 72f;
        private const float TopInset = 4f;
        private const float BottomSpacing = 6f;
        private const float WidthScale = 1.04f;
        private const float ContentZoom = 1.0f;

        private const int BannerTextureWidth = 3840;
        private const int BannerTextureHeight = 480;

        // Opaque art bounds in home_hero_banner.png — updated after 2x export + sharpen.
        private const float ContentU0 = 1014f / 3840f;
        private const float ContentU1 = 2828f / 3840f;
        private const float ContentV0 = 21f / 480f;
        private const float ContentV1 = 458f / 480f;

        public static void Draw(float width)
        {
            Plugin.ReloadHomeHeroBannerIfChanged();

            float scale = ImGuiHelpers.GlobalScale;
            ImGui.Dummy(new Vector2(0f, TopInset * scale));

            float targetWidth = Math.Max(0f, width);
            BrandingImageLayout layout = GetBannerLayout(targetWidth, scale);

            if (layout.Size.X <= 0f || layout.Size.Y <= 0f)
            {
                DrawFallback(targetWidth, FallbackHeight * scale);
                ImGui.Dummy(new Vector2(0f, BottomSpacing * scale));
                return;
            }

            IDalamudTextureWrap? texture = Plugin.HomeHeroBannerTexture?.GetWrapOrDefault();
            if (texture != null)
            {
                HomeBrandingImage.DrawTextureFullWidth(texture, layout.Size, layout.Uv0, layout.Uv1);
            }
            else
            {
                DrawFallback(layout.Size.X, layout.Size.Y);
            }

            ImGui.Dummy(new Vector2(0f, BottomSpacing * scale));
        }

        public static float GetBannerHeight(float width)
        {
            float scale = ImGuiHelpers.GlobalScale;
            BrandingImageLayout layout = GetBannerLayout(Math.Max(0f, width), scale);
            float bannerHeight = layout.Size.Y > 0f ? layout.Size.Y : FallbackHeight * scale;
            return TopInset * scale + bannerHeight + BottomSpacing * scale;
        }

        private static BrandingImageLayout GetBannerLayout(float targetWidth, float scale)
        {
            IDalamudTextureWrap? texture = Plugin.HomeHeroBannerTexture?.GetWrapOrDefault();
            if (texture == null || texture.Width <= 0)
            {
                return new BrandingImageLayout(
                    new Vector2(targetWidth * WidthScale, FallbackHeight * scale),
                    Vector2.Zero,
                    Vector2.One);
            }

            float displayWidth = targetWidth * WidthScale;
            float displayHeight = displayWidth * (texture.Height / (float)texture.Width);
            (Vector2 uv0, Vector2 uv1) = GetContentUv(texture);

            return new BrandingImageLayout(
                new Vector2(displayWidth, displayHeight),
                uv0,
                uv1);
        }

        private static (Vector2 Uv0, Vector2 Uv1) GetContentUv(IDalamudTextureWrap texture)
        {
            if (texture.Width != BannerTextureWidth || texture.Height != BannerTextureHeight)
            {
                return (Vector2.Zero, Vector2.One);
            }

            float uMid = (ContentU0 + ContentU1) * 0.5f;
            float vMid = (ContentV0 + ContentV1) * 0.5f;
            float halfU = (ContentU1 - ContentU0) * 0.5f / ContentZoom;
            float halfV = (ContentV1 - ContentV0) * 0.5f / ContentZoom;

            return (
                new Vector2(uMid - halfU, vMid - halfV),
                new Vector2(uMid + halfU, vMid + halfV));
        }

        private static void DrawFallback(float width, float height)
        {
            Vector2 cursor = ImGui.GetCursorScreenPos();
            Vector2 size = new Vector2(width, height);
            ImGui.Dummy(size);
        }
    }
}
