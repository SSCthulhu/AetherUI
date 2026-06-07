using FFXIVHudPlugin.AetherPlates.Data;
using FFXIVHudPlugin.AetherPlates.Layout;
using FFXIVHudPlugin.AetherPlates.Rendering;
using FFXIVHudPlugin.AetherPlates.Configuration;
using System.Collections.Concurrent;
using System.Numerics;

namespace FFXIVHudPlugin.AetherPlates.Widgets.HealthBar;

public sealed class HealthBarWidget : INameplateWidget
{
    private sealed class SmoothState
    {
        public float Displayed;
        public float DamageTrail = 1f;
    }

    private static readonly ConcurrentDictionary<ulong, SmoothState> SmoothStates = new();
    public string Id => "health_bar";

    public Vector2 GetDesiredSize(NameplateContext context)
    {
        return new Vector2(context.Profile.HealthBar.Width, context.Profile.HealthBar.Height);
    }

    public void Draw(NameplateContext context, DrawContext drawContext, WidgetLayout layout)
    {
        var currentRatio = context.Tracked.MaxHp == 0 ? 0f : context.Tracked.CurrentHp / (float)context.Tracked.MaxHp;
        currentRatio = Math.Clamp(currentRatio, 0f, 1f);
        var state = SmoothStates.GetOrAdd(context.Tracked.ObjectId, _ => new SmoothState { Displayed = currentRatio, DamageTrail = currentRatio });
        state.Displayed = Lerp(state.Displayed, currentRatio, 0.20f);
        state.DamageTrail = state.DamageTrail < state.Displayed
            ? state.Displayed
            : Lerp(state.DamageTrail, state.Displayed, 0.08f);

        var min = layout.Position;
        var max = layout.Position + layout.Size;
        var width = max.X - min.X;
        var fillMax = new Vector2(min.X + width * state.Displayed, max.Y);
        var damageMax = new Vector2(min.X + width * state.DamageTrail, max.Y);
        var radius = 4f;

        var healthColor = context.ActiveStyle?.HealthColor ?? 0xFF4AB34A;
        var healthBackgroundColor = context.ActiveStyle?.HealthBackgroundColor ?? context.Profile.HealthBar.BackgroundColor;
        drawContext.DrawFilledRect(min, max, healthBackgroundColor, radius);
        if (damageMax.X > fillMax.X + 0.5f)
        {
            drawContext.DrawFilledRect(new Vector2(fillMax.X, min.Y), damageMax, 0xAA2F2FFF, radius);
        }

        drawContext.DrawFilledRect(min, fillMax, healthColor, radius);
        drawContext.DrawBorder(min, max, context.Profile.HealthBar.BorderColor, radius, 1.3f);

        var shieldMax = min.X + width * Math.Clamp(state.Displayed + context.Tracked.ShieldRatio, 0f, 1f);
        if (shieldMax > fillMax.X + 1f)
        {
            drawContext.DrawFilledRect(new Vector2(fillMax.X, min.Y), new Vector2(shieldMax, max.Y), 0x664AB3E8, radius);
        }

        DrawBossHealthText(context, drawContext, min, max, currentRatio);
        DrawTargetIndicatorOverlay(context, drawContext, min, max);
    }

    private static float Lerp(float from, float to, float amount)
    {
        return from + ((to - from) * Math.Clamp(amount, 0f, 1f));
    }

    private static void DrawTargetIndicatorOverlay(NameplateContext context, DrawContext drawContext, Vector2 min, Vector2 max)
    {
        if (!context.IsTarget || !context.CategoryVisual.TargetIndicatorEnabled)
        {
            return;
        }

        var targetCfg = context.Profile.TargetIndicator;
        var color = ApplyOpacity(targetCfg.Color, Math.Clamp(targetCfg.Opacity, 0f, 1f));
        var style = targetCfg.Style;
        var indicatorScale = Math.Clamp(targetCfg.Scale, 0.25f, 8f);
        var indicatorOffset = targetCfg.Offset;
        var indicatorSize = new Vector2(
            Math.Max(4f, targetCfg.Size.X) * indicatorScale,
            Math.Max(4f, targetCfg.Size.Y) * indicatorScale);
        var width = max.X - min.X;
        var height = max.Y - min.Y;
        var scale = Math.Clamp(context.GlobalScale, 0.5f, 3f);
        var centerY = min.Y + (height * 0.5f);

        switch (style)
        {
            case TargetIndicatorStyle.GlowBorder:
                var glowExpand = new Vector2(
                    MathF.Max(1f, indicatorSize.X * 0.08f),
                    MathF.Max(1f, indicatorSize.Y * 0.16f));
                drawContext.DrawGlow(min + indicatorOffset - glowExpand, max + indicatorOffset + glowExpand, color, 2.5f * scale * indicatorScale);
                drawContext.DrawBorder(min + indicatorOffset - glowExpand * 0.5f, max + indicatorOffset + glowExpand * 0.5f, color, 4f, 1.2f * indicatorScale);
                break;

            case TargetIndicatorStyle.TopArrow:
            {
                using var fontScope = GameFontRegistry.PushFont(context.FontFamilyId);
                var fontSize = Math.Max(8f, indicatorSize.Y * 1.25f) * scale;
                var pos = new Vector2(
                    min.X + (width * 0.5f) - (indicatorSize.X * 0.25f),
                    min.Y - indicatorSize.Y) + indicatorOffset;
                drawContext.DrawText(pos + new Vector2(1f, 1f), 0xCC000000, "▼", fontSize);
                drawContext.DrawText(pos, color, "▼", fontSize);
                break;
            }

            case TargetIndicatorStyle.DoubleSideArrows:
            {
                using var fontScope = GameFontRegistry.PushFont(context.FontFamilyId);
                var fontSize = Math.Max(8f, indicatorSize.Y * 1.25f) * scale;
                var leftPos = new Vector2(min.X - indicatorSize.X, centerY - (indicatorSize.Y * 0.5f)) + indicatorOffset;
                var rightPos = new Vector2(max.X + (indicatorSize.X * 0.15f), centerY - (indicatorSize.Y * 0.5f)) + indicatorOffset;
                drawContext.DrawText(leftPos + new Vector2(1f, 1f), 0xCC000000, ">>", fontSize);
                drawContext.DrawText(rightPos + new Vector2(1f, 1f), 0xCC000000, "<<", fontSize);
                drawContext.DrawText(leftPos, color, ">>", fontSize);
                drawContext.DrawText(rightPos, color, "<<", fontSize);
                break;
            }

            case TargetIndicatorStyle.SideArrows:
            default:
            {
                using var fontScope = GameFontRegistry.PushFont(context.FontFamilyId);
                var fontSize = Math.Max(8f, indicatorSize.Y * 1.25f) * scale;
                var leftPos = new Vector2(min.X - (indicatorSize.X * 0.65f), centerY - (indicatorSize.Y * 0.5f)) + indicatorOffset;
                var rightPos = new Vector2(max.X + (indicatorSize.X * 0.08f), centerY - (indicatorSize.Y * 0.5f)) + indicatorOffset;
                drawContext.DrawText(leftPos + new Vector2(1f, 1f), 0xCC000000, ">", fontSize);
                drawContext.DrawText(rightPos + new Vector2(1f, 1f), 0xCC000000, "<", fontSize);
                drawContext.DrawText(leftPos, color, ">", fontSize);
                drawContext.DrawText(rightPos, color, "<", fontSize);
                break;
            }
        }
    }

    private static uint ApplyOpacity(uint color, float opacity)
    {
        var alpha = (uint)Math.Clamp((int)MathF.Round(((color >> 24) & 0xFF) * opacity), 0, 255);
        return (color & 0x00FFFFFF) | (alpha << 24);
    }

    private static void DrawBossHealthText(
        NameplateContext context,
        DrawContext drawContext,
        Vector2 min,
        Vector2 max,
        float currentRatio)
    {
        if (!context.IsBoss)
        {
            return;
        }

        var showValue = context.CategoryVisual.BossShowHpValueText;
        var showPercent = context.CategoryVisual.BossShowHpPercentText;
        if (!showValue && !showPercent)
        {
            return;
        }

        if (showValue)
        {
            var valueText = $"{context.Tracked.CurrentHp:N0}/{context.Tracked.MaxHp:N0}";
            var valueFontSize = Math.Max(8f, context.CategoryVisual.BossHpValueTextFontSize) * Math.Clamp(context.GlobalScale, 0.5f, 2.5f);
            var valueUseGlobalFont = context.CategoryVisual.BossHpValueTextUseGlobalFont ?? context.CategoryVisual.BossHpValueTextFontFamilyId == 0;
            var valueFontId = valueUseGlobalFont
                ? context.FontFamilyId
                : GameFontRegistry.NormalizeFamilyId(context.CategoryVisual.BossHpValueTextFontFamilyId);
            var valueOffset = context.CategoryVisual.BossHpValueTextOffset;
            var valuePos = new Vector2(min.X + valueOffset.X, min.Y + valueOffset.Y);
            using var valueFontScope = GameFontRegistry.PushFont(valueFontId);
            drawContext.DrawText(valuePos + new Vector2(1f, 1f), 0xCC000000, valueText, valueFontSize);
            drawContext.DrawText(valuePos, 0xFFEAEAEA, valueText, valueFontSize);
        }

        if (showPercent)
        {
            var percentText = $"{MathF.Round(currentRatio * 100f):0.#}%";
            var percentFontSize = Math.Max(8f, context.CategoryVisual.BossHpPercentTextFontSize) * Math.Clamp(context.GlobalScale, 0.5f, 2.5f);
            var percentUseGlobalFont = context.CategoryVisual.BossHpPercentTextUseGlobalFont ?? context.CategoryVisual.BossHpPercentTextFontFamilyId == 0;
            var percentFontId = percentUseGlobalFont
                ? context.FontFamilyId
                : GameFontRegistry.NormalizeFamilyId(context.CategoryVisual.BossHpPercentTextFontFamilyId);
            var percentOffset = context.CategoryVisual.BossHpPercentTextOffset;
            var percentPos = new Vector2(max.X + percentOffset.X, min.Y + percentOffset.Y);
            using var percentFontScope = GameFontRegistry.PushFont(percentFontId);
            drawContext.DrawText(percentPos + new Vector2(1f, 1f), 0xCC000000, percentText, percentFontSize);
            drawContext.DrawText(percentPos, 0xFFEAEAEA, percentText, percentFontSize);
        }
    }
}
