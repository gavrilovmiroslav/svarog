﻿using svarog.Structures;
using System.Reflection;

namespace svarog
{
    public class PluginManager
    {
        internal static string CurrentlyLoadedPlugin = "";
        private readonly Svarog svarog;
        private readonly HashSet<string> waiting = [];
        private readonly Dictionary<string, int> dllHashes = [];
        private readonly MultiMap<string, IPlugin> loadedPlugins = new();
        
        private FileSystemWatcher watcher;

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

        public bool IsReady => waiting.Count == 0;

        public PluginManager(Svarog svarog)
        {
            this.svarog = svarog;

            var path = Path.GetFullPath("Data//Plugins");
            watcher = new FileSystemWatcher(path);
            watcher.EnableRaisingEvents = true;
            watcher.Filter = "*.dll";
            watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.LastAccess;
            watcher.Changed += Watcher_OnChanged;
            watcher.Created += Watcher_OnCreated;
            watcher.Deleted += Watcher_OnDeleted;
            
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
                List<(int, IPlugin)> newPlugins = [];

                foreach (var item in waiting)
                {
                    if (File.Exists(item))
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

                        if (loadedPlugins.Keys.Contains(item))
                        {
                            foreach (Type t in types)
                            {
                                foreach (var old in loadedPlugins[item])
                                {
                                    if (PluginManager.IsOverriding(t, "Unload"))
                                    {
                                        Game.OnUnload.RemoveInvocation(old.Unload);
                                        old.Unload(svarog);
                                    }

                                    if (PluginManager.IsOverriding(t, "Register"))
                                    {
                                        Game.OnUnload.RemoveInvocation(old.Register);
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

                                    Game.RegisteredFunctions.RemoveAll(t.Name);
                                }

                                loadedPlugins.RemoveAll(item);
                            }
                        }

                        foreach (Type t in types)
                        {
                            if (!t.GetCustomAttribute<PluginAttribute>()?.Autoload ?? false)
                            {
                                continue;
                            }

                            Console.WriteLine($"Checking imports for {item}: {loadedPlugins[item].Count} found.");

                            var priority = t.GetCustomAttribute<PluginAttribute>()?.Priority ?? 100;
                            var instance = Activator.CreateInstance(t);
                            if (instance is IPlugin p)
                            {
                                loadedPlugins.Add(item, p);
                                newPlugins.Add((priority, p));

                                if (PluginManager.IsOverriding(t, "Register"))
                                {
                                    Game.OnRegister.AddInvocation(new RPlugin(t.Name, p.Register, priority));
                                }

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
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var old in loadedPlugins[item])
                        {
                            if (PluginManager.IsOverriding(old.GetType(), "Unload"))
                            {
                                Game.OnUnload.RemoveInvocation(old.Unload);
                                old.Unload(svarog);
                            }

                            if (PluginManager.IsOverriding(old.GetType(), "Register"))
                            {
                                Game.OnUnload.RemoveInvocation(old.Register);
                            }

                            if (PluginManager.IsOverriding(old.GetType(), "Load"))
                            {
                                Game.OnLoad.RemoveInvocation(old.Load);
                            }

                            if (PluginManager.IsOverriding(old.GetType(), "Render"))
                            {
                                Game.OnRender.RemoveInvocation(old.Render);
                            }

                            if (PluginManager.IsOverriding(old.GetType(), "Frame"))
                            {
                                Game.OnFrame.RemoveInvocation(old.Frame);
                            }

                            Game.RegisteredFunctions.RemoveAll(old.GetType().Name);
                        }

                        loadedPlugins.RemoveAll(item);
                    }
                }

                foreach (var p in newPlugins.OrderBy(o => o.Item1))
                {
                    CurrentlyLoadedPlugin = p.Item2.GetType().Name;
                    p.Item2.Register(svarog);
                    CurrentlyLoadedPlugin = "";
                }

                foreach (var p in newPlugins.OrderBy(o => o.Item1))
                {
                    p.Item2.Load(svarog);
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

        private void Watcher_OnDeleted(object sender, FileSystemEventArgs e)
        {
            waiting.Add(e.FullPath);
        }
    }
}
