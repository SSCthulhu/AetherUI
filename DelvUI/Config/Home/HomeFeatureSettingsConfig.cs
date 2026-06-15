using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Config.Presets;

namespace DelvUI.Config.Home
{
    [Disableable(false)]
    [Exportable(false)]
    [Shareable(false)]
    [Resettable(false)]
    [Section("Home")]
    [SubSection("Feature Settings", 0)]
    public class HomeFeatureSettingsConfig : PluginConfigObject
    {
        public new static HomeFeatureSettingsConfig DefaultConfig() => new HomeFeatureSettingsConfig();

        public bool PlayerParameterOrb = true;

        public bool IndividualFramesMaster = true;
        public bool UnitFrames = true;
        public bool ManaBars = true;
        public bool CastBars = true;

        public bool BuffsAndDebuffs = true;
        public bool OtherElements = true;
        public bool ExperienceBar = true;
        public bool GcdIndicator = true;
        public bool PullTimer = true;
        public bool LimitBreak = true;
        public bool MpTicker = true;
        public bool Nameplates = true;
        public bool PartyFrames = true;
        public bool PartyCooldowns = true;
        public bool EnemyList = true;
        public bool JobSpecificBars = true;
        public bool Minimap = true;
        public bool ActionCamera = true;

        public ActivePresetSelection ActivePreset = ActivePresetSelection.Custom;
    }
}
