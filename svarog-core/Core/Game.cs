using SFML.Graphics;
using SFML.Window;

namespace svarog_core
{
    public class Game
    {
        internal static Action<Svarog>? OnLoad;
        internal static event Action<Svarog>? OnRender;
        internal static event Action<Svarog>? OnFrame;
        internal static event Action<Svarog>? OnUnload;

        public static void Start()
        {
            var svarog = new Svarog
            {
                window = new RenderWindow(new VideoMode(1280, 800), "Svarog"),
                render = new RenderTexture(1280, 800)
            };

            svarog.window.KeyPressed += Window_KeyPressed;
            svarog.window.Closed += (window, _) => ((RenderWindow?)window)?.Close();
            svarog.window.SetFramerateLimit(120);

            OnLoad?.Invoke(svarog);
            Sprite screen = new();

            while (svarog.window.IsOpen)
            {
                svarog.clock.Restart();
                svarog.window.DispatchEvents();

                if (svarog.externalPlugins.IsReady)
                {
                    svarog.window.Clear(Color.Black);
                    svarog.render.Clear(Color.Black);

                    OnRender?.Invoke(svarog);
                }

                svarog.render.Display();
                screen.Texture = svarog.render.Texture;
                svarog.window.Draw(screen);
                svarog.window.Display();

                OnFrame?.Invoke(svarog);
                Thread.Yield();
            }
        }

        private static void Window_KeyPressed(object? sender, KeyEventArgs e)
        {
            if (e.Code == Keyboard.Key.F10)
            {
                ((RenderWindow?)sender)?.Close();
            }
        }
    }
}
