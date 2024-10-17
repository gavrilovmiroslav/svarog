using svarog.Effects;

namespace svarog.Plugins
{
    [Plugin(Priority = 1002)]
    public class CRTPlugin : PostprocessPlugin
    {
        public CRTPlugin() : base("CRT") {}

        public override void Load(Svarog instance)
        {
            base.Load(instance);
            instance.mouse.AddWarper(CRTMonitorWarp);
        }

        public override void Unload(Svarog instance)
        {
            base.Unload(instance);
            instance.mouse.RemoveWarper(CRTMonitorWarp);
        }

        private (int, int) CRTMonitorWarp((int, int) xy)
        {
            var warp = 0.5f;
            var zoom = -0.05f;
            
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
