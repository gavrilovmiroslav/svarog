using svarog;

namespace svarog.Plugins
{
    [Plugin]
    public class RoguesLoaderPlugin : Plugin
    {
        public RoguesLoaderPlugin() {}

        public override void Load(Svarog svarog)
        {
            var loader = new RoguesLoader(svarog.resources);
            loader.Load("tiles");
            loader.Load("rogues");
            loader.Load("monsters");
            loader.Load("items");
            loader.Load("animals");
        }
    }
}
