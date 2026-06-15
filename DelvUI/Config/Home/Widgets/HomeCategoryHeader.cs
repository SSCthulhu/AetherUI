using Dalamud.Bindings.ImGui;
using System.Numerics;

namespace DelvUI.Config.Home.Widgets
{
    public static class HomeCategoryHeader
    {
        public static void Draw(string label)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, HomeUiStyle.Gold);
            ImGui.Text(label.ToUpperInvariant());
            ImGui.PopStyleColor();
            ImGui.Dummy(new Vector2(0f, 2f));
        }
    }
}
