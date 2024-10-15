using svarog_core.Structures;
using System.Reflection;

namespace svarog_core
{
    public class ExternalPluginManager
    {
        private readonly Svarog svarog;
        private readonly HashSet<string> waiting = [];
        private readonly Dictionary<string, int> dllHashes = [];
        private readonly MultiMap<string, IPlugin> loadedTypes = new();

        private FileSystemWatcher watcher;

        public bool IsReady => waiting.Count == 0;

        public ExternalPluginManager(Svarog svarog)
        {
            this.svarog = svarog;

            var path = Path.GetFullPath("Data//Plugins");
            watcher = new FileSystemWatcher(path);
            watcher.EnableRaisingEvents = true;
            watcher.Filter = "*.dll";
            watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.Size;
            watcher.Changed += Watcher_OnChanged;
            watcher.Created += Watcher_OnCreated;

            foreach (var file in Directory.EnumerateFiles("Data//Plugins"))
            {
                if (file.EndsWith(".dll"))
                {
                    waiting.Add(Path.GetFullPath(file));
                }
            }

            Update();
        }

        internal void Update()
        {            
            if (waiting.Count > 0)
            {
                foreach (var item in waiting)
                {
                    byte[] dll = File.ReadAllBytes(item);
                    var hash = dll.GetHashCode();
                    if (dllHashes.TryGetValue(item, out int value) && value == hash)
                    {
                        continue;
                    }

                    dllHashes[item] = hash;

                    var assembly = Assembly.Load(dll);

                    Type type = typeof(IPlugin);
                    IEnumerable<Type> types = assembly.GetTypes()
                        .Where(t => type.IsAssignableFrom(t) && t.GetCustomAttribute<PluginAttribute>() is not null)
                        .OrderBy(t => t.GetCustomAttribute<PluginAttribute>()?.Priority ?? 100);

                    if (loadedTypes.Keys.Contains(item))
                    {
                        foreach (Type t in types)
                        {
                            List<IPlugin> toRemove = [];
                            foreach (var old in loadedTypes[item])
                            {
                                if (PluginManager.IsOverriding(t, "Unload"))
                                {
                                    Game.OnUnload.RemoveInvocation(old.Unload);
                                    old.Unload(svarog);
                                }

                                if (PluginManager.IsOverriding(t, "Load"))
                                {
                                    Game.OnLoad.RemoveInvocation(old.Load);
                                }

                                if (PluginManager.IsOverriding(t, "Render"))
                                {
                                    Game.OnRender.RemoveInvocation(old.Render);
                                }

                                if (PluginManager.IsOverriding(t, "Frame"))
                                {
                                    Game.OnFrame.RemoveInvocation(old.Frame);
                                }

                                toRemove.Add(old);
                            }

                            foreach (var old in toRemove)
                            {
                                loadedTypes.Remove(item, old);
                            }
                        }
                    }

                    foreach (Type t in types)
                    {
                        if (!t.GetCustomAttribute<PluginAttribute>()?.Autoload ?? false)
                        {
                            continue;
                        }

                        Console.WriteLine($"Checking imports for {item}: {loadedTypes[item].Count} found.");

                        var priority = t.GetCustomAttribute<PluginAttribute>()?.Priority ?? 100;
                        var instance = Activator.CreateInstance(t);
                        if (instance is IPlugin p)
                        {
                            loadedTypes.Add(item, p);
                            PluginManager.Instance?.Plugins.Add(t, p);

                            if (PluginManager.IsOverriding(t, "Render"))
                            {
                                Game.OnRender.AddInvocation(new RPlugin(t.Name, p.Render, priority));
                            }

                            if (PluginManager.IsOverriding(t, "Frame"))
                            {
                                Game.OnFrame.AddInvocation(new RPlugin(t.Name, p.Frame, priority));
                            }

                            if (PluginManager.IsOverriding(t, "Unload"))
                            {
                                Game.OnUnload.AddInvocation(new RPlugin(t.Name, p.Unload, priority));
                            }

                            if (PluginManager.IsOverriding(t, "Load"))
                            {
                                Game.OnLoad.AddInvocation(new RPlugin(t.Name, p.Load, priority));
                                p.Load(svarog);
                            }
                        }
                    }
                }

                waiting.Clear();
            }
        }

        private void Watcher_OnCreated(object sender, FileSystemEventArgs e)
        {
            waiting.Add(e.FullPath);
        }

        private void Watcher_OnChanged(object sender, FileSystemEventArgs e)
        {
            waiting.Add(e.FullPath);
        }
    }
}
