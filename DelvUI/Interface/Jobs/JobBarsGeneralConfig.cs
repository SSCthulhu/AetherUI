using DelvUI.Config;
using DelvUI.Config.Attributes;

namespace DelvUI.Interface.Jobs
{
    [Exportable(false)]
    [Section("Job Specific Bars")]
    [SubSection("General", 0)]
    public class JobBarsGeneralConfig : PluginConfigObject
    {
        public new static JobBarsGeneralConfig DefaultConfig() => new JobBarsGeneralConfig();
    }
}
