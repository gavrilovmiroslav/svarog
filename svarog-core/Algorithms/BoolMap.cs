
using FloodSpill;

namespace svarog.Algorithms
{
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
    }
}
