using static SFML.Window.Mouse;

namespace svarog_core.Inputs
{
    public class Mouse : InputManager<Button>
    {
        private (int, int) Position = (0, 0);

        public void Move(int x, int y)
        {
            Position.Item1 = x;
            Position.Item2 = y;
        }
    }
}
