using SFML.Graphics;
using svarog;
using svarog.Algorithms;

namespace svarog.Plugins
{
    // uncomment this to make the plugin register:
    // [Plugin]
    public class NoiseProcgenPlugin : Plugin
    {
        public Sprite sprite = new();

        public enum EProcgen
        {
            Generation,
            Playback,
        }

        public enum ETrigger
        {
            Done,
            Restart,
        }

        public override void Load(Svarog instance)
        {
            var sm = instance.resources.CreateStateMachine<EProcgen, ETrigger>("noise-procgen", EProcgen.Generation);

            var generate = () =>
            {
                Console.WriteLine("GENERATING...");
                var hdoorPattern = new IntPattern3x3(
                    """
                    FFF
                    TTT
                    FFF
                    """);

                var vdoorPattern = new IntPattern3x3(
                    """
                    FTF
                    FTF
                    FTF
                    """);

                var bigRoomPattern = new IntPattern3x3(
                    """
                    FFF
                    FFF
                    FFF
                    """);

                var map = instance.resources.Bag("map", IntMap.Noise(40, 25, 0.5f).FilterBelow(150));
                var walls = map.ToBoolMap(v => v > 0);
                var hdoors = instance.resources.Bag("hdoors", walls.Find(hdoorPattern));
                var vdoors = instance.resources.Bag("vdoors", walls.Find(vdoorPattern));
                var bigRoom = instance.resources.Bag("bigroom", walls.Find(bigRoomPattern));
                sm.Fire(ETrigger.Done);
            };

            sm.Configure(EProcgen.Generation)
                .OnEntry(generate)
                .OnActivate(generate)
                .Permit(ETrigger.Done, EProcgen.Playback)
                .Ignore(ETrigger.Restart);

            sm.Configure(EProcgen.Playback)
                .OnEntry(() =>
                {
                    Console.WriteLine("DONE!");
                })
                .Permit(ETrigger.Restart, EProcgen.Generation)
                .Ignore(ETrigger.Done);

            sm.Activate();
        }

        public override void Frame(Svarog instance)
        {
            if (instance.mouse.IsJustPressed(SFML.Window.Mouse.Button.Right))
            {
                var sm = instance.resources.GetStateMachine<EProcgen, ETrigger>("noise-procgen");
                sm?.Fire(ETrigger.Restart);
            }
        }


        public override void Render(Svarog svarog)
        {
            var map = svarog.resources.GetFromBag<IntMap>("map");
            
            var hdoors = svarog.resources.GetFromBag<BoolMap>("hdoors");
            var vdoors = svarog.resources.GetFromBag<BoolMap>("vdoors");
            var bigRoom = svarog.resources.GetFromBag<BoolMap>("bigroom");

            var tiles = svarog.resources.NamedSprites["tiles"];
            int c = 0;

            sprite.Color = Color.White;
            var s = svarog.resources.GetSprite("White");
            if (s != null)
            {
                sprite.Texture = s.Texture;
                sprite.TextureRect = s.Coords;
            }

            for (int i = 0; i < 40; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    var p = sprite.Position;
                    p.X = i * 32;
                    p.Y = j * 32;
                    sprite.Position = p;
                    var v = (byte)(map?.Values[i, j] ?? 0);
                    sprite.Color = new Color(v, v, v, 255);
                    svarog.render?.Draw(sprite, new RenderStates(BlendMode.Add));

                    if (hdoors?.Values[i, j] ?? false)
                    {
                        sprite.Color = Color.Green;
                        svarog.render?.Draw(sprite, new RenderStates(BlendMode.Add));
                    }

                    if (vdoors?.Values[i, j] ?? false)
                    {
                        sprite.Color = Color.Red;
                        svarog.render?.Draw(sprite, new RenderStates(BlendMode.Add));
                    }

                    if (bigRoom?.Values[i, j] ?? false)
                    {
                        sprite.Color = Color.Blue;
                        svarog.render?.Draw(sprite, new RenderStates(BlendMode.Add));
                    }
                    c++;
                }
            }
        }
    }
}
