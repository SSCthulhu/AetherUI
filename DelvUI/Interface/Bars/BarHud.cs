using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Interface.Bars
{
    public class BarHud
    {
        private string ID { get; set; }

        private Rect BackgroundRect { get; set; } = new Rect();

        private List<Rect> ForegroundRects { get; set; } = new List<Rect>();

        private List<LabelHud> LabelHuds { get; set; } = new List<LabelHud>();

        private bool DrawBorder { get; set; }

        private PluginConfigColor? BorderColor { get; set; }

        private int BorderThickness { get; set; }

        private DrawAnchor Anchor { get; set; }

        private IGameObject? Actor { get; set; }

        private PluginConfigColor? GlowColor { get; set; }

        private int GlowSize { get; set; }

        private float? Current;
        private float? Max;

        private ShadowConfig? ShadowConfig { get; set; }

        private string? BarTextureName { get; set; }
        private BarTextureDrawMode BarTextureDrawMode { get; set; }
        private float CornerRounding { get; set; }

        public bool NeedsInputs = false;

        public BarHud(
            string id,
            bool drawBorder = true,
            PluginConfigColor? borderColor = null,
            int borderThickness = 1,
            DrawAnchor anchor = DrawAnchor.TopLeft,
            IGameObject? actor = null,
            PluginConfigColor? glowColor = null,
            int? glowSize = 1,
            float? current = null,
            float? max = null,
            ShadowConfig? shadowConfig = null,
            string? barTextureName = null,
            BarTextureDrawMode barTextureDrawMode = BarTextureDrawMode.Stretch,
            float cornerRounding = 0f)
        {
            ID = id;
            DrawBorder = drawBorder;
            BorderColor = borderColor;
            BorderThickness = borderThickness;
            Anchor = anchor;
            Actor = actor;
            GlowColor = glowColor;
            GlowSize = glowSize ?? 1;
            Current = current;
            Max = max;
            ShadowConfig = shadowConfig;
            BarTextureName = barTextureName;
            BarTextureDrawMode = barTextureDrawMode;
            CornerRounding = cornerRounding;
        }

        public BarHud(BarConfig config, IGameObject? actor = null, BarGlowConfig? glowConfig = null, float? current = null, float? max = null)
            : this(config.ID, 
                  config.DrawBorder, 
                  config.BorderColor, 
                  config.BorderThickness, 
                  config.Anchor, 
                  actor, 
                  glowConfig?.Color, 
                  glowConfig?.Size, 
                  current, 
                  max, 
                  null, 
                  config.BarTextureName, 
                  config.BarTextureDrawMode,
                  config.CornerRounding)
        {
            BackgroundRect = new Rect(config.Position, config.Size, config.BackgroundColor);
            ShadowConfig = config.ShadowConfig;
        }

        public BarHud SetBackground(Rect rect)
        {
            BackgroundRect = rect;
            return this;
        }

        public BarHud AddForegrounds(params Rect[] rects)
        {
            ForegroundRects.AddRange(rects);
            return this;
        }

        public BarHud AddLabels(params LabelConfig[]? labels)
        {
            if (labels != null)
            {
                foreach (LabelConfig config in labels)
                {
                    var labelHud = new LabelHud(config);
                    LabelHuds.Add(labelHud);
                }
            }

            return this;
        }

        public BarHud SetGlow(PluginConfigColor color, int size = 1)
        {
            GlowColor = color;
            GlowSize = size;

            return this;
        }

        public void Draw(Vector2 origin)
        {
            Vector2 scaledBgOffset = GlobalHudScaleHelper.Scale(BackgroundRect.Position);
            Vector2 scaledSize = GlobalHudScaleHelper.Scale(BackgroundRect.Size);
            var barPos = Utils.GetAnchoredPosition(origin, BackgroundRect.Size, Anchor);
            var backgroundPos = barPos + scaledBgOffset;

            DrawRects(barPos, backgroundPos, scaledSize);

            // labels — pass logical bar size; LabelHud applies global scale in anchor math
            foreach (LabelHud label in LabelHuds)
            {
                label.Draw(backgroundPos, BackgroundRect.Size, Actor, null, (uint?)Current, (uint?)Max);
            }
        }

        public List<(StrataLevel, Action)> GetDrawActions(Vector2 origin, StrataLevel strataLevel)
        {
            List<(StrataLevel, Action)> drawActions = new List<(StrataLevel, Action)>();

            Vector2 scaledBgOffset = GlobalHudScaleHelper.Scale(BackgroundRect.Position);
            Vector2 scaledSize = GlobalHudScaleHelper.Scale(BackgroundRect.Size);
            var barPos = Utils.GetAnchoredPosition(origin, BackgroundRect.Size, Anchor);
            var backgroundPos = barPos + scaledBgOffset;

            drawActions.Add((strataLevel, () =>
            {
                DrawRects(barPos, backgroundPos, scaledSize);
            }
            ));

            // labels — pass logical bar size; LabelHud applies global scale in anchor math
            foreach (LabelHud label in LabelHuds)
            {
                drawActions.Add((label.GetConfig().StrataLevel, () =>
                {
                    label.Draw(backgroundPos, BackgroundRect.Size, Actor, null, (uint?)Current, (uint?)Max);
                }
                ));
            }

            return drawActions;
        }

        private void DrawRects(Vector2 barPos, Vector2 backgroundPos, Vector2 scaledBackgroundSize)
        {
            DrawHelper.DrawInWindow(ID, backgroundPos, scaledBackgroundSize, NeedsInputs, (drawList) =>
            {
                float rounding = Math.Clamp(CornerRounding, 0f, Math.Min(scaledBackgroundSize.X, scaledBackgroundSize.Y) / 2f);
                Vector2 shadowOffset = GlobalHudScaleHelper.Scale(new Vector2(ShadowConfig?.Offset ?? 0f));
                float shadowThickness = GlobalHudScaleHelper.Scale(ShadowConfig?.Thickness ?? 0f);
                float borderThickness = GlobalHudScaleHelper.Scale(BorderThickness);
                float glowSize = GlobalHudScaleHelper.Scale(GlowSize);

                // Draw background
                drawList.AddRectFilled(backgroundPos, backgroundPos + scaledBackgroundSize, BackgroundRect.Color.Base, rounding);

                // Draw Shadow
                if (ShadowConfig != null && ShadowConfig.Enabled)
                {
                    // Right Side
                    drawList.AddRectFilled(
                        backgroundPos + new Vector2(scaledBackgroundSize.X, shadowOffset.Y),
                        backgroundPos + scaledBackgroundSize + shadowOffset + new Vector2(shadowThickness - 1f, shadowThickness - 1f),
                        ShadowConfig.Color.Base);

                    // Bottom Size
                    drawList.AddRectFilled(
                        backgroundPos + new Vector2(shadowOffset.X, scaledBackgroundSize.Y),
                        backgroundPos + scaledBackgroundSize + shadowOffset + new Vector2(shadowThickness - 1f, shadowThickness - 1f),
                        ShadowConfig.Color.Base);
                }

                // Draw foregrounds
                foreach (Rect rect in ForegroundRects)
                {
                    DrawHelper.DrawBarTexture(
                        barPos + GlobalHudScaleHelper.Scale(rect.Position),
                        GlobalHudScaleHelper.Scale(rect.Size),
                        rect.Color,
                        BarTextureName,
                        BarTextureDrawMode,
                        drawList,
                        rounding);
                }

                // Draw Border
                if (DrawBorder)
                {
                    drawList.AddRect(backgroundPos, backgroundPos + scaledBackgroundSize, BorderColor?.Base ?? 0xFF000000, rounding, ImDrawFlags.None, borderThickness);
                }

                // Draw Glow
                if (GlowColor != null)
                {
                    var glowPosition = backgroundPos - new Vector2(1f);
                    var glowDrawSize = scaledBackgroundSize + new Vector2(2f);

                    drawList.AddRect(glowPosition, glowPosition + glowDrawSize, GlowColor.Base, 0, ImDrawFlags.None, glowSize);
                }
            });
        }
    }
}
