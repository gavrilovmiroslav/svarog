using Arch.Core;
using SFML.Graphics;
using SFML.System;

namespace svarog
{
    public class Svarog
    {
        public Clock clock;
        public World world;
        public Resources resources;
        public PluginManager plugins;
        public RenderWindow? window;
        public RenderTexture? render;
        public Inputs.Keyboard keyboard;
        public Inputs.Mouse mouse;
        public long frame;

        public Svarog()
        {
            clock = new Clock();
            world = World.Create();
            resources = new Resources();
            keyboard = new Inputs.Keyboard();
            mouse = new Inputs.Mouse();
            frame = 0;

            plugins = new PluginManager(this);
        }
    }
}
