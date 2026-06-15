using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using System;
using System.Numerics;

namespace DelvUI.Config.Home.Widgets
{
    public struct HomeBorderedSectionScope : IDisposable
    {
        public const float SidePadding = 18f;

        private const float BottomPadding = 12f;
        private const float InnerTopPadding = 8f;
        private const float TitleGap = 6f;
        public const float BottomAdvance = 4f;

        private const float CornerRadius = 8f;
        private const float BorderThickness = 1f;

        private readonly string _title;
        private readonly float _titleScreenX;
        private readonly float _borderLeftScreenX;
        private readonly float _outerWidth;
        private readonly float _sidePad;
        private readonly float _bottomPad;
        private readonly float _borderTopScreenY;
        private float _borderBottomLocalY;

        public float InnerWidth { get; }
        public float BorderBottomLocalY => _borderBottomLocalY;

        public static HomeBorderedSectionScope Begin(string title, float outerWidth, float titleLeftOffset = 0f)
        {
            float scale = ImGuiHelpers.GlobalScale;
            float sidePad = SidePadding * scale;
            float bottomPad = BottomPadding * scale;
            float innerTopPad = InnerTopPadding * scale;
            float textLine = ImGui.GetTextLineHeight();
            float topReserve = textLine * 0.5f + innerTopPad;
            float innerWidth = Math.Max(0f, outerWidth - sidePad * 2f);

            Vector2 sectionStart = ImGui.GetCursorPos();
            Vector2 sectionStartScreen = ImGui.GetCursorScreenPos();
            float borderTopScreenY = sectionStartScreen.Y + textLine * 0.5f;
            float titleScreenX = titleLeftOffset > 0f
                ? sectionStartScreen.X + titleLeftOffset
                : sectionStartScreen.X + sidePad;

            ImGui.PushID($"bordered_{title}");
            ImGui.BeginGroup();
            ImGui.Dummy(new Vector2(outerWidth, topReserve));
            ImGui.SetCursorPos(new Vector2(sectionStart.X + sidePad, ImGui.GetCursorPosY()));

            return new HomeBorderedSectionScope(
                title,
                titleScreenX,
                sectionStartScreen.X,
                outerWidth,
                innerWidth,
                sidePad,
                bottomPad,
                borderTopScreenY);
        }

        private HomeBorderedSectionScope(
            string title,
            float titleScreenX,
            float borderLeftScreenX,
            float outerWidth,
            float innerWidth,
            float sidePad,
            float bottomPad,
            float borderTopScreenY)
        {
            _title = title;
            _titleScreenX = titleScreenX;
            _borderLeftScreenX = borderLeftScreenX;
            _outerWidth = outerWidth;
            InnerWidth = innerWidth;
            _sidePad = sidePad;
            _bottomPad = bottomPad;
            _borderTopScreenY = borderTopScreenY;
        }

        public void Dispose()
        {
            ImGui.Dummy(new Vector2(InnerWidth, _bottomPad));
            ImGui.Dummy(new Vector2(_outerWidth, 0f));
            ImGui.EndGroup();

            Vector2 contentMax = ImGui.GetItemRectMax();
            float borderInset = BorderThickness * 0.5f;
            Vector2 outerMin = new Vector2(_borderLeftScreenX + borderInset, _borderTopScreenY + borderInset);
            Vector2 outerMax = new Vector2(_borderLeftScreenX + _outerWidth - borderInset, contentMax.Y - borderInset);

            DrawBorder(outerMin, outerMax, _title, _titleScreenX);

            _borderBottomLocalY = outerMax.Y - ImGui.GetWindowPos().Y;
            float localBottom = _borderBottomLocalY + BottomAdvance * ImGuiHelpers.GlobalScale;
            float localLeft = _borderLeftScreenX - ImGui.GetWindowPos().X;
            ImGui.SetCursorPos(new Vector2(localLeft, localBottom));
            ImGui.PopID();
        }

        private static void DrawBorder(Vector2 outerMin, Vector2 outerMax, string title, float titleScreenX)
        {
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            uint borderColor = ImGui.ColorConvertFloat4ToU32(HomeUiStyle.PanelBorder);
            uint backdropColor = ImGui.GetColorU32(ImGuiCol.WindowBg);
            uint titleColor = ImGui.ColorConvertFloat4ToU32(HomeUiStyle.Gold);

            string upperTitle = title.ToUpperInvariant();
            Vector2 textSize = ImGui.CalcTextSize(upperTitle);
            float scale = ImGuiHelpers.GlobalScale;
            float titleGap = TitleGap * scale;
            float titleLeft = titleScreenX - titleGap;
            float titleRight = titleScreenX + textSize.X + titleGap;
            float textY = outerMin.Y - textSize.Y * 0.5f;

            drawList.AddRect(
                outerMin,
                outerMax,
                borderColor,
                CornerRadius,
                ImDrawFlags.RoundCornersAll,
                BorderThickness);

            drawList.AddRectFilled(
                new Vector2(titleLeft, outerMin.Y - BorderThickness),
                new Vector2(titleRight, outerMin.Y + BorderThickness + 1f),
                backdropColor);

            drawList.AddText(new Vector2(titleScreenX, textY), titleColor, upperTitle);

            float cornerInset = CornerRadius;
            if (titleLeft > outerMin.X + cornerInset)
            {
                drawList.AddLine(
                    new Vector2(outerMin.X + cornerInset, outerMin.Y),
                    new Vector2(titleLeft, outerMin.Y),
                    borderColor,
                    BorderThickness);
            }

            if (titleRight < outerMax.X - cornerInset)
            {
                drawList.AddLine(
                    new Vector2(titleRight, outerMin.Y),
                    new Vector2(outerMax.X - cornerInset, outerMin.Y),
                    borderColor,
                    BorderThickness);
            }
        }
    }
}
