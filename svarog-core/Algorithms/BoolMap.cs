
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
    }
}
