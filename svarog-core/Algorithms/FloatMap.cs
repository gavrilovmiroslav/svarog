using SFML.System;

namespace svarog.Algorithms
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

        public FloatMap Blur()
        {
            FloatMap newMap = new FloatMap(Width, Height);
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    var ns = Neighbors(i, j).ToArray();
                    var v = ns.Select(xy => Values[xy.X, xy.Y]).Sum() / ns.Length;
                    newMap.Values[i, j] = (Values[i, j] + v) / 2.0f;

                }
            }

            return newMap;
        }

        public IEnumerable<Vector2i> Neighbors(int x, int y)
        {
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (i == 0 && j == 0) continue;
                    if (x + i >= 0 && y + j >= 0 && x + i < Width && y + j < Height)
                    {
                        yield return new Vector2i(x + i, y + j);
                    }
                }
            }
        }

        public IEnumerable<Vector2i> DirectNeighbors(int x, int y)
        {
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (i != 0 && j != 0) continue;
                    if (i == 0 && j == 0) continue;
                    if (x + i >= 0 && y + j >= 0 && x + i < Width && y + j < Height)
                    {
                        yield return new Vector2i(x + i, y + j);
                    }
                }
            }
        }

        public FloatMap Copy(BoolMap pattern)
        {
            FloatMap newMap = new FloatMap(Width, Height);
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (pattern.Values[i, j])
                    {
                        newMap.Values[i, j] = this.Values[i, j];
                    }
                }
            }

            return newMap;
        }
    }
}
