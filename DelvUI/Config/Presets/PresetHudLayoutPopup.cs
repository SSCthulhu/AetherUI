using DelvUI.Config;
using DelvUI.Helpers;
using Dalamud.Bindings.ImGui;
using System.Numerics;

namespace DelvUI.Config.Presets
{
    public static class PresetHudLayoutPopup
    {
        private const string PopupId = "PresetHudLayoutPopup ##Delvui";

        private static AetherPreset _activePreset;
        private static bool _tempAttachHudEnabled;
        private static int _tempHudLayout;

        public static void Open(AetherPreset preset)
        {
            PresetHudLayoutSettingsConfig? settings =
                ConfigurationManager.Instance.GetConfigObject<PresetHudLayoutSettingsConfig>();
            if (settings == null)
            {
                return;
            }

            PresetHudLayoutBinding binding = settings.GetBinding(preset);
            _activePreset = preset;
            _tempAttachHudEnabled = binding.AttachHudEnabled;
            _tempHudLayout = binding.HudLayout;

            ImGui.OpenPopup(PopupId);
        }

        public static void Draw(ref bool changed)
        {
            bool isOpen = true;
            if (!ImGui.BeginPopupModal(PopupId, ref isOpen, ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.AlwaysAutoResize))
            {
                return;
            }

            ImGui.Text($"Configure HUD Layout for {GetPresetLabel(_activePreset)}");
            ImGui.Spacing();

            ImGui.Checkbox("Attach HUD Layout to this preset", ref _tempAttachHudEnabled);

            if (!_tempAttachHudEnabled)
            {
                _tempHudLayout = 0;
            }
            else
            {
                ImGui.Text("\u2514");

                for (int i = 1; i <= 4; i++)
                {
                    ImGui.SameLine();
                    bool layoutEnabled = _tempHudLayout == i;
                    if (ImGui.Checkbox($"Hud Layout {i}##presetHudLayoutPopup_{i}", ref layoutEnabled) && layoutEnabled)
                    {
                        _tempHudLayout = i;
                    }
                }
            }

            ImGui.Spacing();
            ImGuiHelper.DrawSeparator(1, 1);
            ImGui.Spacing();

            if (ImGui.Button("Save", new Vector2(120, 0)))
            {
                PresetHudLayoutSettingsConfig? settings =
                    ConfigurationManager.Instance.GetConfigObject<PresetHudLayoutSettingsConfig>();
                if (settings != null)
                {
                    PresetHudLayoutBinding binding = settings.GetBinding(_activePreset);
                    if (binding.AttachHudEnabled != _tempAttachHudEnabled || binding.HudLayout != _tempHudLayout)
                    {
                        binding.AttachHudEnabled = _tempAttachHudEnabled;
                        binding.HudLayout = _tempHudLayout;
                        changed = true;
                        ConfigurationManager.Instance.ForceNeedsSave();
                    }
                }

                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel", new Vector2(120, 0)))
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        private static string GetPresetLabel(AetherPreset preset)
        {
            return preset switch
            {
                AetherPreset.Minimal => "Minimal",
                AetherPreset.MmoModern => "MMO Modern",
                AetherPreset.RaidFocused => "Raid Focused",
                AetherPreset.ActionCombat => "Action Combat",
                _ => preset.ToString()
            };
        }
    }
}
