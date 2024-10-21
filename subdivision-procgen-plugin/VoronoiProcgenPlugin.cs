using DelaunatorSharp;
using SFML.Graphics;
using SFML.System;
using SharpGraph;
using Stateless;
using svarog.Algorithms;
using svarog.Structures;

namespace svarog.Plugins
{
    [Plugin]
    public class VoronoiProcgenPlugin : GenerativePlugin
    {
        public VoronoiProcgenPlugin() : base("voronoi") { }

        public override void Generate(Svarog instance, StateMachine<EProcgen, ETrigger> sm)
        {
            Console.WriteLine("GENERATING...");

            var rand = new Random();

            var equ = instance.resources.Bag("equimap", BoolMap.EquidistantSampling(40, 25, ESamplingDistance.Low, 4.0f));
            var equi = equ.ToIntMap(BoolMap.TruthinessToInt);
            var (cells, graph, voronoi) = Subdivision.Polygonize(equi);
            instance.resources.Bag<Graph>("graph", graph);
            instance.resources.Bag<IntMap>("voronoi", voronoi);

            var height = instance.resources.Bag("height", FloatMap.Noise(160, 100));
            var noise = instance.resources.Bag("noise", FloatMap.Noise(160, 100, 0.9f));
            for (int i = 0; i < 160; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    if (voronoi.Values[i, j] == 0)
                        height.Values[i, j] = 0;
                    else
                        height.Values[i, j] = MathF.Sqrt(height.Values[i, j]);
                }
            }

            var tree = graph.GenerateMinimumSpanningTree(SpanningTreeAlgorithm.Kruskal, false);
            instance.resources.Bag<List<SharpGraph.Edge>>("tree", tree);
            var doors = instance.resources.Bag<List<(SharpGraph.Edge, (int, int))>>("doors", new List<(SharpGraph.Edge, (int, int))>());

            int c = 128;
            foreach (var edge in tree)
            {
                var (x1, y1) = ParseLabel(edge.From().GetLabel());
                var (x2, y2) = ParseLabel(edge.To().GetLabel());

                var c1 = voronoi.Values[x1, y1];
                var c2 = voronoi.Values[x2, y2];

                var cell1 = cells.Where(p => p.Index == c1).First();
                var cell2 = cells.Where(p => p.Index == c2).First();

                var r1 = cell1.Bounds();
                for (int rx = 0; rx < (int)r1.Width; rx++)
                {
                    for (int ry = 0; ry < (int)r1.Height; ry++)
                    {
                        var x = (int)r1.Left + rx;
                        var y = (int)r1.Top + ry;

                        if (height.Values[x, y] == 0) continue;
                        if ((rx == 0 || rx == (int)r1.Width - 1) || (ry == 0 || ry == (int)r1.Height - 1))
                        {
                            continue;
                        }
                        else
                        {
                            var deg1 = graph.Degree(edge.From());
                            var deg2 = graph.Degree(edge.To());
                            var degFactor = (int)((float)(deg1 + deg2) / 2);
                            var deg = Math.Clamp(126 / degFactor, 0, 126);

                            height.Values[x, y] = c + deg;
                        }
                    }
                }

                var r2 = cell2.Bounds();
                for (int rx = 0; rx < (int)r2.Width; rx++)
                {
                    for (int ry = 0; ry < (int)r2.Height; ry++)
                    {
                        var x = (int)r2.Left + rx;
                        var y = (int)r2.Top + ry;

                        if (height.Values[x, y] == 0) continue;
                        if ((rx == 0 || rx == (int)r2.Width - 1) || (ry == 0 || ry == (int)r2.Height - 1))
                        {
                            continue;
                        }
                        else
                        {
                            var deg1 = graph.Degree(edge.From());
                            var deg2 = graph.Degree(edge.To());
                            var degFactor = (int)((float)(deg1 + deg2) / 2);
                            var deg = Math.Clamp(126 / degFactor, 0, 126);

                            height.Values[x, y] = c + deg;
                        }
                    }
                }
            }

            foreach (var edge in tree)
            {
                var change = new List<(int, int)>();
                var (x1, y1) = ParseLabel(edge.From().GetLabel());
                var (x2, y2) = ParseLabel(edge.To().GetLabel());

                var p = new Vector2f(x1, y1);
                var q = new Vector2f(x2, y2);

                var dx = MathF.Abs(x1 - x2) / 2;
                var dy = MathF.Abs(y1 - y2) / 2;

                var m = (p + q) / 2;

                if (dx > 2f * dy)
                {
                    for (int i = (int)-dx; i < (int)dx; i++)
                    {
                        var px = (int)(m.X + i);
                        var py = (int)m.Y;
                        if (height.Values[px, py] < 128)
                            change.Add((px, py));
                    }
                }
                else if (dy > 2f * dx)
                {
                    for (int i = (int)-dy; i < (int)dy; i++)
                    {
                        var px = (int)m.X;
                        var py = (int)(m.Y + i);
                        if (height.Values[px, py] < 128)
                            change.Add((px, py));
                    }
                }
                else
                {
                    foreach (var (x, y) in Bresenham.Line(x1, y1, x2, y2))
                    {
                        if (height.Values[x, y] < 128)
                        height.Values[x, y] = c;
                    }
                }

                if (change.Count > 0)
                {
                    var c1 = height.Values[x1, y1];
                    var c2 = height.Values[x2, y2];
                    var min = Math.Min(c1, c2);
                    var max = Math.Max(c1, c2);

                    foreach (var (x, y) in change)
                    {
                        height.Values[x, y] = c;
                    }

                    var (dox, doy) = change[rand.Next(0, change.Count)];
                    if ((height.Values[dox - 1, doy] < 128 && height.Values[dox + 1, doy] < 128)
                      || (height.Values[dox, doy - 1] < 128 && height.Values[dox, doy + 1] < 128))
                    {
                        doors.Add((edge, (dox, doy)));
                    }
                }
            }
        }

        private static (int, int) ParseLabel(string label)
        {
            var parts = label.Split(",");
            var x = parts[0].Trim();
            var y = parts[1].Trim();
            return (int.Parse(x), int.Parse(y));
        }

        public override void Render(Svarog svarog)
        {
            float scale = 0.25f;
            var equi = svarog.resources.GetFromBag<BoolMap>("equimap");
            var map = svarog.resources.GetFromBag<IntMap>("voronoi");
            var height = svarog.resources.GetFromBag<FloatMap>("height");
            var graph = svarog.resources.GetFromBag<Graph>("graph");
            var tree = svarog.resources.GetFromBag<List<SharpGraph.Edge>>("tree");
            var doors = svarog.resources.GetFromBag< List<(SharpGraph.Edge, (int, int))>>("doors");

            if (graph == null) return;

            sprite.Color = Color.White;
            var s = svarog.resources.GetSprite("White");
            if (s != null)
            {
                sprite.Texture = s.Texture;
                sprite.TextureRect = s.Coords;
                sprite.Scale = new Vector2f(scale, scale);
            }

            for (int i = 0; i < 160; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    var p = sprite.Position;
                    p.X = i * 32 * scale;
                    p.Y = j * 32 * scale;
                    sprite.Position = p;
                    var v = (byte)height.Values[i, j];
                    sprite.Color = new Color(v, v, v, 255);
                    svarog.render?.Draw(sprite, new RenderStates(BlendMode.Add));
                }
            }

            int roomCounter = 1;
            
            foreach (var edge in tree)
            {
                var (px, py) = ParseLabel(edge.From().GetLabel());
                var (qx, qy) = ParseLabel(edge.To().GetLabel());

                svarog.render?.Draw(new Vertex[] {
                    new Vertex(new Vector2f((float)px * 32 + 16, (float)py * 32 + 16) * scale, colors[roomCounter]),
                    new Vertex(new Vector2f((float)qx * 32 + 16, (float)qy * 32 + 16) * scale, colors[roomCounter])
                }, PrimitiveType.Lines);
            }

            foreach (var door in doors)
            {
                var (i, j) = door.Item2;
                var p = sprite.Position;
                p.X = i * 32 * scale;
                p.Y = j * 32 * scale;
                sprite.Position = p;
                var v = (byte)height.Values[i, j];
                sprite.Color = new Color(v, v, v, 255);
                svarog.render?.Draw(sprite, new RenderStates(BlendMode.Add));
            }

            roomCounter++;
        }
    }
}
