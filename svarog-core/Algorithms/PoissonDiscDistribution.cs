using SFML.System;

namespace svarog.Algorithms
{
    public static class PoissonDiscSampling
    {
        public static List<Vector2f> GeneratePoints(float radius, Vector2f sampleRegionSize, int numSamplesBeforeRejection = 30)
        {
            var random = new Random();
            float cellSize = radius / MathF.Sqrt(2);

            int[,] grid = new int[(int)MathF.Ceiling(sampleRegionSize.X / cellSize), (int)MathF.Ceiling(sampleRegionSize.Y / cellSize)];
            List<Vector2f> points = new();
            List<Vector2f> spawnPoints = new();

            spawnPoints.Add(sampleRegionSize / 2);
            while (spawnPoints.Count > 0)
            {
                int spawnIndex = random.Next(0, spawnPoints.Count);
                Vector2f spawnCentre = spawnPoints[spawnIndex];
                bool candidateAccepted = false;

                for (int i = 0; i < numSamplesBeforeRejection; i++)
                {
                    float angle = random.NextSingle() * MathF.PI * 2;
                    Vector2f dir = new Vector2f(MathF.Sin(angle), MathF.Cos(angle));
                    Vector2f candidate = spawnCentre + dir * (radius + random.NextSingle() * radius);
                    if (IsValid(candidate, sampleRegionSize, cellSize, radius, points, grid))
                    {
                        points.Add(candidate);
                        spawnPoints.Add(candidate);
                        grid[(int)(candidate.X / cellSize), (int)(candidate.Y / cellSize)] = points.Count;
                        candidateAccepted = true;
                        break;
                    }
                }
                if (!candidateAccepted)
                {
                    spawnPoints.RemoveAt(spawnIndex);
                }

            }

            return points;
        }

        static bool IsValid(Vector2f candidate, Vector2f sampleRegionSize, float cellSize, float radius, List<Vector2f> points, int[,] grid)
        {
            if (candidate.X >= 0 && candidate.X < sampleRegionSize.X && candidate.Y >= 0 && candidate.Y < sampleRegionSize.Y)
            {
                int cellX = (int)(candidate.X / cellSize);
                int cellY = (int)(candidate.Y / cellSize);
                int searchStartX = (int)MathF.Max(0, cellX - 2);
                int searchEndX = (int)MathF.Min(cellX + 2, grid.GetLength(0) - 1);
                int searchStartY = (int)MathF.Max(0, cellY - 2);
                int searchEndY = (int)MathF.Min(cellY + 2, grid.GetLength(1) - 1);

                for (int x = searchStartX; x <= searchEndX; x++)
                {
                    for (int y = searchStartY; y <= searchEndY; y++)
                    {
                        int pointIndex = grid[x, y] - 1;
                        if (pointIndex != -1)
                        {
                            float sqrDst = (candidate - points[pointIndex]).SqrMagnitude();
                            if (sqrDst < radius * radius)
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }
    }
}
