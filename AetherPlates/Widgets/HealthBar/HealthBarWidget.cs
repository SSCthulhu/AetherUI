using FFXIVHudPlugin.AetherPlates.Data;
using FFXIVHudPlugin.AetherPlates.Layout;
using FFXIVHudPlugin.AetherPlates.Rendering;
using FFXIVHudPlugin.AetherPlates.Configuration;
using Dalamud.Bindings.ImGui;
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
        var borderInset = 1f;
        var innerMin = new Vector2(min.X + borderInset, min.Y + borderInset);
        var innerMax = new Vector2(MathF.Max(innerMin.X, max.X - borderInset), MathF.Max(innerMin.Y, max.Y - borderInset));
        var width = innerMax.X - innerMin.X;
        var fillMax = new Vector2(innerMin.X + width * state.Displayed, innerMax.Y);
        var damageMax = new Vector2(innerMin.X + width * state.DamageTrail, innerMax.Y);
        var roundness = Math.Clamp(context.CategoryVisual.HealthBarCornerRoundness, 0f, 1f);
        var radius = MathF.Min(max.X - min.X, max.Y - min.Y) * 0.5f * roundness;
        var innerRadius = MathF.Max(0f, radius - borderInset);

        var healthColor = context.CategoryVisual.UseCustomHealthBarColors
            ? context.CategoryVisual.HealthBarFillColor
            : (context.ActiveStyle?.HealthColor ?? 0xFF4AB34A);
        var healthBackgroundColor = context.CategoryVisual.UseCustomHealthBarColors
            ? context.CategoryVisual.HealthBarBackgroundColor
            : (context.ActiveStyle?.HealthBackgroundColor ?? context.Profile.HealthBar.BackgroundColor);
        var healthBorderColor = context.CategoryVisual.UseCustomHealthBarColors
            ? context.CategoryVisual.HealthBarBorderColor
            : context.Profile.HealthBar.BorderColor;
        drawContext.DrawFilledRect(innerMin, innerMax, healthBackgroundColor, innerRadius);
        var hasDamageTrail = damageMax.X > fillMax.X + 0.5f;
        // Trail underlay: starts from the left edge so HP overlays it cleanly.
        if (damageMax.X > innerMin.X + 0.5f)
        {
            var trailColor = 0xAA2F2FFFu;
            // Keep the underlay's outer-left contour aligned with the bar shell so no color leaks
            // through anti-aliased edge pixels on the left.
            var trailFlags = ImDrawFlags.RoundCornersAll;
            drawContext.DrawFilledRect(innerMin, damageMax, trailColor, innerRadius, trailFlags);
        }

        if (fillMax.X > innerMin.X + 0.5f)
        {
            var fillFlags = hasDamageTrail
                ? ImDrawFlags.RoundCornersLeft
                : ImDrawFlags.RoundCornersAll;
            drawContext.DrawFilledRect(innerMin, fillMax, healthColor, innerRadius, fillFlags);
        }
        drawContext.DrawBorder(min, max, healthBorderColor, radius, 1.3f);

        var shieldMax = innerMin.X + width * Math.Clamp(state.Displayed + context.Tracked.ShieldRatio, 0f, 1f);
        if (shieldMax > fillMax.X + 1f)
        {
            drawContext.DrawFilledRect(new Vector2(fillMax.X, innerMin.Y), new Vector2(shieldMax, innerMax.Y), 0x664AB3E8, innerRadius);
        }

        DrawBossHealthText(context, drawContext, min, max, currentRatio);
        DrawTargetIndicatorOverlay(context, drawContext, min, max, radius);
    }

    private static float Lerp(float from, float to, float amount)
    {
        return from + ((to - from) * Math.Clamp(amount, 0f, 1f));
    }

    private static void DrawTargetIndicatorOverlay(NameplateContext context, DrawContext drawContext, Vector2 min, Vector2 max, float barRadius)
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
        var centerWithHealth = context.CategoryVisual.TargetIndicatorCenterWithHealthBar;
        var indicatorSize = new Vector2(
            Math.Max(4f, targetCfg.Size.X) * indicatorScale,
            Math.Max(4f, targetCfg.Size.Y) * indicatorScale);
        var width = max.X - min.X;
        var height = max.Y - min.Y;
        var scale = Math.Clamp(context.GlobalScale, 0.5f, 3f);
        var centerY = min.Y + (height * 0.5f);
        if (centerWithHealth)
        {
            // Preserve X offset, but apply Y from the health-bar centerline baseline.
            indicatorOffset.Y = 0f;
        }

        switch (style)
        {
            case TargetIndicatorStyle.GlowBorder:
                var glowExpand = new Vector2(
                    MathF.Max(1f, indicatorSize.X * 0.08f),
                    MathF.Max(1f, indicatorSize.Y * 0.16f));
                var glowRadius = barRadius + MathF.Max(glowExpand.X, glowExpand.Y);
                var borderRadius = barRadius + (MathF.Max(glowExpand.X, glowExpand.Y) * 0.5f);
                drawContext.DrawGlow(min + indicatorOffset - glowExpand, max + indicatorOffset + glowExpand, color, glowRadius);
                drawContext.DrawBorder(min + indicatorOffset - glowExpand * 0.5f, max + indicatorOffset + glowExpand * 0.5f, color, borderRadius, 1.2f * indicatorScale);
                break;

            case TargetIndicatorStyle.TopArrow:
            {
                using var fontScope = GameFontRegistry.PushFont(context.FontFamilyId);
                const string glyph = "▼";
                var fontSize = Math.Max(8f, indicatorSize.Y * 1.25f) * scale;
                var glyphSize = MeasureText(glyph, fontSize);
                var baseY = centerWithHealth
                    ? centerY - (glyphSize.Y * 0.5f)
                    : min.Y - glyphSize.Y;
                var pos = new Vector2(
                    min.X + (width * 0.5f) - (glyphSize.X * 0.5f),
                    baseY) + indicatorOffset;
                drawContext.DrawText(pos + new Vector2(1f, 1f), 0xCC000000, glyph, fontSize);
                drawContext.DrawText(pos, color, glyph, fontSize);
                break;
            }

            case TargetIndicatorStyle.DoubleSideArrows:
            {
                using var fontScope = GameFontRegistry.PushFont(context.FontFamilyId);
                var fontSize = Math.Max(8f, indicatorSize.Y * 1.25f) * scale;
                var leftGlyph = ">>";
                var rightGlyph = "<<";
                var leftGlyphSize = MeasureText(leftGlyph, fontSize);
                var rightGlyphSize = MeasureText(rightGlyph, fontSize);
                var y = centerY - (leftGlyphSize.Y * 0.5f);
                var leftPos = new Vector2(min.X - indicatorSize.X, y) + indicatorOffset;
                var rightPos = new Vector2(max.X + (indicatorSize.X * 0.15f), centerY - (rightGlyphSize.Y * 0.5f)) + indicatorOffset;
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
                var leftGlyphSize = MeasureText(">", fontSize);
                var rightGlyphSize = MeasureText("<", fontSize);
                var leftPos = new Vector2(min.X - (indicatorSize.X * 0.65f), centerY - (leftGlyphSize.Y * 0.5f)) + indicatorOffset;
                var rightPos = new Vector2(max.X + (indicatorSize.X * 0.08f), centerY - (rightGlyphSize.Y * 0.5f)) + indicatorOffset;
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

    private static Vector2 MeasureText(string text, float fontSize)
    {
        var baseFontSize = Math.Max(1f, ImGui.GetFontSize());
        return ImGui.CalcTextSize(text) * (fontSize / baseFontSize);
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
