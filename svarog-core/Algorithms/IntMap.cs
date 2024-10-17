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

    }
}
