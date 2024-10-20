using SFML.Graphics;
using SFML.System;
using svarog.Algorithms;

namespace svarog.Plugins
{
    // uncomment this to make the plugin register:
    [Plugin]
    public class VoronoiProcgenPlugin : Plugin
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

        Color[] colors = new Color[]
        {
            Color.Black,
            new Color(0xFF0000FF), // Red
            new Color(0x00FF00FF), // Green
            new Color(0x0000FFFF), // Blue
            new Color(0xFFFF00FF), // Yellow
            new Color(0xFF00FFFF), // Magenta
            new Color(0x00FFFFFF), // Cyan
            new Color(0x800000FF), // Maroon
            new Color(0x808000FF), // Olive
            new Color(0x008000FF), // Dark Green
            new Color(0x800080FF), // Purple
            new Color(0x008080FF), // Teal
            new Color(0x000080FF), // Navy
            new Color(0xFFA500FF), // Orange
            new Color(0xA52A2AFF), // Brown
            new Color(0x8B4513FF), // Saddle Brown
            new Color(0xDEB887FF), // Burlywood
            new Color(0xD2691EFF), // Chocolate
            new Color(0xF4A460FF), // Sandy Brown
            new Color(0xFA8072FF), // Salmon
            new Color(0xE9967AFF), // Dark Salmon
            new Color(0xFF4500FF), // Orange Red
            new Color(0xFF6347FF), // Tomato
            new Color(0xFF7F50FF), // Coral
            new Color(0xFFD700FF), // Gold
            new Color(0xEEE8AAFF), // Pale Goldenrod
            new Color(0xF0E68CFF), // Khaki
            new Color(0xBDB76BFF), // Dark Khaki
            new Color(0x7FFF00FF), // Chartreuse
            new Color(0x7CFC00FF), // Lawn Green
            new Color(0xADFF2FFF), // Green Yellow
            new Color(0x32CD32FF), // Lime Green
            new Color(0x9ACD32FF), // Yellow Green
            new Color(0x6B8E23FF), // Olive Drab
            new Color(0x556B2FFF), // Dark Olive Green
            new Color(0x66CDAAFF), // Medium Aquamarine
            new Color(0x4682B4FF), // Steel Blue
            new Color(0x5F9EA0FF), // Cadet Blue
            new Color(0xB0C4DEFF), // Light Steel Blue
            new Color(0xADD8E6FF), // Light Blue
            new Color(0x87CEEBFF), // Sky Blue
            new Color(0x87CEFAFF), // Light Sky Blue
            new Color(0x00BFFFFF), // Deep Sky Blue
            new Color(0x1E90FFFF), // Dodger Blue
            new Color(0x6495EDFF), // Cornflower Blue
            new Color(0x7B68EEFF), // Medium Slate Blue
            new Color(0x6A5ACDFF), // Slate Blue
            new Color(0x483D8BFF), // Dark Slate Blue
            new Color(0x9370DBFF), // Medium Purple
            new Color(0x8A2BE2FF), // Blue Violet
            new Color(0x9400D3FF), // Dark Violet
            new Color(0x9932CCFF), // Dark Orchid
            new Color(0xBA55D3FF), // Medium Orchid
            new Color(0xDA70D6FF), // Orchid
            new Color(0xEE82EEFF), // Violet
            new Color(0xDDA0DDFF), // Plum
            new Color(0xD8BFD8FF), // Thistle
            new Color(0xE6E6FAFF), // Lavender
            new Color(0xFFF0F5FF), // Lavender Blush
            new Color(0xFFE4E1FF), // Misty Rose
            new Color(0xFFDAB9FF), // Peach Puff
            new Color(0xFFF5EEFF),  // Seashell
            new Color(0xCD5C5CFF), // Indian Red
            new Color(0xF08080FF), // Light Coral
            new Color(0xFF69B4FF), // Hot Pink
            new Color(0xFF1493FF), // Deep Pink
            new Color(0xC71585FF), // Medium Violet Red
            new Color(0xDB7093FF), // Pale Violet Red
            new Color(0xB22222FF), // Firebrick
            new Color(0xFFFAF0FF), // Floral White
            new Color(0xFDF5E6FF), // Old Lace
            new Color(0xF5FFFAFF)  // Mint Cream
        };

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

                var cells = Subdivision.Polygonize(points);
                var voronoi = instance.resources.Bag("voronoi", new IntMap(40, 25));
                int counter = 1;
                Console.WriteLine($"Voronoi cells: {cells.Count}");                
                
                foreach (var cell in cells)
                {
                    var b = cell.Bounds();
                    for (int x = (int)b.Left - 1; x <= (int)(b.Left + b.Width); x++)
                    {
                        for (int y = (int)b.Top - 1; y <= (int)(b.Top + b.Height); y++)
                        {
                            if (cell.IsPointInPolygon(new Vector2f(x, y)))
                            {
                                voronoi.Values[x, y] = counter;
                            }
                        }
                    }

                    counter++;
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
            var map = svarog.resources.GetFromBag<IntMap>("voronoi");
            
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
                    var c = map?.Values[i, j] ?? 0;
                    sprite.Color = colors[c];
                    svarog.render?.Draw(sprite, new RenderStates(BlendMode.Add));

                    byte v = (byte)((equi?.Values[i, j] ?? false) ? 255 : 0);
                    sprite.Color = new Color(v, v, v, 128);
                    svarog.render?.Draw(sprite, new RenderStates(BlendMode.Add));
                }
            }
        }
    }
}
