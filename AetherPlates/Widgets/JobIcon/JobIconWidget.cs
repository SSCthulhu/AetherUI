using Dalamud.Interface.Textures;
using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVHudPlugin.AetherPlates.Data;
using FFXIVHudPlugin.AetherPlates.Layout;
using FFXIVHudPlugin.AetherPlates.Rendering;
using FFXIVHudPlugin.AetherPlates.Configuration;
using FFXIVHudPlugin.AetherPlates.Widgets.NameText;
using System.Collections.Concurrent;
using System.Numerics;

namespace FFXIVHudPlugin.AetherPlates.Widgets.JobIcon;

public sealed class JobIconWidget : INameplateWidget
{
    private static readonly ConcurrentDictionary<uint, ISharedImmediateTexture?> JobIconCache = new();
    public string Id => "job_icon";

    public Vector2 GetDesiredSize(NameplateContext context)
    {
        return new Vector2(20f, 20f);
    }

    public void Draw(NameplateContext context, DrawContext drawContext, WidgetLayout layout)
    {
        if (context.Tracked.Kind != ObjectKind.Pc)
        {
            return;
        }

        var iconId = context.Tracked.JobIconId;
        if (iconId == 0)
        {
            return;
        }

        var resolvedIconId = context.CategoryVisual.JobIconType == NameplateJobIconType.Type2
            ? iconId + 100u
            : iconId;

        var texture = JobIconCache.GetOrAdd(
            resolvedIconId,
            static (id, ctx) => ctx.TextureProvider.GetFromGameIcon(new GameIconLookup(id)),
            context);
        if (texture is null)
        {
            return;
        }

        var min = ResolvePositionAnchoredToNameText(context, layout);
        var max = min + layout.Size;
        drawContext.DrawImage(texture, min, max, 0xFFFFFFFF);
    }

    private static Vector2 ResolvePositionAnchoredToNameText(NameplateContext context, WidgetLayout iconLayout)
    {
        if (NameTextLayoutCache.TryGetCurrent(context.Tracked.ObjectId, out var nameText))
        {
            var gap = context.CategoryVisual.JobIconNameTextGap * Math.Clamp(context.GlobalScale, 0.5f, 3.0f);
            var x = context.CategoryVisual.JobIconNameTextEdge == NameplateTextEdge.Right
                ? nameText.Position.X + nameText.Size.X + gap
                : nameText.Position.X - iconLayout.Size.X - gap;
            var y = nameText.Position.Y + MathF.Max(0f, (nameText.Size.Y - iconLayout.Size.Y) * 0.5f);
            return new Vector2(x, y);
        }

        // Fallback when name text widget is disabled or not drawn.
        var fallbackGap = context.CategoryVisual.JobIconNameTextGap * Math.Clamp(context.GlobalScale, 0.5f, 3.0f);
        return context.CategoryVisual.JobIconNameTextEdge == NameplateTextEdge.Right
            ? new Vector2(iconLayout.Position.X + fallbackGap, iconLayout.Position.Y)
            : new Vector2(iconLayout.Position.X - fallbackGap, iconLayout.Position.Y);
    }
}
