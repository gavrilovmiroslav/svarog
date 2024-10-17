using Arch.Core;
using SFML.Graphics;
using SFML.System;
using svarog.Structures;

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
        public svarog_core.Inputs.Keyboard keyboard;
        public svarog_core.Inputs.Mouse mouse;
        public long frame;

        public Svarog()
        {
            clock = new Clock();
            world = World.Create();
            resources = new Resources();
            plugins = new PluginManager(this);
            keyboard = new svarog_core.Inputs.Keyboard();
            mouse = new svarog_core.Inputs.Mouse();
            frame = 0;
        }
    }
}
