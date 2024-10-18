using SFML.Graphics;
using SFML.Window;
using svarog.Effects;

namespace svarog.Plugins
{
    [Plugin(Priority = 1002)]
    public class CRTPlugin : PostprocessPlugin
    {
        Sprite mouse;

        public CRTPlugin() : base("CRT") 
        {
            mouse = new Sprite();
        }

        public override void Load(Svarog instance)
        {
            base.Load(instance);
            instance.window?.SetMouseCursorVisible(false);

            mouse.Scale = new SFML.System.Vector2f(0.1f, 0.1f);
            var rs = instance.resources.GetSprite("White");
            if (rs != null)
            {
                mouse.Texture = rs.Texture;
                mouse.TextureRect = rs.Coords;
            }
            mouse.Color = Color.White;

            instance.mouse.AddWarper(CRTMonitorWarp);
        }

        public override void Unload(Svarog instance)
        {
            base.Unload(instance);
            instance.window?.SetMouseCursorVisible(true);
            instance.mouse.RemoveWarper(CRTMonitorWarp);
        }

        public override void Render(Svarog instance)
        {
            base.Render(instance);
            var (x, y) = instance.mouse.Position;
            mouse.Position = new SFML.System.Vector2f(x, y);
            instance.render?.Draw(mouse);
        }

        private (int, int) CRTMonitorWarp((int, int) xy)
        {
            var warp = 0.2f;
            var zoom = -0.01f;
            
            var (x, y) = xy;
            var (u, v) = (x / 1280.0f, y / 800.0f);
            var (d, c) = (MathF.Abs(0.5f - u), MathF.Abs(0.5f - v));
            var (d2, c2) = (d * d, c * c);

            u -= 0.5f; u *= 1.0f + (c2 * (0.3f * warp)); u += 0.5f;
            v -= 0.5f; v *= 1.0f + (d2 * (0.4f * warp)); v += 0.5f;

            u = (u * (1 - zoom)) + zoom / 2;
            v = (v * (1 - zoom)) + zoom / 2;
            return ((int)MathF.Round(u * 1280.0f), (int)MathF.Round(v * 800.0f));
        }
    }
}
