using svarog.Effects;
using svarog;

namespace svarog.Plugins
{
    [Plugin(Priority = 505)]
    public class ScanlinesPlugin : PostprocessPlugin
    {
        public ScanlinesPlugin() : base("Scanlines") {}
    }
}
