using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

namespace A_SIA2WebAPI.BL.PluginSystem
{
    public class PluginLoader<T> : IPluginLoader<T>
    {
        private readonly ILogger<PluginLoader<T>> _logger;

        public PluginLoader(ILogger<PluginLoader<T>> logger)
        {
            _logger = logger;
        }

        public IEnumerable<T> LoadPlugins(string pluginFolderPath)
        {
            try
            {
                List<T> plugins = new();

                // Load dlls from folder
                List<string> pluginPaths = Directory.GetFiles(pluginFolderPath).ToList();

                if (pluginPaths.Count == 0)
                {
                    _logger.LogInformation("No Plugin dlls were provided");
                    return Enumerable.Empty<T>();
                }

                foreach (var file in pluginPaths)
                {
                    // Remove non dlls
                    if (!file.Contains(".dll"))
                        pluginPaths.Remove(file);
                }

                // Load plugins and add to plugin list
                plugins.AddRange(pluginPaths.SelectMany(pluginPath =>
                {
                    Assembly pluginAssembly = LoadPlugin(pluginPath);
                    return CreateTypeInstances(pluginAssembly);
                }));

                // Display new plugins
                if (plugins.Count > 0)
                {
                    using (_logger.BeginScope($"Loaded Plugins of type {typeof(T).Name}"))
                    {
                        foreach (var plugin in plugins)
                        {
                            _logger.LogInformation(plugin?.GetType().Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return Enumerable.Empty<T>();
        }

        private IEnumerable<T> CreateTypeInstances(Assembly assembly)
        {
            int count = 0;

            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(T).IsAssignableFrom(type))
                {
                    T? result = (T?)Activator.CreateInstance(type);
                    if (result != null)
                    {
                        count++;
                        yield return result;
                    }
                }
            }

            if (count == 0)
            {
                _logger.LogInformation($"Can't find any type which implements {typeof(T).Name} in {assembly} from {assembly.Location}!");
            }
        }

        private Assembly LoadPlugin(string pluginLocation)
        {
            _logger.LogInformation($"Loaded Plugins from: {Path.GetFileName(pluginLocation)}");
            PluginLoadContext loadContext = new PluginLoadContext(pluginLocation);
            return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
        }
    }
}
