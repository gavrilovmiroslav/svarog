using svarog.Algorithms;

namespace svarog.Plugins
{
    [Plugin(Priority = 1)]
    public class ShadowcastExports : Plugin
    {
        public override void Register(Svarog instance)
        {
            Export("shadowcast")
                .With<BoolMap>("map")
                .With<(int, int)>("position")
                .With<int>("range")
                .Returning((svarog, args) => {
                    return Shadowcast.GenerateShadowCast(
                        (BoolMap)args["map"],
                        ((int, int))args["position"],
                        (int)args["range"]);
                });
        }   
    }
}
