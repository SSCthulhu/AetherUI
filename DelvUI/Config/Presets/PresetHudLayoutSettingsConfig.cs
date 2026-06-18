using DelvUI.Config;
using DelvUI.Config.Attributes;

namespace DelvUI.Config.Presets
{
    [Disableable(false)]
    [Exportable(false)]
    [Shareable(false)]
    [Resettable(false)]
    [Section("Home")]
    [SubSection("Preset HUD Layouts", 0)]
    public class PresetHudLayoutSettingsConfig : PluginConfigObject
    {
        public new static PresetHudLayoutSettingsConfig DefaultConfig() => new PresetHudLayoutSettingsConfig();

        public PresetHudLayoutBinding Minimal = new PresetHudLayoutBinding();
        public PresetHudLayoutBinding MmoModern = new PresetHudLayoutBinding();
        public PresetHudLayoutBinding RaidFocused = new PresetHudLayoutBinding();
        public PresetHudLayoutBinding ActionCombat = new PresetHudLayoutBinding();

        public PresetHudLayoutBinding GetBinding(AetherPreset preset)
        {
            return preset switch
            {
                AetherPreset.Minimal => Minimal,
                AetherPreset.MmoModern => MmoModern,
                AetherPreset.RaidFocused => RaidFocused,
                AetherPreset.ActionCombat => ActionCombat,
                _ => Minimal
            };
        }

        public void CopyBindingsFrom(PresetHudLayoutSettingsConfig other)
        {
            CopyBinding(Minimal, other.Minimal);
            CopyBinding(MmoModern, other.MmoModern);
            CopyBinding(RaidFocused, other.RaidFocused);
            CopyBinding(ActionCombat, other.ActionCombat);
        }

        private static void CopyBinding(PresetHudLayoutBinding target, PresetHudLayoutBinding source)
        {
            target.AttachHudEnabled = source.AttachHudEnabled;
            target.HudLayout = source.HudLayout;
        }
    }

    public class PresetHudLayoutBinding
    {
        public bool AttachHudEnabled = false;
        public int HudLayout = 0;
    }
}
