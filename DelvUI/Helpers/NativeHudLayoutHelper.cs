using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace DelvUI.Helpers
{
    public static unsafe class NativeHudLayoutHelper
    {
        public static void ApplyAttachedHudLayout(bool attachHudEnabled, int hudLayout)
        {
            if (!attachHudEnabled || hudLayout < 1 || hudLayout > 4)
            {
                return;
            }

            AddonConfig.Instance()->ChangeHudLayout((uint)hudLayout - 1);
        }
    }
}
