using System.Reflection;
using System.Runtime.Versioning;

namespace ServerLibs.ConfigSystem;
using MultiThreadsSupport;
using LogSystem;

/// <summary>
/// This class is used to manage all configs and their config files,
/// this class enables you to add, delete, modify configs and their config files.
/// The class will update the configs automatically when config files are updated.
/// </summary>
public class ConfigMan
{
    // Single Instance
    private static ConfigMan? _instance;
    private ConfigMan() { }
    public static ConfigMan GetInstance()
    {
        _instance ??= new ConfigMan();
        return _instance;
    }
    
    // Logger
    private readonly Logger _logger = new Logger("ConfigMan", LoggerMan.GetInstance().GetRootLogger());
    
    // Configs
    private readonly Dictionary<string, Type> _configTypes = new Dictionary<string, Type>();
    private readonly Dictionary<string, ConfigVarBase> _configs = new Dictionary<string, ConfigVarBase>();
    private readonly Dictionary<string, string> _configFiles = new Dictionary<string, string>();
    
    /// <summary>
    /// This method adds a config to the config manager.
    /// </summary>
    /// <param name="name">Config Name</param>
    /// <param name="configFilePath">Config File Path</param>
    /// <typeparam name="T">The class of configuration</typeparam>
    public void AddConfig<T>(string name, string configFilePath) where T : new()
    {
        _configTypes.Add(name, typeof(T));
        _configs.Add(name, new ConfigVar<T>(name, new T()));
        _configFiles.Add(name, configFilePath);
    }
    
    /// <summary>
    /// This method gets a config from the config manager.
    /// </summary>
    /// <param name="name">Name of the config</param>
    /// <typeparam name="T">Type of the config</typeparam>
    /// <returns>The config class</returns>
    /// <exception cref="KeyNotFoundException">The config has not been created yet.</exception>
    /// <exception cref="TypeLoadException">The type of the config is different from <typeparamref name="T"/>.</exception>
    public T GetConfig<T>(string name) where T : new()
    {
        if (!_configs.ContainsKey(name))
        {
            throw new KeyNotFoundException($"Config {name} does not exist");
        }
        
        var config = _configs[name];
        if (config is ConfigVar<T> configVar)
        {
            return configVar.Value;
        }
        // The type of config is not T
        throw new TypeLoadException($"Config {name} is not of type {typeof(T)}");
    }
    
    [UnsupportedOSPlatform("ios")] 
    [UnsupportedOSPlatform("macos")] 
    [UnsupportedOSPlatform("tvos")] 
    [UnsupportedOSPlatform("freebsd")]
    private bool UpdateConfig(string name)
    {
        string filePath = _configFiles[name];
        try
        {
            string content = SynchronizedIO.FileRead(filePath);
            Type configType = _configTypes[name];
            var config = _configs[name];
            // We use reflection to call the FromString method of the config
            // Wrap the config in the Generic ConfigVar class
            Type configVarType = typeof(ConfigVar<>).MakeGenericType(configType);
            // The FromString method must exist in the config class
            MethodInfo fromStringMethod = configVarType.GetMethod("FromString")!;
            var result = (bool)fromStringMethod.Invoke(config, new object[] {content})!;
            return result;
        }
        catch (FileNotFoundException e)
        {
            _logger.Error($"Config file {filePath} not found");
            return false;
        }
        catch (DirectoryNotFoundException e)
        {
            _logger.Error($"Path {filePath} not found");
            return false;
        }
    }
    
    [UnsupportedOSPlatform("ios")] 
    [UnsupportedOSPlatform("macos")] 
    [UnsupportedOSPlatform("tvos")] 
    [UnsupportedOSPlatform("freebsd")]
    public List<Task> UpdateTasks()
    {
        var tasks = new List<Task>();
        foreach (var name in _configs.Keys)
        {
            tasks.Add(Task.Run(() =>
            {
                UpdateConfig(name);
            }));
        }

        return tasks;
    }
}