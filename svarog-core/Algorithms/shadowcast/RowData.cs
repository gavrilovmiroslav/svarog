namespace svarog.Algorithms.shadowcast
{
    internal class RowData
    {
        public int Depth;
        public float StartSlope;
        public float EndSlope;

        public RowData(int depth, float startSlope, float endSlope)
        {
            Depth = depth;
            StartSlope = startSlope;
            EndSlope = endSlope;
        }

        public RowData GetNextRowData()
        {
            return new RowData(Depth + 1, StartSlope, EndSlope);
        }

        public List<(int, int)> GetTiles()
        {
            var min_col = (int)Math.Floor(Depth * StartSlope + 0.5);
            var max_col = (int)Math.Ceiling(Depth * EndSlope - 0.5);
            if (min_col > max_col)
                return new List<(int, int)>();

            return new List<(int, int)>(Enumerable.Zip(Enumerable.Repeat(Depth, max_col - min_col + 1), Enumerable.Range(min_col, max_col - min_col + 1)));
        }
    }
}
