using SFML.Graphics;
using svarog.Effects;
using static SFML.Window.Keyboard;

namespace svarog.Plugins
{
    [Plugin(Priority = 1010)]
    public class EmergencyExitPlugin : Plugin
    {
        private Scancode escapeKey = Scancode.Escape;
        SFML.Graphics.RectangleShape countdownBar;
        Shader? gradient;

        public EmergencyExitPlugin()
        {
            gradient = ShaderUtility.LoadFromName("Gradient");
            countdownBar = new RectangleShape();
            countdownBar.FillColor = Color.White;
            countdownBar.Position = new SFML.System.Vector2f(0, 0);
            countdownBar.Size = new SFML.System.Vector2f(1280, 5);
        }

        public override void Frame(Svarog instance)
        {
            var esc = instance.keyboard.GetHoldDuration(escapeKey);
            if (esc >= 3000.0)
            {
                instance.window?.Close();
            }
        }

        public override void Render(Svarog instance)
        {
            var esc = instance.keyboard.GetHoldDuration(escapeKey);
            if (gradient != null)
            {
                gradient.SetUniform("size", (float)(esc / 3000.0));
                instance.render?.Draw(countdownBar, new RenderStates(gradient));
            }
        }
    }
}
