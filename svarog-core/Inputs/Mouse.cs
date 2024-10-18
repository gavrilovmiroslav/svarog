using static SFML.Window.Mouse;

namespace svarog.Inputs
{
    public class Mouse : InputManager<Button>
    {
        private (int, int) position = (0, 0);
        private List<Func<(int, int), (int, int)>> Warpers = new();
        
        public (int, int) Position => Warpers.Aggregate(position, ((int, int) xy, Func<(int, int), (int, int)> f) => f(xy));

        public void AddWarper(Func<(int, int), (int, int)> warper)
        {
            Warpers.Add(warper);
        }

        public void RemoveWarper(Func<(int, int), (int, int)> warper)
        {
            Warpers.Remove(warper);
        }

        public void Move(int x, int y)
        {
            position.Item1 = x;
            position.Item2 = y;
        }
    }
}
