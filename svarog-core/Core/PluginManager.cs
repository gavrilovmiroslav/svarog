using System.Reflection;

namespace svarog_core
{
    public class PluginManager
    {
        public static PluginManager? Instance;
        public Dictionary<Type, IPlugin> Plugins = [];

        public static bool IsOverriding(Type t, string name)
        {
            var post = t.GetMethod(name);
            var decl = post?.DeclaringType;
            if (decl != null)
            {
                return decl != typeof(IPlugin);
            }
            else
            {
                return false;
            }
        }

        public PluginManager()
        {
            Instance ??= this;

            Type type = typeof(IPlugin);
            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t => type.IsAssignableFrom(t) && t.GetCustomAttribute<PluginAttribute>() is not null)
                .OrderBy(t => t.GetCustomAttribute<PluginAttribute>()?.Priority ?? 100);

            foreach (Type t in types)
            {
                if (!t.GetCustomAttribute<PluginAttribute>()?.Autoload ?? false)
                {
                    continue;
                }

                var instance = Activator.CreateInstance(t);
                var priority = t.GetCustomAttribute<PluginAttribute>()?.Priority ?? 100;
                if (instance is IPlugin p)
                {
                    Plugins.Add(t, p);

                    if (IsOverriding(t, "Load"))
                    {
                        Game.OnLoad.AddInvocation(new RPlugin(t.Name, p.Load, priority));
                    }

                    if (IsOverriding(t, "Render"))
                    {
                        Game.OnRender.AddInvocation(new RPlugin(t.Name, p.Render, priority));
                    }

                    if (IsOverriding(t, "Frame"))
                    {
                        Game.OnFrame.AddInvocation(new RPlugin(t.Name, p.Frame, priority));
                    }

                    if (IsOverriding(t, "Unload"))
                    {
                        Game.OnUnload.AddInvocation(new RPlugin(t.Name, p.Unload, priority));
                    }
                }
            }
        }
    }
}
