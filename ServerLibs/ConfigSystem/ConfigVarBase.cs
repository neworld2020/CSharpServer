namespace ServerLibs.ConfigSystem;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using LogSystem;

public class ConfigVarBase
{
    protected static readonly Logger Logger = new Logger("Configs", LogSystem.LoggerMan.GetInstance().GetRootLogger());
    public string Name { get; init; }
    protected ConfigVarBase(string name) => Name = name;
}

public class ConfigVar<T> : ConfigVarBase
{
    public ConfigVar(string name, T defaultValue = default(T)) : base(name) => Value = defaultValue;
    public T? Value { get; private set; }
    public override string? ToString()
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

