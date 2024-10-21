using SFML.Graphics;
using svarog;
using svarog.Algorithms;

namespace svarog.Plugins
{
    // uncomment this to make the plugin register:
    //[Plugin]
    public class DijkstraMapProcgenPlugin : Plugin
    {
        Sprite sprite = new();

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
                var map = instance.resources.Bag("map", FloatMap.Noise(40, 25, 0.5f).ToBoolMap(f => f < 120));
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
                        var flooded = map.Flood(x, y)?.ToFloatMap();
                        if (flooded != null)
                        {
                            instance.resources?.Bag<FloatMap>("flood", flooded);
                        }
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

        public override void Render(Svarog svarog)
        {
            var map = svarog.resources.GetFromBag<BoolMap>("map");
            var tiles = svarog.resources.NamedSprites["tiles"];
            int c = 0;

            sprite.Color = Color.White;

            for (int i = 0; i < 40; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    var p = sprite.Position;
                    p.X = i * 32;
                    p.Y = j * 32;
                    sprite.Position = p;
                    var s = svarog.resources.GetSprite(map?.Values[i, j] ?? false ? "Blank_floor" : "Inner_wall");
                    if (s != null)
                    {
                        sprite.Texture = s.Texture;
                        sprite.TextureRect = s.Coords;
                        svarog.render?.Draw(sprite);
                    }
                    c++;
                }
            }

            var flood = svarog.resources.GetFromBag<FloatMap>("flood");
            if (flood != null)
            {
                for (int i = 0; i < 40; i++)
                {
                    for (int j = 0; j < 25; j++)
                    {
                        var v = flood.Values[i, j];
                        if (v > 255)
                        {
                            sprite.Color = Color.White;
                            continue;
                        }

                        var p = sprite.Position;
                        p.X = i * 32;
                        p.Y = j * 32;
                        sprite.Position = p;
                        var s = svarog.resources.GetSprite("White");
                        if (s != null)
                        {
                            var a = 255 - v * 5;
                            if (a < 0) a = 0;
                            sprite.Color = v == 0 ? new Color(255, 0, 0, 128) : new Color((byte)a, 0, 0, (byte)(a / 2));
                            sprite.Texture = s.Texture;
                            sprite.TextureRect = s.Coords;
                            svarog.render?.Draw(sprite);
                        }
                        c++;
                    }
                }
            }
        }
    }
}
