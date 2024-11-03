using SFML.Graphics;
using SFML.System;

namespace svarog.Algorithms
{
    public enum ELerp
    {
        Linear,
        Cubic,
    }

    public static class Lerp
    {
        public static float Float(float x, float y, float t, ELerp kind)
        {
            return kind switch
            {
                ELerp.Linear => Linear(x, y, t),
                ELerp.Cubic => Cubic(x, y, t),
                _ => throw new NotImplementedException(),
            };
        }

        public static Vector2f Vec2(Vector2f x, Vector2f y, float t, ELerp kind)
        {
            return kind switch
            {
                ELerp.Linear => Linear(x, y, t),
                ELerp.Cubic => Cubic(x, y, t),
                _ => throw new NotImplementedException(),
            };
        }

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
            float CubicTransformation(float t) => 1 - MathF.Pow(1 - t, 3);

            return x + (y - x) * CubicTransformation(t);
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
