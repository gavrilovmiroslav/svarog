using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace svarog.Algorithms
{
    public static class Lerp
    {
        public static float Linear(float x, float y, float t)
        {
            return x + (y - x) * t;
        }

        public static float Cubic(float x, float y, float t)
        {
            return x + (y - x) * t * t * t;
        }

        public static Vector2f Linear(Vector2f x, Vector2f y, float t)
        {
            return x + (y - x) * t;
        }

        public static Vector2f Cubic(Vector2f x, Vector2f y, float t)
        {
            return x + (y - x) * t * t * t;
        }

        public static Color Linear(Color c1, Color c2, float t)
        {
            var r = Linear(c1.R, c2.R, t);
            var g = Linear(c1.G, c2.G, t);
            var b = Linear(c1.B, c2.B, t);
            var a = Linear(c1.A, c2.A, t);
            return new Color((byte)r, (byte)g, (byte)b, (byte)a);
        }

        public static Color Cubic(Color c1, Color c2, float t)
        {
            var t3 = t * t * t;
            var r = Linear(c1.R, c2.R, t3);
            var g = Linear(c1.G, c2.G, t3);
            var b = Linear(c1.B, c2.B, t3);
            var a = Linear(c1.A, c2.A, t3);
            return new Color((byte)r, (byte)g, (byte)b, (byte)a);
        }
    }
}
