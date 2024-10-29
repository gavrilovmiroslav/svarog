using SFML.Graphics;
using SFML.Window;
using svarog.Structures;
using FActionDescriptor = (System.Func<svarog.Svarog, 
    System.Collections.Generic.Dictionary<string, object>, object>, 
    System.Collections.Generic.Dictionary<string, System.Type>);

namespace svarog
{
    public class Game
    {
        internal static Dictionary<string, FActionDescriptor> RegisteredActions = [];
        internal static MultiMap<string, string> RegisteredFunctions = new();

        internal static List<RPlugin> OnRegister = [];
        internal static List<RPlugin> OnLoad = [];
        internal static List<RPlugin> OnRender = [];
        internal static List<RPlugin> OnFrame = [];
        internal static List<RPlugin> OnUnload = [];
        
        public static void Start()
        {
            var svarog = new Svarog
            {
                window = new RenderWindow(new VideoMode(1280, 800), "Svarog"),
                render = new RenderTexture(1280, 800),
            };

            svarog.window.SetKeyRepeatEnabled(false);

            svarog.window.KeyPressed += (sender, e) => svarog.keyboard.InputDown(e.Scancode);
            svarog.window.KeyReleased += (sender, e) => svarog.keyboard.InputUp(e.Scancode);
            svarog.window.MouseMoved += (sender, e) => svarog.mouse.Move(e.X, e.Y);
            svarog.window.MouseButtonPressed += (sender, e) => svarog.mouse.InputDown(e.Button);
            svarog.window.MouseButtonReleased += (sender, e) => svarog.mouse.InputUp(e.Button);

            svarog.window.Closed += (window, _) => ((RenderWindow?)window)?.Close();
            svarog.window.SetFramerateLimit(120); // TODO: move to config

            OnRegister.Invoke(svarog);

            Sprite screen = new();

            while (svarog.window.IsOpen)
            {
                svarog.clock.Restart();
                svarog.window.DispatchEvents();

                if (svarog.plugins.IsReady)
                {
                    svarog.window.Clear(Color.Black);
                    svarog.render.Clear(Color.Black);

                    OnRender.Invoke(svarog);
                }

                svarog.render.Display();
                screen.Texture = svarog.render.Texture;
                svarog.window.Draw(screen);
                svarog.window.Display();

                OnFrame.Invoke(svarog);
                svarog.keyboard.Frame();
                svarog.mouse.Frame();
                Thread.Yield();

                svarog.frame++;                
                if (svarog.frame % 60 == 0) // TODO: move to config
                {
                    svarog.plugins.Update();
                }
            }
        }
    }
}
