using Dalamud.Bindings.ImGui;
using System.Numerics;

namespace DelvUI.Config.Home.Widgets
{
    public static class HomeSectionHeader
    {
        public static void Draw(string label, float leftOffset = 0f)
        {
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            float startX = ImGui.GetCursorPosX() + leftOffset;

            ImGui.SetCursorPosX(startX);
            Vector2 textScreenPos = ImGui.GetCursorScreenPos();

            ImGui.PushStyleColor(ImGuiCol.Text, HomeUiStyle.Gold);
            string upperLabel = label.ToUpperInvariant();
            ImGui.Text(upperLabel);
            ImGui.PopStyleColor();

            Vector2 textSize = ImGui.CalcTextSize(upperLabel);
            float lineY = textScreenPos.Y + textSize.Y * 0.5f;
            float lineStartX = textScreenPos.X + textSize.X + 10f;
            float lineEndX = ImGui.GetWindowPos().X + ImGui.GetWindowSize().X - ImGui.GetStyle().WindowPadding.X;

            uint lineColor = ImGui.ColorConvertFloat4ToU32(HomeUiStyle.PanelBorder);
            if (lineEndX > lineStartX)
            {
                drawList.AddLine(new Vector2(lineStartX, lineY), new Vector2(lineEndX, lineY), lineColor, 1f);
            }

            ImGui.Spacing();
        }
    }
}
