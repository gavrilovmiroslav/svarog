using svarog.Effects;
using svarog_core;

namespace svarog.Plugins
{
    [Plugin(Priority = 1002)]
    public class ScanlinesPlugin : PostprocessPlugin
    {
        public ScanlinesPlugin() : base("Scanlines") {}
    }
}
