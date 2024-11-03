
using FloodSpill;
using SFML.System;

namespace svarog.Algorithms
{
    public enum ESamplingDistance
    {
        Minimal = 1,
        Low = 2,
        Moderate = 3,
        High = 4,
    }

    public class BoolMap
    {
        public bool[,] Values;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public BoolMap(int width, int height)
        {
            Values = new bool[width, height];
            Width = width;
            Height = height;
        }

        public BoolMap Clear()
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Values[i, j] = false;
                }
            }

            return this;
        }

        public static BoolMap Random(int width, int height, int percentFilled)
        {
            var map = new BoolMap(width, height);
            var full = width * height;
            if (percentFilled > 0)
            {
                var choose = (int)MathF.Round((float)full * (float)percentFilled / 100.0f);
                var rand = new Random();

                while (choose > 0)
                {
                    var x = rand.Next(0, width);
                    var y = rand.Next(0, height);
                    if (!map.Values[x, y])
                        map.Values[x, y] = true;
                    choose--;
                }
            }

            return map;
        }

        public IntMap? Flood(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) { return null; }
            var map = new IntMap(Width, Height);
            new FloodSpiller().SpillFlood(new FloodParameters(startX: x, startY: y) { Qualifier = (x, y) => Values[x, y] }, map.Values);
            return map;
        }

        public static BoolMap EquidistantSampling(int width, int height, ESamplingDistance distance, float scale = 1.0f)
        {
            var size = new Vector2f(width * scale, height * scale);
            var map = new BoolMap((int)size.X, (int)size.Y);
            var samples = PoissonDiscSampling.GeneratePoints(1.0f + (int)distance * MathF.Pow(0.25f, 1 + (float)distance * 0.01f), new Vector2f(width, height), 50);
            foreach (var sample in samples)
            {
                var x = (int)(sample.X * scale);
                var y = (int)(sample.Y * scale);
                if (x >= 0 && y >= 0 && x < size.X && y < size.Y)
                {
                    map.Values[x, y] = true;
                }
            }

            return map;
        }

        public BoolMap Find(IntPattern3x3 pattern)
        {
            bool CheckMatrix(int i, int j)
            {
                for (byte x = 0; x < 3; x++)
                {
                    for (byte y = 0; y < 3; y++)
                    {
                        if (pattern.IsImportant(x, y))
                        {
                            if (Values[i + x - 1, j + y - 1] != (pattern.Matrix[x, y] == EPattern.T))
                            {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }

            List<(int, int)> toChange = [];

            for (int i = 1; i < Width - 1; i++)
            {
                for (int j = 1; j < Height - 1; j++)
                {
                    if (CheckMatrix(i, j))
                    {
                        toChange.Add((i, j));
                    }
                }
            }

            var result = new BoolMap(Width, Height);
            foreach (var (i, j) in toChange)
            {
                result.Values[i, j] = true;
            }
            return result;
        }

        public static int TruthinessToInt(bool b) => b ? 1 : 0;

        public IntMap ToIntMap(Func<bool, int> predicate)
        {
            var map = new IntMap(Width, Height);
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    map.Values[i, j] = predicate(Values[i, j]);
                }
            }

            return map;
        }

        public BoolMap InplaceCombine(BoolMap map)
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Values[i, j] = Values[i, j] || map.Values[i, j];
                }
            }

            return this;
        }

        public BoolMap Filter(Func<int, int, bool, bool> filter)
        {
            var map = new BoolMap(Width, Height);

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    map.Values[i, j] = filter(i, j, Values[i, j]);
                }
            }

            return map;
        }

        public BoolMap FilterInplace(Func<int, int, bool, bool> filter)
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

        public BoolMap Combine(BoolMap map)
        {
            var newMap = new BoolMap(Width, Height);
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    newMap.Values[i, j] = Values[i, j] || map.Values[i, j];
                }
            }
            return newMap;
        }
    }
}
