namespace svarog
{
    public record RPlugin(string Name, Action<Svarog> Act, int Priority);

    public class PluginExportBuilder
    {
        private string Name;
        private Dictionary<string, Type> Args = [];

        public PluginExportBuilder(string name)
        {
            Name = name;
        }

        public PluginExportBuilder With<T>(string name)
        {
            Args.Add(name, typeof(T));
            return this;
        }

        public void Returning(Func<Svarog, Dictionary<string, object>, object> action)
        {
            if (Game.RegisteredActions.ContainsKey(Name))
            {
                Console.WriteLine($"Warning: removing registered action {Name} and exporting new!");
            }
    
            Game.RegisteredActions[Name] = (action, Args);
            Game.RegisteredFunctions.Add(PluginManager.CurrentlyLoadedPlugin, Name);
        }
    }
    
    public interface IPlugin
    {
        void Register(Svarog instance);
        void Load(Svarog instance);
        void Render(Svarog instance);
        void Frame(Svarog instance);
        void Unload(Svarog instance);
    }

    public class Plugin : IPlugin
    {
        public virtual void Register(Svarog instance) { }
        public virtual void Load(Svarog instance) { }
        public virtual void Render(Svarog instance) { }
        public virtual void Frame(Svarog instance) { }
        public virtual void Unload(Svarog instance) { }

        public void Stop()
        {
            Game.OnRegister.RemoveInvocation(Register);
            Game.OnLoad.RemoveInvocation(Load);
            Game.OnRender.RemoveInvocation(Render);
            Game.OnFrame.RemoveInvocation(Frame);
            Game.OnUnload.RemoveInvocation(Unload);
        }

        public PluginExportBuilder Export(string name)
        {
            return new PluginExportBuilder(name);
        }
    }

    public class PluginAttribute : Attribute
    {
        public bool Autoload { get; set; } = true;
        public int Priority { get; set; } = 100;
    }
}
