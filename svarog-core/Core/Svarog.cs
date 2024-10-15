﻿using SFML.Graphics;
using SFML.System;
using svarog_core.Structures;

namespace svarog_core
{
    public class Resources
    {
        public Dictionary<string, Texture> Textures = new();
        public Dictionary<string, (Texture, IntRect)> Sprites = new();
        public MultiMap<string, string> NamedSprites = new();
    }

    public class Svarog
    {
        public Clock clock;
        public Resources resources;
        public PluginManager plugins;
        public ExternalPluginManager externalPlugins;
        public RenderWindow? window;
        public RenderTexture? render;
        public long frame;

        public Svarog()
        {
            clock = new Clock();
            resources = new Resources();
            plugins = new PluginManager();
            externalPlugins = new ExternalPluginManager(this);
            frame = 0;
        }
    }
}
