using FFXIVHudPlugin.AetherPlates.Data;
using FFXIVHudPlugin.AetherPlates.Styles;
using System.Numerics;

namespace FFXIVHudPlugin.AetherPlates.Layout;

public sealed class LayoutEngine
{
    public WidgetLayout Calculate(
        NameplateContext context,
        NameplateStyle style,
        string widgetId,
        Vector2 desiredSize)
    {
        if (!context.CategoryVisual.WidgetLayouts.TryGetValue(widgetId, out var rule))
        {
            rule = WidgetLayoutRule.Default(widgetId);
        }

        var scale = Math.Clamp(context.GlobalScale, 0.5f, 3.0f);
        var baseSize = new Vector2(
            rule.Size.X > 0f ? rule.Size.X : desiredSize.X,
            rule.Size.Y > 0f ? rule.Size.Y : desiredSize.Y);
        var size = baseSize * scale;
        var scaledOffset = rule.Offset * scale;
        var anchorPosition = ResolveAnchorPosition(context.AnchorScreenPosition, size, rule.Anchor);
        var finalPosition = anchorPosition + scaledOffset;
        finalPosition = ApplyHealthBarCenterYAlignment(context, widgetId, finalPosition, size, scale);

        return new WidgetLayout(
            widgetId,
            rule.Anchor,
            scaledOffset,
            size,
            finalPosition,
            rule.Visible);
    }

    private static Vector2 ApplyHealthBarCenterYAlignment(
        NameplateContext context,
        string widgetId,
        Vector2 currentPosition,
        Vector2 currentSize,
        float scale)
    {
        var shouldCenter = widgetId switch
        {
            "buff_row" => context.CategoryVisual.BuffRowCenterWithHealthBar,
            "debuff_row" => context.CategoryVisual.DebuffRowCenterWithHealthBar,
            _ => false,
        };

        if (!shouldCenter)
        {
            return currentPosition;
        }

        if (!context.CategoryVisual.WidgetLayouts.TryGetValue("health_bar", out var healthRule))
        {
            healthRule = WidgetLayoutRule.Default("health_bar");
        }

        var healthBaseSize = healthRule.Size;
        if (healthBaseSize.X <= 0f || healthBaseSize.Y <= 0f)
        {
            healthBaseSize = new Vector2(context.Profile.HealthBar.Width, context.Profile.HealthBar.Height);
        }

        var healthSize = healthBaseSize * scale;
        var healthAnchorPosition = ResolveAnchorPosition(context.AnchorScreenPosition, healthSize, healthRule.Anchor);
        var healthPosition = healthAnchorPosition + (healthRule.Offset * scale);
        var healthCenterY = healthPosition.Y + (healthSize.Y * 0.5f);
        var centeredY = healthCenterY - (currentSize.Y * 0.5f);
        return new Vector2(currentPosition.X, centeredY);
    }

    private static Vector2 ResolveAnchorPosition(Vector2 center, Vector2 size, WidgetAnchor anchor)
    {
        var half = size * 0.5f;
        return anchor switch
        {
            WidgetAnchor.Top => new Vector2(center.X - half.X, center.Y - size.Y),
            WidgetAnchor.Bottom => new Vector2(center.X - half.X, center.Y),
            WidgetAnchor.Left => new Vector2(center.X - size.X, center.Y - half.Y),
            WidgetAnchor.Right => new Vector2(center.X, center.Y - half.Y),
            WidgetAnchor.Center => new Vector2(center.X - half.X, center.Y - half.Y),
            WidgetAnchor.TopLeft => new Vector2(center.X - size.X, center.Y - size.Y),
            WidgetAnchor.TopRight => new Vector2(center.X, center.Y - size.Y),
            WidgetAnchor.BottomLeft => new Vector2(center.X - size.X, center.Y),
            WidgetAnchor.BottomRight => center,
            _ => center,
        };
    }
}
