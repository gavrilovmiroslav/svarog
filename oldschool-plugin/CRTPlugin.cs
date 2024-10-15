using svarog_core;
using svarog.Effects;

namespace svarog.Plugins
{
    [Plugin(Priority = 1004)]
    public class CRTPlugin : PostprocessPlugin
    {
        public CRTPlugin() : base("CRT") {}
    }
}
