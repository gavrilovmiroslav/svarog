using svarog_core.Structures;
using System.Reflection;

namespace svarog_core
{
    public class ExternalPluginManager
    {
        FileSystemWatcher watcher;
        System.Timers.Timer timer;
        HashSet<string> waiting = new HashSet<string>();

        public bool IsReady => waiting.Count == 0;

        MultiMap<string, IPlugin> loadedTypes = new();

        private bool importInProgress = false;
        private Svarog svarog;

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
            Console.WriteLine("Watching " + path);

            foreach (var file in Directory.EnumerateFiles("Data//Plugins"))
            {
                if (file.EndsWith(".dll"))
                {
                    waiting.Add(Path.GetFullPath(file));
                }
            }

            timer = new System.Timers.Timer(500);
            timer.Elapsed += Timer_OnImportWaiting;
            timer.AutoReset = true;
            timer.Start();
        }

        private void Timer_OnImportWaiting(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (importInProgress) return;
            importInProgress = true;
            
            if (waiting.Count > 0)
            {
                foreach (var item in waiting)
                {
                    byte[] dll = File.ReadAllBytes(item);
                    var assembly = Assembly.Load(dll);

                    Type type = typeof(IPlugin);
                    IEnumerable<Type> types = assembly.GetTypes()
                        .Where(t => type.IsAssignableFrom(t) && t.GetCustomAttribute<PluginAttribute>() is not null)
                        .OrderBy(t => t.GetCustomAttribute<PluginAttribute>()?.Priority ?? 100);

                    if (loadedTypes.Keys.Contains(item))
                    {
                        foreach (Type t in types)
                        {
                            foreach (var old in loadedTypes[item])
                            {
                                if (PluginManager.IsOverriding(t, "Unload"))
                                {
                                    Game.OnUnload -= old.Unload;
                                    old.Unload(svarog);
                                }

                                if (PluginManager.IsOverriding(t, "Load"))
                                {
                                    Game.OnLoad -= old.Load;
                                }

                                if (PluginManager.IsOverriding(t, "Render"))
                                {
                                    Game.OnRender -= old.Render;
                                }

                                if (PluginManager.IsOverriding(t, "Frame"))
                                {
                                    Game.OnFrame -= old.Frame;
                                }

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

                        var instance = Activator.CreateInstance(t);
                        if (instance is IPlugin p)
                        {
                            loadedTypes.Add(item, p);
                            PluginManager.Instance?.Plugins.Add(t, p);

                            if (PluginManager.IsOverriding(t, "Render"))
                            {
                                Game.OnRender += p.Render;
                            }

                            if (PluginManager.IsOverriding(t, "Frame"))
                            {
                                Game.OnFrame += p.Frame;
                            }

                            if (PluginManager.IsOverriding(t, "Unload"))
                            {
                                Game.OnUnload += p.Unload;
                            }

                            if (PluginManager.IsOverriding(t, "Load"))
                            {
                                Game.OnLoad += p.Load;
                                p.Load(svarog);
                            }
                        }
                    }
                }

                waiting.Clear();
            }

            importInProgress = false;
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
