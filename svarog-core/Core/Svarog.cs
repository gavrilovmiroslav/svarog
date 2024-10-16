using SFML.Graphics;
using SFML.System;
using svarog.Structures;

namespace svarog
{
    public class Resources
    {
        public Dictionary<string, Texture> Textures = new();
        public Dictionary<string, (Texture, IntRect)> Sprites = new();
        public MultiMap<string, string> NamedSprites = new();

        public (Texture, IntRect)? GetSprite(string name)
        {
            if (Sprites.TryGetValue(name, out var sprite)) 
                return sprite;
            else 
                return null;
        }
    }

    public class Svarog
    {
        public Clock clock;
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
            resources = new Resources();
            plugins = new PluginManager(this);
            keyboard = new svarog_core.Inputs.Keyboard();
            mouse = new svarog_core.Inputs.Mouse();
            frame = 0;
        }
    }
}
