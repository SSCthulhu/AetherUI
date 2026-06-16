using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures.TextureWraps;
using System;
using System.Numerics;

namespace DelvUI.Config.Home.Widgets
{
    public readonly record struct BrandingImageLayout(Vector2 Size, Vector2 Uv0, Vector2 Uv1);

    public static class HomeBrandingImage
    {
        private const uint OpaqueWhiteTint = 0xFFFFFFFF;

        public static Vector2 GetFitSize(float maxWidth, float maxHeight, int textureWidth, int textureHeight)
        {
            return GetContainLayout(maxWidth, maxHeight, textureWidth, textureHeight).Size;
        }

        /// <summary>
        /// Scales the full texture to fit inside the bounds without cropping.
        /// </summary>
        public static BrandingImageLayout GetContainLayout(
            float maxWidth,
            float maxHeight,
            int textureWidth,
            int textureHeight)
        {
            if (textureWidth <= 0 || textureHeight <= 0 || maxWidth <= 0f || maxHeight <= 0f)
            {
                return new BrandingImageLayout(Vector2.Zero, Vector2.Zero, Vector2.One);
            }

            float aspect = textureHeight / (float)textureWidth;
            float displayWidth = maxWidth;
            float displayHeight = displayWidth * aspect;

            if (displayHeight > maxHeight)
            {
                displayHeight = maxHeight;
                displayWidth = displayHeight / aspect;
            }

            return new BrandingImageLayout(
                new Vector2(displayWidth, displayHeight),
                Vector2.Zero,
                Vector2.One);
        }

        /// <summary>
        /// Fills target width and crops from the top of the texture to fit target height.
        /// </summary>
        public static BrandingImageLayout GetWidthFillTopCropLayout(
            float targetWidth,
            float targetHeight,
            int textureWidth,
            int textureHeight)
        {
            if (textureWidth <= 0 || textureHeight <= 0 || targetWidth <= 0f || targetHeight <= 0f)
            {
                return new BrandingImageLayout(Vector2.Zero, Vector2.Zero, Vector2.One);
            }

            float scaledFullHeight = targetWidth * (textureHeight / (float)textureWidth);
            if (scaledFullHeight <= targetHeight)
            {
                return new BrandingImageLayout(
                    new Vector2(targetWidth, scaledFullHeight),
                    Vector2.Zero,
                    Vector2.One);
            }

            float visibleFraction = targetHeight / scaledFullHeight;
            return new BrandingImageLayout(
                new Vector2(targetWidth, targetHeight),
                Vector2.Zero,
                new Vector2(1f, visibleFraction));
        }

        public static void DrawTextureFullWidth(
            IDalamudTextureWrap texture,
            Vector2 displaySize,
            Vector2 uv0,
            Vector2 uv1)
        {
            if (displaySize.X <= 0f || displaySize.Y <= 0f)
            {
                return;
            }

            Vector2 cursor = ImGui.GetCursorScreenPos();
            Vector2 snappedSize = new Vector2(MathF.Round(displaySize.X), MathF.Round(displaySize.Y));
            Vector2 max = new Vector2(MathF.Round(cursor.X + snappedSize.X), MathF.Round(cursor.Y + snappedSize.Y));
            cursor = new Vector2(MathF.Round(cursor.X), MathF.Round(cursor.Y));

            ImGui.GetWindowDrawList().AddImage(
                texture.Handle,
                cursor,
                max,
                uv0,
                uv1,
                OpaqueWhiteTint);

            ImGui.Dummy(new Vector2(0f, snappedSize.Y));
        }

        public static void DrawTexture(
            IDalamudTextureWrap texture,
            Vector2 displaySize,
            float horizontalInset = 0f)
        {
            DrawTexture(texture, displaySize, Vector2.Zero, Vector2.One, horizontalInset);
        }

        public static void DrawTexture(
            IDalamudTextureWrap texture,
            Vector2 displaySize,
            Vector2 uv0,
            Vector2 uv1,
            float horizontalInset = 0f)
        {
            if (displaySize.X <= 0f || displaySize.Y <= 0f)
            {
                return;
            }

            float availableWidth = ImGui.GetContentRegionAvail().X;
            float xOffset = horizontalInset + Math.Max(0f, (availableWidth - horizontalInset * 2f - displaySize.X) * 0.5f);
            Vector2 cursor = ImGui.GetCursorScreenPos() + new Vector2(xOffset, 0f);
            Vector2 max = cursor + displaySize;

            ImGui.GetWindowDrawList().AddImage(
                texture.Handle,
                cursor,
                max,
                uv0,
                uv1,
                OpaqueWhiteTint);

            ImGui.Dummy(new Vector2(0f, displaySize.Y));
        }
    }
}
