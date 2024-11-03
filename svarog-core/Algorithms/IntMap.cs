using SFML.System;
using System.Diagnostics;

namespace svarog.Algorithms
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

        public IntMap Filter(Func<int, int, int, int> filter)
        {
            var map = new IntMap(Width, Height);

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    map.Values[i, j] = filter(i, j, Values[i, j]);
                }
            }

            return map;
        }

        public IntMap FilterInplace(Func<int, int, int, int> filter)
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Values[i, j] = filter(i, j, Values[i, j]);
                }
            }

            return this;
        }

        public IntMap FilterByBoolPredicate(BoolMap map)
        {
            Debug.Assert(Width == map.Width && Height == map.Height);
            var newMap = new IntMap(Width, Height);
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    newMap.Values[i, j] = map.Values[i, j] ? Values[i, j] : 0;
                }
            }

            return newMap;
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

        public IntMap Blur()
        {
            IntMap newMap = new IntMap(Width, Height);
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    var ns = Neighbors(i, j).ToArray();
                    var v = ns.Select(xy => Values[xy.X, xy.Y]).Sum() / ns.Length;
                    newMap.Values[i, j] = (Values[i, j] + v) / 2;
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

        public IntMap Copy(BoolMap pattern)
        {
            IntMap newMap = new IntMap(Width, Height);
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
