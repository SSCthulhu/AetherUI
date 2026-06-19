using DelvUI.Config;
using DelvUI.Interface.GeneralElements;
using System;
using System.Numerics;

namespace DelvUI.Helpers
{
    public static class GlobalHudScaleHelper
    {
        // Display scale: what users see on the slider and what is stored in config JSON.
        public const float MinScale = 0.5f;
        public const float MaxScale = 2.5f;
        public const float DefaultScale = 1f;

        // Effective scale: display × multiplier is applied to HUD draw/drag math (ScaleFactor).
        // Display 1.00 renders at the size that display 0.50 used before this baseline shift.
        public const float EffectiveScaleMultiplier = 0.5f;

        public static float ScaleFactor { get; private set; } = DefaultScale * EffectiveScaleMultiplier;

        public static void UpdateFromConfig()
        {
            HUDOptionsConfig? options = ConfigurationManager.Instance.GetConfigObject<HUDOptionsConfig>();
            float displayScale = options?.GlobalHudScale ?? DefaultScale;
            displayScale = Math.Clamp(displayScale, MinScale, MaxScale);
            ScaleFactor = displayScale * EffectiveScaleMultiplier;
        }

        public static float Scale(float value) => value * ScaleFactor;

        public static Vector2 Scale(Vector2 value) => value * ScaleFactor;

        public static Vector2 ApplyOriginOffset(Vector2 origin, Vector2 configOffset) => origin + Scale(configOffset);

        public static float Unscale(float value) => ScaleFactor <= 0f ? value : value / ScaleFactor;

        public static Vector2 Unscale(Vector2 value) => ScaleFactor <= 0f ? value : value / ScaleFactor;
    }
}
