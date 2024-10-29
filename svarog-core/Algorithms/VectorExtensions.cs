using SFML.System;

namespace svarog.Algorithms
{
    public static class VectorExtensions
    {
        public static Vector2f Abs(this Vector2f v)
        {
            return new Vector2f(MathF.Abs(v.X), MathF.Abs(v.Y));
        }

        public static Vector2f Add(this Vector2f v, float f)
        {
            return new Vector2f(v.X + f, v.Y + f);
        }

        public static Vector2f Sub(this Vector2f v, float f)
        {
            return new Vector2f(v.X - f, v.Y - f);
        }

        public static Vector2f SubFrom(this Vector2f v, float f)
        {
            return new Vector2f(f - v.X, f - v.Y);
        }

        public static float Dot(this Vector2f v1, Vector2f v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }

        public static float SqrMagnitude(this Vector2f v)
        {
            return MathF.Sqrt(v.Dot(v));
        }

        public static Vector2f Div(this Vector2f v1, Vector2f v2)
        {
            return new Vector2f(v1.X / v2.X, v1.Y / v2.Y);
        }

        public static Vector2f Mult(this Vector2f v1, Vector2f v2)
        {
            return new Vector2f(v1.X * v2.X, v1.Y * v2.Y);
        }

        public static Vector2f Sqr(this Vector2f v) => v.Mult(v);

        public static Vector2f Sqrt(this Vector2f v) => new Vector2f(MathF.Sqrt(v.X), MathF.Sqrt(v.Y));

        public static Vector2f ToVec(this (int, int) xy) => new Vector2f(xy.Item1, xy.Item2);
        public static Vector2f ToVec(this (float, float) xy) => new Vector2f(xy.Item1, xy.Item2);

        public static Vector2f ToFloats(this Vector2u xy) => new Vector2f(xy.X, xy.Y);
        public static Vector2f ToFloats(this Vector2i xy) => new Vector2f(xy.X, xy.Y);
        public static Vector2i ToInts(this Vector2f xy) => new Vector2i((int)MathF.Round(xy.X), (int)MathF.Round(xy.Y));

        public static (float, float) AsTuple(this Vector2f xy) => (xy.X, xy.Y);
        public static (int, int) AsTuple(this Vector2i xy) => (xy.X, xy.Y);

        public static float Distance(this Vector2f v1, Vector2f v2)
        {
            var dv = v1 - v2;
            return MathF.Sqrt(dv.X * dv.X + dv.Y * dv.Y);
        }
    }
}
