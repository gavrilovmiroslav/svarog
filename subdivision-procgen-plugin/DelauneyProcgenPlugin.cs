using SFML.Graphics;
using SFML.System;
using svarog.Algorithms;

namespace svarog.Plugins
{
    // uncomment this to make the plugin register:
    //[Plugin]
    public class DelauneyProcgenPlugin : Plugin
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
            var sm = instance.resources.CreateStateMachine<EProcgen, ETrigger>("graph-procgen", EProcgen.Generation);

            var generate = () =>
            {
                Console.WriteLine("GENERATING...");

                var equ = instance.resources.Bag("equimap", BoolMap.EquidistantSampling(40, 25, ESamplingDistance.High));
                var equi = equ.ToIntMap(BoolMap.TruthinessToInt);
                var points = new List<Vector2f>();
                for (int i = 0; i < equi.Width; i++)
                {
                    for (int j = 0; j < equi.Height; j++)
                    {
                        if (equi.Values[i, j] > 0)
                        {
                            points.Add(new Vector2f(i, j));
                        }
                    }
                }
                points.Add(new Vector2f(0, 0));
                points.Add(new Vector2f(equi.Width, 0));
                points.Add(new Vector2f(0, equi.Height));
                points.Add(new Vector2f(equi.Width, equi.Height));

                var tris = Subdivision.Triangulate(points);
                var cents = instance.resources.Bag("centroids", new BoolMap(40, 25));
                foreach (var tri in tris)
                {
                    var c = tri.Centroid();
                    cents.Values[(int)c.X, (int)c.Y] = true;
                }

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
                var sm = instance.resources.GetStateMachine<EProcgen, ETrigger>("graph-procgen");
                sm?.Fire(ETrigger.Restart);
            }
        }

        public override void Render(Svarog svarog)
        {
            var equi = svarog.resources.GetFromBag<BoolMap>("equimap");
            var map = svarog.resources.GetFromBag<BoolMap>("centroids");
            
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
                    var v = (byte)((map?.Values[i, j] ?? false) ? 255 : 0);
                    sprite.Color = new Color(v, 0, 0, 128);
                    svarog.render?.Draw(sprite, new RenderStates(BlendMode.Add));

                    v = (byte)((equi?.Values[i, j] ?? false) ? 255 : 0);
                    sprite.Color = new Color(0, v, 0, 128);
                    svarog.render?.Draw(sprite, new RenderStates(BlendMode.Add));
                }
            }
        }
    }
}
