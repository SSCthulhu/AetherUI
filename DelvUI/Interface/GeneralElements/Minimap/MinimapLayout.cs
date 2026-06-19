using System.Numerics;
using System;
using DelvUI.Helpers;

namespace DelvUI.Interface.GeneralElements
{
    internal static class MinimapLayout
    {
        public const float DefaultSize = 168f;
        public const float DefaultOffsetX = 780f;
        public const float DefaultOffsetY = -420f;
        public const float DefaultVisibleRangeYalms = 46f;
        public const float MinSize = 96f;
        public const float MaxSize = 600f;
        public const float MinVisibleRangeYalms = 12f;
        public const float MaxVisibleRangeYalms = 120f;
        public const float DefaultFacingConeSizeScale = 0.72f;
        public const float MinFacingConeSizeScale = 0.15f;
        public const float MaxFacingConeSizeScale = 1.15f;
        public const float DefaultFacingConeOpacity = 0.6f;
        public const float MinFacingConeOpacity = 0.05f;
        public const float MaxFacingConeOpacity = 1f;
        public const float DefaultBorderThickness = 3f;
        public const uint DefaultBorderColor = 0xFF000000;
        public const float MinBorderThickness = 0f;
        public const float MaxBorderThickness = 12f;

        public const int MaxNativeMarkersPerFrame = 64;
        public const int MaxNativeMarkerIconLoadsPerFrame = 24;
        public const float DefaultMarkerIconSize = 20f;
        public const float MinMarkerIconSize = 20f;
        public const float MaxMarkerIconSize = 80f;
        public const float DefaultPlayerPinSize = 12f;
        public const float MinPlayerPinSize = 8f;
        public const float MaxPlayerPinSize = 22f;
        public const uint DefaultPlayerPinColor = 0xFFE87848;

        private const float OffsetEdgePadding = 96f;

        public static float ClampSize(float size) => Math.Clamp(size, MinSize, MaxSize);
        public static float ClampVisibleRange(float range) => Math.Clamp(range, MinVisibleRangeYalms, MaxVisibleRangeYalms);
        public static float ClampFacingConeSizeScale(float scale) => Math.Clamp(scale, MinFacingConeSizeScale, MaxFacingConeSizeScale);
        public static float ClampFacingConeOpacity(float opacity) => Math.Clamp(opacity, MinFacingConeOpacity, MaxFacingConeOpacity);
        public static float ClampBorderThickness(float thickness) => Math.Clamp(thickness, MinBorderThickness, MaxBorderThickness);
        public static float ClampMarkerIconSize(float size) => Math.Clamp(size, MinMarkerIconSize, MaxMarkerIconSize);
        public static float ClampPlayerPinSize(float size) => Math.Clamp(size, MinPlayerPinSize, MaxPlayerPinSize);

        public static float GetScaledSize(float size) => GlobalHudScaleHelper.Scale(ClampSize(size));

        public static float GetScaledContentHalf(float size) => GetScaledSize(size) * 0.5f;

        public static float GetScaledMarkerIconSize(float size) => GlobalHudScaleHelper.Scale(ClampMarkerIconSize(size));

        public static float GetScaledPlayerPinSize(float size) => GlobalHudScaleHelper.Scale(ClampPlayerPinSize(size));

        public static ScreenOffsetBounds GetOffsetBounds(Vector2 viewportSize, float minimapSize)
        {
            var halfMinimap = GetScaledContentHalf(minimapSize);
            var maxX = (viewportSize.X * 0.5f) + halfMinimap + OffsetEdgePadding;
            var maxY = (viewportSize.Y * 0.5f) + halfMinimap + OffsetEdgePadding;
            return new ScreenOffsetBounds(-maxX, maxX, -maxY, maxY);
        }

        public static float ClampOffsetX(float value, Vector2 viewportSize, float minimapSize) =>
            GetOffsetBounds(viewportSize, minimapSize).ClampX(value);

        public static float ClampOffsetY(float value, Vector2 viewportSize, float minimapSize) =>
            GetOffsetBounds(viewportSize, minimapSize).ClampY(value);
    }

    internal readonly record struct ScreenOffsetBounds(float MinX, float MaxX, float MinY, float MaxY)
    {
        public float ClampX(float value) => Math.Clamp(value, MinX, MaxX);
        public float ClampY(float value) => Math.Clamp(value, MinY, MaxY);
    }
}
