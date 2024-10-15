using svarog_core;
using svarog.Effects;

namespace svarog.Plugins
{
    [Plugin(Priority = 1003)]
    public class CRTPlugin : PostprocessPlugin
    {
        public CRTPlugin() : base("CRT") {}
    }
}
