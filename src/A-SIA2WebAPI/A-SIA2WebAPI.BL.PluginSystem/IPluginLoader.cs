using System.Collections.Generic;

namespace A_SIA2WebAPI.BL.PluginSystem
{
    public interface IPluginLoader<T>
    {
        IEnumerable<T> LoadPlugins(string pluginFolderPath);
    }
}