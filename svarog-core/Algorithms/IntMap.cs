﻿namespace svarog.Algorithms
{
    public class IntMap
    {
        public int[,] Values;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public IntMap(int width, int height)
        {
            Values = new int[width, height];
            Width = width;
            Height = height;
        }

        public FloatMap ToFloatMap()
        {
            var map = new FloatMap(Width, Height);
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    map.Values[i, j] = (float)Values[i, j];
                }
            }
            return map;
        }

        public static IntMap Noise(int width, int height, float scale = 0.5f, int mult = 100)
        {
            return FloatMap.Noise(width, height, scale).ToIntMap();
        }

        public BoolMap ToBoolMap(Predicate<int> predicate)
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

        public IntMap FilterBelow(int height)
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

        public IntMap FilterAbove(int height)
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
