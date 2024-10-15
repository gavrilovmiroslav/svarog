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
        public long frame;

        public Svarog()
        {
            clock = new Clock();
            resources = new Resources();
            plugins = new PluginManager(this);
            frame = 0;
        }
    }
}
