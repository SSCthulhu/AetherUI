using FFXIVHudPlugin.AetherPlates.Data;
using FFXIVHudPlugin.AetherPlates.Layout;
using FFXIVHudPlugin.AetherPlates.Rendering;
using System.Numerics;

namespace FFXIVHudPlugin.AetherPlates.Widgets.TargetIndicator;

public sealed class TargetIndicatorWidget : INameplateWidget
{
    public string Id => "target_indicator";

    public Vector2 GetDesiredSize(NameplateContext context)
    {
        return new Vector2(24f, 12f);
    }

    public void Draw(NameplateContext context, DrawContext drawContext, WidgetLayout layout)
    {
        // Target indicator visuals are now rendered as part of the health bar.
        // Keep this widget as a no-op for backward-compatible layout/config migration.
    }
}
