using Dalamud.Bindings.ImGui;
using System.Numerics;

namespace DelvUI.Config.Home.Widgets
{
    public static class HomeUiLayout
    {
        public static Vector2 ContentLocalToScreen(Vector2 local)
        {
            Vector2 windowPos = ImGui.GetWindowPos();
            Vector2 contentMin = ImGui.GetWindowContentRegionMin();
            Vector2 scroll = new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY());
            return windowPos + contentMin + local - scroll;
        }

        public static Vector2 ScreenToContentLocal(Vector2 screen)
        {
            Vector2 windowPos = ImGui.GetWindowPos();
            Vector2 contentMin = ImGui.GetWindowContentRegionMin();
            Vector2 scroll = new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY());
            return screen - windowPos - contentMin + scroll;
        }

        public static float ScreenToContentLocalY(float screenY) => ScreenToContentLocal(new Vector2(0f, screenY)).Y;

        public static Vector2 GetScrollViewportScreenMin()
        {
            Vector2 windowPos = ImGui.GetWindowPos();
            Vector2 contentMin = ImGui.GetWindowContentRegionMin();
            return windowPos + contentMin;
        }

        public static Vector2 GetScrollViewportScreenMax()
        {
            Vector2 windowPos = ImGui.GetWindowPos();
            Vector2 contentMax = ImGui.GetWindowContentRegionMax();
            return windowPos + contentMax;
        }

        /// <summary>Maps content-local coordinates to screen space fixed in the current window viewport.</summary>
        public static Vector2 ContentLocalToFixedScreen(Vector2 local)
        {
            Vector2 windowPos = ImGui.GetWindowPos();
            Vector2 contentMin = ImGui.GetWindowContentRegionMin();
            return windowPos + contentMin + local;
        }

        public static float ContentLocalToFixedScreenX(float localX) => ContentLocalToFixedScreen(new Vector2(localX, 0f)).X;

        /// <summary>Maps content-local Y to a screen Y that stays fixed when the child scrolls.</summary>
        public static float ContentLocalToFixedScreenY(float localY) => ContentLocalToFixedScreen(new Vector2(0f, localY)).Y;
    }
}
