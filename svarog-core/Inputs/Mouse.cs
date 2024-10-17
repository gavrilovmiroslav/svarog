using static SFML.Window.Mouse;

namespace svarog_core.Inputs
{
    public class Mouse : InputManager<Button>
    {
        private (int, int) position = (0, 0);

        public (int, int) Position => position;

        public void Move(int x, int y)
        {
            position.Item1 = x;
            position.Item2 = y;
        }
    }
}
