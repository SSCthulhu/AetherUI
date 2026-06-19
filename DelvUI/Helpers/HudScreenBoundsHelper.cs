using Dalamud.Bindings.ImGui;
using System.Numerics;

namespace DelvUI.Helpers
{
    public static class HudScreenBoundsHelper
    {
        public const float DefaultScreenMargin = 8f;

        public static Vector2 GetViewportMin(float margin = DefaultScreenMargin)
        {
            var viewport = ImGui.GetMainViewport();
            return viewport.Pos + new Vector2(margin);
        }

        public static Vector2 GetViewportMax(float margin = DefaultScreenMargin)
        {
            var viewport = ImGui.GetMainViewport();
            return viewport.Pos + viewport.Size - new Vector2(margin);
        }

        /// <summary>
        /// Returns the screen-space delta needed to keep axis-aligned bounds inside the viewport.
        /// When the element is larger than the viewport on an axis, it is pinned to the leading edge
        /// instead of alternating corrections frame-to-frame.
        /// </summary>
        public static Vector2 ComputeClampDelta(Vector2 boundsMin, Vector2 boundsMax, float margin = DefaultScreenMargin)
        {
            Vector2 viewportMin = GetViewportMin(margin);
            Vector2 viewportMax = GetViewportMax(margin);

            Vector2 delta = Vector2.Zero;

            float boundsWidth = boundsMax.X - boundsMin.X;
            float viewportWidth = viewportMax.X - viewportMin.X;
            if (boundsWidth >= viewportWidth)
            {
                delta.X = viewportMin.X - boundsMin.X;
            }
            else if (boundsMax.X > viewportMax.X)
            {
                delta.X = viewportMax.X - boundsMax.X;
            }
            else if (boundsMin.X < viewportMin.X)
            {
                delta.X = viewportMin.X - boundsMin.X;
            }

            float boundsHeight = boundsMax.Y - boundsMin.Y;
            float viewportHeight = viewportMax.Y - viewportMin.Y;
            if (boundsHeight >= viewportHeight)
            {
                delta.Y = viewportMin.Y - boundsMin.Y;
            }
            else if (boundsMax.Y > viewportMax.Y)
            {
                delta.Y = viewportMax.Y - boundsMax.Y;
            }
            else if (boundsMin.Y < viewportMin.Y)
            {
                delta.Y = viewportMin.Y - boundsMin.Y;
            }

            return delta;
        }
    }
}
