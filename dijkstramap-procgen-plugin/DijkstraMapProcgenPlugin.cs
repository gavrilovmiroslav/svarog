using svarog;
using svarog.Algorithms;

namespace svarog.Plugins
{
    // uncomment this to make the plugin register:
    [Plugin]
    public class DijkstraMapProcgenPlugin : Plugin
    {
        public enum EProcgen
        {
            Generation,
            Playback,
            Rerouting,
        }

        public enum ETrigger
        {
            Done,
            Restart,
            Reroute,
        }

        public override void Load(Svarog instance)
        {
            var sm = instance.resources.CreateStateMachine<EProcgen, ETrigger>("dijkstramap-procgen", EProcgen.Generation);

            var generate = () =>
            {
                Console.WriteLine("GENERATING...");
                var map = instance.resources.Bag("map", BoolMap.Random(40, 25, 70));
                instance.resources.RemoveFromBag("flood");
                sm.Fire(ETrigger.Done);
            };

            sm.Configure(EProcgen.Generation)
                .OnEntry(generate)
                .OnActivate(generate)
                .Permit(ETrigger.Done, EProcgen.Playback)
                .Ignore(ETrigger.Restart)
                .Ignore(ETrigger.Reroute);

            sm.Configure(EProcgen.Playback)
                .OnEntry(() =>
                {
                    Console.WriteLine("DONE!");
                })
                .Permit(ETrigger.Restart, EProcgen.Generation)
                .Permit(ETrigger.Reroute, EProcgen.Rerouting)
                .Ignore(ETrigger.Done);

            sm.Configure(EProcgen.Rerouting)
                .OnEntry(e =>
                {
                    var x = (int)e.Parameters[0] / 32;
                    var y = (int)e.Parameters[1] / 32;
                    var map = instance.resources.GetFromBag<BoolMap>("map");
                    if (map != null)
                    {
                        instance.resources?.Bag<IntMap>("flood", map.Flood(x, y));
                    }
                    sm.Fire(ETrigger.Done);
                })
                .Permit(ETrigger.Done, EProcgen.Playback)
                .Ignore(ETrigger.Restart)
                .Ignore(ETrigger.Reroute);

            sm.Activate();
        }

        public override void Frame(Svarog instance)
        {
            if (instance.mouse.IsJustPressed(SFML.Window.Mouse.Button.Right))
            {
                var sm = instance.resources.GetStateMachine<EProcgen, ETrigger>("dijkstramap-procgen");
                sm?.Fire(ETrigger.Restart);
            }
            else if (instance.mouse.IsJustPressed(SFML.Window.Mouse.Button.Left))
            {
                var sm = instance.resources.GetStateMachine<EProcgen, ETrigger>("dijkstramap-procgen");
                sm?.Fire<int, int>(new Stateless.StateMachine<EProcgen, ETrigger>.TriggerWithParameters<int, int>(ETrigger.Reroute), instance.mouse.Position.Item1, instance.mouse.Position.Item2);
            }
        }
    }
}
