using DelaunatorSharp;
using SFML.Graphics;
using SFML.System;
using SharpGraph;
using System.Diagnostics.Metrics;

namespace svarog.Algorithms
{
    class Point(double x, double y) : IPoint
    {
        public double X { get => x; set => x = value; }
        public double Y { get => y; set => y = value; }
    }

    public class Subdivision
    {
        public class Polygon
        {
            public Vector2f[] Points { get; set; }
            public int Index { get; set; }
            public Polygon(int index, Vector2f[] polygon) { Index = index; Points = polygon; }

            public Vector2f Centroid()
            {
                var x = Points.Select(p => p.X).Sum() / Points.Length;
                var y = Points.Select(p => p.Y).Sum() / Points.Length;
                return new Vector2f(x, y);
            }

            public FloatRect Bounds()
            {
                FloatRect rect = new();
                var minx = Points.Select(p => p.X).Min();
                var maxx = Points.Select(p => p.X).Max();
                var miny = Points.Select(p => p.Y).Min();
                var maxy = Points.Select(p => p.Y).Max();
                rect.Left = minx;
                rect.Top = miny;
                rect.Width = maxx - minx;
                rect.Height = maxy - miny;
                return rect;
            }

            public bool IsPointInPolygon(Vector2f testPoint)
            {
                var polygon = this.Points;
                bool result = false;
                int j = polygon.Length - 1;
                for (int i = 0; i < polygon.Length; i++)
                {
                    if (polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y ||
                        polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y)
                    {
                        if (polygon[i].X + (testPoint.Y - polygon[i].Y) /
                           (polygon[j].Y - polygon[i].Y) *
                           (polygon[j].X - polygon[i].X) < testPoint.X)
                        {
                            result = !result;
                        }
                    }
                    j = i;
                }
                return result;
            }
        }

        public static List<Polygon> Triangulate(List<Vector2f> points)
        {
            var tris = new List<Polygon>();
            var d = new Delaunator(points.Select(p => new Point((double)p.X, (double)p.Y)).ToArray());
            d.ForEachTriangle(tri =>
            {
                tris.Add(new Polygon(tri.Index, tri.Points.Select(p => new Vector2f((float)p.X, (float)p.Y)).ToArray()));
            });

            return tris;
        }

        public static (List<Polygon>, Graph, IntMap) Polygonize(IntMap dots)
        {
            var points = new List<Vector2f>();

            for (int i = 0; i < dots.Width; i++)
            {
                for (int j = 0; j < dots.Height; j++)
                {
                    if (dots.Values[i, j] > 0)
                    {
                        points.Add(new Vector2f(i, j));
                    }
                }
            }

            var graph = new Graph();
            var cells = new List<Polygon>();
            var edges = new Dictionary<(string, string), DelaunatorSharp.Edge>();
            var voronoi = new IntMap(dots.Width, dots.Height);

            var d = new Delaunator(points.Select(p => new Point((double)p.X, (double)p.Y)).ToArray());

            d.ForEachVoronoiCell(cell =>
            {
                var p = new Polygon(cell.Index, cell.Points.Select(p => new Vector2f((float)p.X, (float)p.Y)).ToArray());
                var b = p.Bounds();
                if (b.Width >= 12 && b.Height >= 10)
                {
                    for (int x = (int)b.Left - 1; x <= (int)(b.Left + b.Width); x++)
                    {
                        for (int y = (int)b.Top - 1; y <= (int)(b.Top + b.Height); y++)
                        {
                            if (p.IsPointInPolygon(new Vector2f(x, y)))
                            {
                                voronoi.Values[x, y] = cell.Index;
                            }
                        }
                    }

                    cells.Add(p);
                }
            });

            Dictionary<string, Node> nodes = new();
            Dictionary<(string, string), SharpGraph.Edge> edgs = new();

            foreach (var edge in d.GetEdges())
            {
                var px = (int)edge.P.X;
                var py = (int)edge.P.Y;

                if (voronoi.Values[px, py] == 0) continue;
                
                var qx = (int)edge.Q.X;
                var qy = (int)edge.Q.Y;

                if (voronoi.Values[qx, qy] == 0) continue;

                var ptp = new Vector2f(px, py);
                var ptq = new Vector2f(qx, qy);
                var s = (ptp - ptq).Sqr();
                var dist = MathF.Sqrt(s.X + s.Y);

                var pxy = $"{px},{py}";
                var qxy = $"{qx},{qy}";

                if (!nodes.ContainsKey(pxy)) { nodes.Add(pxy, new Node(pxy)); }
                if (!nodes.ContainsKey(qxy)) { nodes.Add(qxy, new Node(qxy)); }

                var p = nodes[pxy];
                var q = nodes[qxy];
                var e = new SharpGraph.Edge(p, q);

                graph.AddNode(pxy);
                graph.AddNode(qxy);
                graph.AddEdge(pxy, qxy);
                graph.AddComponent<EdgeWeight>(e).Weight = dist;
                graph.AddComponent<EdgeDirection>(e).Direction = Direction.Both;
                edges.Add((pxy, qxy), new DelaunatorSharp.Edge(edge.Index, edge.P, edge.Q));
            }

            return (cells, graph, voronoi);
        }
    }
}
