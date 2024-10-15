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
                if (instance is IPlugin p)
                {
                    Plugins.Add(t, p);

                    if (IsOverriding(t, "Load"))
                    {
                        Game.OnLoad += p.Load;
                    }

                    if (IsOverriding(t, "Render"))
                    {
                        Game.OnRender += p.Render;
                    }

                    if (IsOverriding(t, "Frame"))
                    {
                        Game.OnFrame += p.Frame;
                    }

                    if (IsOverriding(t, "Unload"))
                    {
                        Game.OnUnload += p.Unload;
                    }
                }
            }
        }
    }
}
