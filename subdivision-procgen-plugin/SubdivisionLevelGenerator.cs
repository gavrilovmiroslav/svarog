using SFML.System;
using SharpGraph;
using svarog;
using svarog.Algorithms;
using svarog.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static svarog.Plugins.Subdivision;

namespace subdivision_procgen_plugin
{
    public class SubdivisionLevelGenerator
    {
        public class LevelDescriptor
        {
            public (int, int) MapSize;
            public int DoorProbability;
            public Func<int, int, int> CorridorDistribution;
            public Graph FloorPlan;
            public IntMap RoomIdMap;
            public HashSet<string> Doors;
        }

        internal static (int, int) ParseLabel(string label)
        {
            var parts = label.Split(",");
            var x = parts[0].Trim();
            var y = parts[1].Trim();
            return (int.Parse(x), int.Parse(y));
        }

        public static LevelDescriptor Generate(Svarog instance, (int, int) mapSize, int doorProbability, Func<int, int, int> corridorConnections)
        {
            var rand = new Random();
            var (width, height) = mapSize;

            var equ = BoolMap.EquidistantSampling(width / 4, height / 4, ESamplingDistance.Low, 4.0f);
            Voronoi? v = (Voronoi?)instance.Invoke("voronoi", ("points", equ));
            
            var rooms = new IntMap(width, height);
            var heightmap = FloatMap.Noise(width, height);
            var noise = FloatMap.Noise(width, height, 0.9f);
            var connectedness = new Dictionary<int, int>();
            var doorSet = new HashSet<string>();
            // Initialize noisemap
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (v.Grid.Values[i, j] == 0)
                        heightmap.Values[i, j] = 0;
                    else
                        heightmap.Values[i, j] = MathF.Sqrt(heightmap.Values[i, j]);
                }
            }

            var mstree = v.Connectivity.GenerateMinimumSpanningTree(SpanningTreeAlgorithm.Kruskal, false);
            var treeGraph = new Graph(mstree);
            var addEdgesToTree = new List<SharpGraph.Edge>();

            foreach (var edge in mstree)
            {
                var (x1, y1) = ParseLabel(edge.From().GetLabel());
                var (x2, y2) = ParseLabel(edge.To().GetLabel());

                var c1 = v.Grid.Values[x1, y1];
                var c2 = v.Grid.Values[x2, y2];

                var cell1 = v.Polygons.Where(p => p.Index == c1).First();
                var cell2 = v.Polygons.Where(p => p.Index == c2).First();

                var roomByCells = new Dictionary<string, int>();

                // Make rooms
                foreach (var (cell, id) in new[] { (cell1, c1), (cell2, c2) })
                {
                    int cellCounter = 0;
                    var r = cell.Bounds();
                    for (int rx = 0; rx < (int)r.Width; rx++)
                    {
                        for (int ry = 0; ry < (int)r.Height; ry++)
                        {
                            var x = (int)r.Left + rx;
                            var y = (int)r.Top + ry;

                            if (heightmap.Values[x, y] == 0) continue;
                            if ((rx == 0 || rx == (int)r.Width - 1) || (ry == 0 || ry == (int)r.Height - 1))
                            {
                                continue;
                            }
                            else
                            {
                                if (rand.Next(0, 100) > corridorConnections(x, y))
                                {
                                    var neighbors = rooms.Neighbors(x, y);
                                    if (neighbors.Any(n => rooms.Values[n.X, n.Y] > 0 && rooms.Values[n.X, n.Y] != id))
                                    {
                                        var vid = v[id];
                                        if (vid.HasValue && v.Connectivity.Degree(vid.Value) <= 3)
                                        {
                                            var others = neighbors.Select(n => v.Grid.Values[n.X, n.Y]).Distinct().Where(i => i != id && i != 0).ToList();
                                            if (others.Count == 0)
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                foreach (var other in others)
                                                {
                                                    if (!v[other].HasValue) continue;
                                                    var vot = v[other].Value;
                                                    if (!treeGraph.GetEdge(vid.Value, vot).HasValue)
                                                    {
                                                        v.Connectivity.AddEdge(vid.Value, vot);
                                                        addEdgesToTree.Add(v.Connectivity.GetEdge(vid.Value, vot).Value);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    } 
                                }

                                heightmap.Values[x, y] = 255;
                                rooms.Values[x, y] = id;
                                cellCounter++;
                            }
                        }
                    }
                }
            }

            foreach (var edge in addEdgesToTree) mstree.Add(edge);

            // Connectedness analysis
            {
                var treegraph = new Graph(mstree);
                foreach (var node in treegraph.GetNodes())
                {
                    var (x, y) = ParseLabel(node.GetLabel());
                    var roomId = rooms.Values[x, y];
                    var conn = treegraph.Degree(node);
                    connectedness[roomId] = conn;
                }
            }

            // Make corridors and doors
            var doors = new MultiMap<SharpGraph.Edge, (int, int)>();
            foreach (var edge in mstree)
            {
                var change = new List<(int, int)>();
                var delete = new List<(int, int)>();
                var (x1, y1) = ParseLabel(edge.From().GetLabel());
                var (x2, y2) = ParseLabel(edge.To().GetLabel());
                var id1 = v.Grid.Values[x1, y1];
                var id2 = v.Grid.Values[x2, y2];

                var p = new Vector2f(x1, y1);
                var q = new Vector2f(x2, y2);

                var dx = MathF.Abs(x1 - x2) / 2;
                var dy = MathF.Abs(y1 - y2) / 2;

                var m = (p + q) / 2;

                if (dx > 2.25f * dy)
                {
                    for (int i = (int)-dx; i < (int)dx; i++)
                    {
                        var px = (int)(m.X + i);
                        var py = (int)m.Y;
                        var pp = new Vector2f(px, py);
                        if (heightmap.Values[px, py] < 128)
                        {
                            change.Add((px, py));
                            delete.Add((px, py - 1));
                            delete.Add((px, py + 1));
                        }
                    }
                }
                else if (dy > 2f * dx)
                {
                    for (int i = (int)-dy; i < (int)dy; i++)
                    {
                        var px = (int)m.X;
                        var py = (int)(m.Y + i);
                        var pp = new Vector2f(px, py);
                        if (heightmap.Values[px, py] < 128)
                        {
                            change.Add((px, py));
                            delete.Add((px - 1, py));
                            delete.Add((px + 1, py));
                        }
                    }
                }
                else
                {
                    foreach (var (x, y) in Bresenham.Line(x1, y1, x2, y2))
                    {
                        if (heightmap.Values[x, y] < 128)
                        {
                            heightmap.Values[x, y] = 255;
                            rooms.Values[x, y] = 255;
                        }
                    }
                }

                if (delete.Count > 0)
                {
                    foreach (var (x, y) in delete)
                    {
                        heightmap.Values[x, y] = 0;
                        rooms.Values[x, y] = 0;
                    }
                }

                if (change.Count > 0)
                {
                    var c1 = heightmap.Values[x1, y1];
                    var c2 = heightmap.Values[x2, y2];
                    var min = Math.Min(c1, c2);
                    var max = Math.Max(c1, c2);

                    foreach (var (x, y) in change)
                    {
                        heightmap.Values[x, y] = 255;
                        rooms.Values[x, y] = 255;
                    }

                    connectedness.TryGetValue(v.Grid.Values[x1, y1], out int conn1);
                    connectedness.TryGetValue(v.Grid.Values[x2, y2], out int conn2);

                    var conns = conn1 + conn2;
                    var maxConn = Math.Max(conn1, conn2);

                    if (conn1 + conn2 < 3 || maxConn == 5 || rand.Next(0, 100) > (100 - doorProbability))
                    {
                        var midc = change.Select(c => c.ToVec()).Aggregate((a, b) => a + b);
                        midc = midc / change.Count;
                        var (dox, doy) = change.OrderBy(c => c.ToVec().Distance(midc)).First();
                        if ((heightmap.Values[dox - 1, doy] < 128 && heightmap.Values[dox + 1, doy] < 128)
                          || (heightmap.Values[dox, doy - 1] < 128 && heightmap.Values[dox, doy + 1] < 128))
                        {
                            doors.Add(edge, (dox, doy));
                        }
                    }
                }
            }

            var removeDoors = new List<SharpGraph.Edge>();
            foreach (var edge in doors.Keys)
            {
                var from = edge.From();
                var to = edge.To();
                mstree.Remove(edge);

                if (mstree.Any(e => (e.From() == from && e.To() == to) || (e.From() == to && e.To() == from)))
                {
                    removeDoors.Add(edge);
                }
                else
                {
                    foreach (var (x, y) in doors[edge])
                    {
                        var key = $"{x},{y}";
                        if (key == from.GetLabel() || key == to.GetLabel()) continue;
                        doorSet.Add(key);
                        v.Connectivity.AddNode(new Node(key));
                        var door = v.Connectivity.GetNode(key).Value;
                        v.Connectivity.AddEdge(from, door);
                        v.Connectivity.AddEdge(door, to);

                        mstree.Add(v.Connectivity.GetEdge(from, door).Value);
                        mstree.Add(v.Connectivity.GetEdge(door, to).Value);
                    }
                }
            }

            foreach (var edge in removeDoors)
            {
                doors.RemoveAll(edge);
            }

            treeGraph = new Graph(mstree);

            Console.WriteLine(treeGraph.GetConnectedComponents().Count);
            return new LevelDescriptor { 
                MapSize = mapSize,
                DoorProbability = doorProbability,
                CorridorDistribution = corridorConnections,
                FloorPlan = treeGraph, 
                RoomIdMap = rooms,
                Doors = doorSet
            };
        }
    }
}
