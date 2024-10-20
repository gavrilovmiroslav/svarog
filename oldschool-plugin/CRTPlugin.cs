using SFML.Graphics;
using SFML.System;
using SFML.Window;
using svarog.Algorithms;
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
            
            var v = xy.ToVec();
            var r = (1280.0f, 800.0f).ToVec();
            var uv = v.Div(r);
            var dc = uv.SubFrom(0.5f).Abs();
            var dc2 = dc.Sqr();
            uv.X -= 0.5f; uv.X *= 1.0f + (dc2.Y * (0.3f * warp)); uv.X += 0.5f;
            uv.Y -= 0.5f; uv.Y *= 1.0f + (dc2.X * (0.4f * warp)); uv.Y += 0.5f;
            uv = (uv * (1 - zoom)).Add(zoom / 2).Mult(r);
            return uv.ToInts().AsTuple();
        }
    }
}
