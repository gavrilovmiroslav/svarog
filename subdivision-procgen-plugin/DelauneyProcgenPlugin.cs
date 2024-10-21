using SFML.Graphics;
using SFML.System;
using Stateless;
using svarog.Algorithms;

namespace svarog.Plugins
{
    // uncomment this to make the plugin register:
    //[Plugin]
    public class DelauneyProcgenPlugin : GenerativePlugin
    {
        public DelauneyProcgenPlugin() : base("delauney") { }

        public override void Generate(Svarog instance, StateMachine<EProcgen, ETrigger> sm)
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
