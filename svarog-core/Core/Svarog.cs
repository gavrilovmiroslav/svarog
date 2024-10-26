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

        public object? Invoke(string name, params (string, object)[] args)
        {
            if (!Game.RegisteredActions.ContainsKey(name))
            {
                Console.WriteLine($"Warning: cannot invoke registered action {name} -- action not found.");
                return null;
            }

            var (action, form) = Game.RegisteredActions[name];

            var dict = new Dictionary<string, object>();
            foreach(var (key, val) in args)
            {
                if (form.TryGetValue(key, out Type? type))
                {
                    if (val.GetType().IsAssignableFrom(type))
                    {
                        dict[key] = val;
                    }
                    else
                    {
                        Console.WriteLine($"Warning: parameter {name}#{key} expected to be {type.Name}, but {val.GetType()} found!");
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine($"Warning: parameter {name}#{key} not found!");
                    return null;
                }
            }

            return action.Invoke(this, dict);
        }
    }
}
