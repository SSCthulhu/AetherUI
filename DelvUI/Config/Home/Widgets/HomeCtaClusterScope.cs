using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using System;
using System.Numerics;

namespace DelvUI.Config.Home.Widgets
{
    public struct HomeCtaClusterScope : IDisposable
    {
        public const float SidePadding = 12f;

        private const float BottomPadding = 12f;
        private const float InnerTopPadding = 8f;

        public static float GetTotalLayoutHeight(float innerBlockHeight)
        {
            float scale = ImGuiHelpers.GlobalScale;
            return (InnerTopPadding + BottomPadding) * scale
                + innerBlockHeight
                + HomeBorderedSectionScope.BottomAdvance * scale;
        }

        private readonly float _borderLeftScreenX;
        private readonly float _borderTopScreenY;
        private readonly float _outerWidth;
        private readonly float _bottomPad;
        private readonly float _sectionStartLocalX;
        private float _borderBottomLocalY;

        public float InnerWidth { get; }
        public float BorderBottomLocalY => _borderBottomLocalY;

        public static HomeCtaClusterScope Begin(float outerWidth)
        {
            float scale = ImGuiHelpers.GlobalScale;
            float sidePad = SidePadding * scale;
            float bottomPad = BottomPadding * scale;
            float innerTopPad = InnerTopPadding * scale;
            float innerWidth = Math.Max(0f, outerWidth - sidePad * 2f);

            Vector2 sectionStart = ImGui.GetCursorPos();
            Vector2 sectionStartScreen = ImGui.GetCursorScreenPos();

            ImGui.PushID("homeCtaCluster");
            ImGui.BeginGroup();
            ImGui.Dummy(new Vector2(outerWidth, innerTopPad));
            ImGui.SetCursorPos(new Vector2(sectionStart.X + sidePad, ImGui.GetCursorPosY()));

            return new HomeCtaClusterScope(
                sectionStartScreen.X,
                sectionStartScreen.Y,
                sectionStart.X,
                outerWidth,
                innerWidth,
                bottomPad);
        }

        private HomeCtaClusterScope(
            float borderLeftScreenX,
            float borderTopScreenY,
            float sectionStartLocalX,
            float outerWidth,
            float innerWidth,
            float bottomPad)
        {
            _borderLeftScreenX = borderLeftScreenX;
            _borderTopScreenY = borderTopScreenY;
            _sectionStartLocalX = sectionStartLocalX;
            _outerWidth = outerWidth;
            InnerWidth = innerWidth;
            _bottomPad = bottomPad;
        }

        public void Dispose()
        {
            ImGui.Dummy(new Vector2(InnerWidth, _bottomPad));
            float bottomLocalY = ImGui.GetCursorPosY();
            ImGui.Dummy(new Vector2(_outerWidth, 0f));
            ImGui.EndGroup();

            Vector2 contentMax = ImGui.GetItemRectMax();
            Vector2 outerMin = new Vector2(_borderLeftScreenX, _borderTopScreenY);
            Vector2 outerMax = new Vector2(_borderLeftScreenX + _outerWidth, contentMax.Y);
            HomeCtaClusterBorder.Draw(outerMin, outerMax);

            _borderBottomLocalY = bottomLocalY;
            float localBottom = _borderBottomLocalY + HomeBorderedSectionScope.BottomAdvance * ImGuiHelpers.GlobalScale;
            ImGui.SetCursorPos(new Vector2(_sectionStartLocalX, localBottom));
            ImGui.PopID();
        }
    }
}
