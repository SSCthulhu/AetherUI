using System.Numerics;

namespace DelvUI.Config.Home.Widgets
{
    public static class HomeUiStyle
    {
        public static readonly Vector4 Accent = new(0f / 255f, 162f / 255f, 252f / 255f, 1f);
        public static readonly Vector4 AccentDim = new(0f / 255f, 162f / 255f, 252f / 255f, 0.35f);
        public static readonly Vector4 AccentGlow = new(0f / 255f, 162f / 255f, 252f / 255f, 0.55f);
        public static readonly Vector4 Gold = new(197f / 255f, 160f / 255f, 89f / 255f, 1f);
        public static readonly Vector4 PanelBg = new(24f / 255f, 24f / 255f, 24f / 255f, 0.95f);
        public static readonly Vector4 PanelBorder = new(197f / 255f, 160f / 255f, 89f / 255f, 0.45f);
        public static readonly Vector4 TextMuted = new(0.65f, 0.65f, 0.65f, 1f);
        public static readonly Vector4 PillOn = Accent;
        public static readonly Vector4 PillOff = new(90f / 255f, 78f / 255f, 58f / 255f, 1f);
    }
}
