using CommunityToolkit.HighPerformance;
using DelaunatorSharp;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            public Polygon(Vector2f[] polygon) { Points = polygon; }

            public Vector2f Centroid()
            {
                var x = Points.Select(p => p.X).Sum() / Points.Length;
                var y = Points.Select(p => p.Y).Sum() / Points.Length;
                return new Vector2f(x, y);
            }

            public FloatRect Bounds()
            {
                FloatRect rect = new FloatRect();
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
            d.ForEachTriangle((tri) =>
            {
                tris.Add(new Polygon(tri.Points.Select(p => new Vector2f((float)p.X, (float)p.Y)).ToArray()));
            });

            return tris;
        }

        public static List<Polygon> Polygonize(List<Vector2f> points)
        {
            var cells = new List<Polygon>();
            var d = new Delaunator(points.Select(p => new Point((double)p.X, (double)p.Y)).ToArray());
            d.ForEachVoronoiCellBasedOnCentroids((cell) =>
            {
                cells.Add(new Polygon(cell.Points.Select(p => new Vector2f((float)p.X, (float)p.Y)).ToArray()));
            });

            return cells;
        }
    }
}
