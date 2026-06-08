namespace FFXIVHudPlugin.AetherPlates.Widgets.CastBar;

using FFXIVHudPlugin.AetherPlates.Data;
using FFXIVHudPlugin.AetherPlates.Layout;
using FFXIVHudPlugin.AetherPlates.Rendering;
using System.Numerics;

public sealed class CastBarWidget : INameplateWidget
{
    public string Id => "cast_bar";

    public Vector2 GetDesiredSize(NameplateContext context)
    {
        return new Vector2(context.Profile.CastBar.Width, context.Profile.CastBar.Height);
    }

    public void Draw(NameplateContext context, DrawContext drawContext, WidgetLayout layout)
    {
        var cast = context.Tracked.CastInfo;
        if (!cast.IsCasting || cast.TotalTime <= 0.001f)
        {
            return;
        }

        var castConfig = context.Profile.CastBar;
        var useCustomColors = context.CategoryVisual.UseCustomCastBarColors;
        var progress = Math.Clamp(cast.CurrentTime / cast.TotalTime, 0f, 1f);
        var min = layout.Position;
        var max = layout.Position + layout.Size;
        var fillMax = new Vector2(min.X + ((max.X - min.X) * progress), max.Y);
        var roundness = Math.Clamp(context.CategoryVisual.CastBarCornerRoundness, 0f, 1f);
        var radius = MathF.Min(max.X - min.X, max.Y - min.Y) * 0.5f * roundness;

        var backgroundColor = useCustomColors ? context.CategoryVisual.CastBarBackgroundColor : castConfig.BackgroundColor;
        var fillColor = useCustomColors ? context.CategoryVisual.CastBarFillColor : castConfig.FillColor;
        var borderColor = useCustomColors ? context.CategoryVisual.CastBarBorderColor : castConfig.BorderColor;

        drawContext.DrawFilledRect(min, max, backgroundColor, radius);
        drawContext.DrawFilledRect(min, fillMax, fillColor, radius);
        drawContext.DrawBorder(min, max, borderColor, radius, 1.2f);

        var stateColor = cast.IsInterruptible
            ? (useCustomColors ? context.CategoryVisual.CastBarInterruptibleColor : castConfig.InterruptibleColor)
            : (useCustomColors ? context.CategoryVisual.CastBarNotInterruptibleColor : castConfig.NotInterruptibleColor);
        drawContext.DrawBorder(min - new Vector2(1f, 1f), max + new Vector2(1f, 1f), stateColor, radius + 1f, 1.1f);

        if (castConfig.ShowSpark)
        {
            var sparkX = fillMax.X;
            drawContext.DrawFilledRect(
                new Vector2(sparkX - 1f, min.Y - 1f),
                new Vector2(sparkX + 1f, max.Y + 1f),
                0xCCFFFFFF,
                1f);
            drawContext.DrawGlow(
                new Vector2(sparkX - 2f, min.Y - 2f),
                new Vector2(sparkX + 2f, max.Y + 2f),
                0x80FFFFFF,
                2f);
        }

        if (castConfig.ShowSafeZoneMarker && castConfig.SafeZoneSeconds > 0.01f && cast.TotalTime > castConfig.SafeZoneSeconds)
        {
            var safeStart = Math.Clamp((cast.TotalTime - castConfig.SafeZoneSeconds) / cast.TotalTime, 0f, 1f);
            var safeX = min.X + ((max.X - min.X) * safeStart);
            drawContext.DrawFilledRect(
                new Vector2(safeX - 1f, min.Y - 2f),
                new Vector2(safeX + 1f, max.Y + 2f),
                0xFF4DCC4D,
                1f);
        }
    }
}
