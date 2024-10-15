using svarog_core.Structures;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace svarog_core
{
    public interface IPlugin
    {
        void Load(Svarog instance);
        void Render(Svarog instance);
        void Frame(Svarog instance);
        void Unload(Svarog instance);
    }

    public class Plugin : IPlugin
    {
        public virtual void Load(Svarog instance) { }
        public virtual void Render(Svarog instance) { }
        public virtual void Frame(Svarog instance) { }
        public virtual void Unload(Svarog instance) { }
        public void Stop()
        {
            Game.OnLoad -= Load;
            Game.OnRender -= Render;
            Game.OnFrame -= Frame;
            Game.OnUnload -= Unload;
        }
    }

    public class PluginAttribute : Attribute
    {
        public bool Autoload { get; set; } = true;
        public int Priority { get; set; } = 100;
    }
}
