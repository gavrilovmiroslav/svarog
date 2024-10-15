using svarog_core;
using svarog.Effects;

namespace svarog.Plugins
{
    [Plugin(Priority = 1002)]
    public class CRTPlugin : PostprocessPlugin
    {
        public CRTPlugin() : base("CRT") {}
    }
}
