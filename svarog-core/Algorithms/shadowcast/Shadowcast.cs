namespace svarog.Algorithms.shadowcast
{
    public static class Shadowcast
    {
        public static BoolMap GenerateShadowCast(BoolMap boolMap, (int, int) startingCoordinates, int visionUnits)
        {
            var result = new BoolMap(boolMap.Width, boolMap.Height);
            MarkAsVisible(result, startingCoordinates);

            for (int i = 0; i < 4; i++)
            {
                var quadrant = new Quadrant((Quadrant.Direction)i, startingCoordinates);
                var firstRow = new RowData(1, -1f, 1f);
                Scan(boolMap, result, firstRow, quadrant, visionUnits);
            }

            return result;
        }

        private static void Scan(BoolMap map, BoolMap shadows, RowData row, Quadrant q, int visionUnits)
        {
            (int, int)? previousTile = null;
            foreach (var tile in row.GetTiles())
            {
                if (IsWall(map, tile, q) || IsSymetric(row, tile))
                {
                    MarkAsVisible(shadows, q.Transform(tile));
                }

                if (previousTile != null && IsWall(map, previousTile.GetValueOrDefault(), q) && IsFloor(map, tile, q))
                {
                    row.StartSlope = Slope(tile.Item1, tile.Item2);
                }

                if (previousTile != null && IsFloor(map, previousTile.GetValueOrDefault(), q) && IsWall(map, tile, q) && row.Depth < visionUnits)
                {
                    var nextRow = row.GetNextRowData();
                    nextRow.EndSlope = Slope(tile.Item1, tile.Item2);
                    Scan(map, shadows, nextRow, q, visionUnits);
                }

                previousTile = tile;
            }

            if (previousTile != null && IsFloor(map, previousTile.GetValueOrDefault(), q) && row.Depth < visionUnits)
            {
                Scan(map, shadows, row.GetNextRowData(), q, visionUnits);
            }
        }

        static bool IsSymetric(RowData row, (int, int) tile)
        {
            var col = tile.Item2;

            return (col >= row.Depth * row.StartSlope && col <= row.Depth * row.EndSlope);
        }

        static float Slope(int depth, int column) => ((float)2 * column - 1) / (2 * depth);

        static void MarkAsVisible(BoolMap outMap, (int, int) coordinates)
        {
            var width = outMap.Width;
            var height = outMap.Height;

            if (coordinates.Item1 >= 0 && coordinates.Item2 >= 0 && coordinates.Item1 < width && coordinates.Item2 < height)
                outMap.Values[coordinates.Item1, coordinates.Item2] = true;

        }

        static bool IsFloor(BoolMap map, (int, int) tile, Quadrant q) => !IsWall(map, tile, q);
        static bool IsWall(BoolMap map, (int, int) tile, Quadrant q)
        {
            (int, int) cords = q.Transform((tile.Item1, tile.Item2));
            if (cords.Item1 >= 0 && cords.Item2 >= 0 && cords.Item1 < map.Width && cords.Item2 < map.Height)
            {
                return map.Values[cords.Item1, cords.Item2];
            }

            return true;
        }
    }
}
