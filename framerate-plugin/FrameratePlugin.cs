using svarog_core;

namespace svarog.Plugins
{
    [Plugin(Priority = 15)]
    public class FrameratePlugin : Plugin
    {
        int time;
        int counter;

        public override void Load(Svarog instance)
        {
            time = 0;
            counter = 0;
            instance.window?.SetTitle($"Svarog -- FPS: 120");
        }

        public override void Frame(Svarog instance)
        {
            counter++;
            time += instance.clock.ElapsedTime.AsMilliseconds();
            if (time >= 1000)
            {
                instance.window?.SetTitle($"Svarog -- FPS: {counter}");
                time = 0;
                counter = 0;
            }
        }
    }
}
