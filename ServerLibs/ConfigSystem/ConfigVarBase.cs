namespace ServerLibs.ConfigSystem;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using LogSystem;

public class ConfigVarBase
{
    protected static readonly Logger Logger = new ("Configs", LogSystem.LoggerMan.GetInstance().GetRootLogger());
    public string Name { get; }
    protected ConfigVarBase(string name) => Name = name;
    
    // Lock for thread safety
    protected readonly ReaderWriterLockSlim LockSlim = new ();
}

public class ConfigVar<T> : ConfigVarBase where T : new()
{
    public ConfigVar(string name, T defaultValue = default(T)) : base(name) => Value = defaultValue ??= new T();

    // Thread-safe Value
    private T _value;
    public T Value
    {
        get
        {
            // Read Lock
            LockSlim.EnterReadLock();
            try
            {
                return _value;
            }
            finally
            {
                LockSlim.ExitReadLock();
            }
        }
        private set
        {
            // Write Lock
            LockSlim.EnterWriteLock();
            try
            {
                _value = value;
            }
            finally
            {
                LockSlim.ExitWriteLock();
            }
        }
    }
    
    public override string ToString()
    {
        if (Value is null) return "";
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var yaml = serializer.Serialize(Value);
        return yaml;
    }

    public bool FromString(string value)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)  // see height_in_inches in sample yml 
            .Build();
        
        try
        {
            Value = deserializer.Deserialize<T>(value);
            return true;
        }catch (YamlDotNet.Core.YamlException e)
        {
            Logger.Error($"Error parsing config {Name}: {e.Message}");
            return false;
        }
    }
}

