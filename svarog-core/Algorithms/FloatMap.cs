﻿namespace svarog.Algorithms
{
    public class FloatMap
    {
        public float[,] Values;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public static FloatMap Noise(int width, int height, float scale = 0.5f)
        {
            SimplexNoise.Noise.Seed++;
            var map = new FloatMap(width, height);
            map.Values = SimplexNoise.Noise.Calc2D(width, height, scale);
            return map;
        }

        public FloatMap(int width, int height)
        {
            Values = new float[width, height];
            Width = width;
            Height = height;
        }

        public IntMap ToIntMap()
        {
            var map = new IntMap(Width, Height);
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    map.Values[i, j] = (int)MathF.Round(Values[i, j]);
                }
            }
            return map;
        }

        public BoolMap ToBoolMap(Predicate<float> predicate)
        {
            var map = new BoolMap(Width, Height);
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    map.Values[i, j] = predicate(Values[i, j]);
                }
            }

            return map;
        }

        public FloatMap FilterBelow(int height)
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Values[i, j] = Values[i, j] < height ? 0 : Values[i, j];
                }
            }

            return this;
        }

        public FloatMap FilterAbove(int height)
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Values[i, j] = Values[i, j] > height ? 0 : Values[i, j];
                }
            }

            return this;
        }
    }
}