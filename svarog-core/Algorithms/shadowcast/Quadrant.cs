namespace svarog.Algorithms.shadowcast
{
    internal class Quadrant
    {
        internal enum Direction
        {
            North = 0,
            East = 1,
            South = 2,
            West = 3
        }

        private Direction _direction;
        private int _startX;
        private int _startY;
        public Quadrant(Direction direction, (int, int) coords)
        {
            _direction = direction;
            _startX = coords.Item1;
            _startY = coords.Item2;
        }

        public (int, int) Transform((int, int) tileCoords)
        {
            var row = tileCoords.Item1;
            var column = tileCoords.Item2;

            if (_direction == Direction.North)
            {
                return (_startX + column, _startY - row);
            }
            else if (_direction == Direction.South)
            {
                return (_startX + column, _startY + row);
            }
            else if (_direction == Direction.East)
            {
                return (_startX + row, _startY + column);
            }
            else if (_direction == Direction.West)
            {
                return (_startX - row, _startY + column);
            }

            return (0, 0);
        }
    }
}
