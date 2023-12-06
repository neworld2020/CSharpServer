# Config System

The config system contains

+ Management to Config Variables
+ The transformation between variables and config text

We use **YAML** as structured format to arrange config file.

## YAML

Here is an example YAML of configurations.

```yaml
# Key-value pairs
database:
  host: localhost
  port: 5432
  username: myuser
  password: mypassword

logging:
  level: info
  file: /var/log/myapp.log

# List
servers:
  - name: server1
    ip: 192.168.0.101
    port: 8080
  - name: server2
    ip: 192.168.0.102
    port: 8080
  - name: server3
    ip: 192.168.0.103
    port: 8080
```

To parse the file, we should first define a class like follow:

```csharp
class TDatabase
{
    public string Host {get;set;} = "localhost";
    public int Port {get;set;} = 8888;
    public string Username {get;set;} = "user";
    public string Password {get;set;} = "";
}

class TLogging
{
    public string Level {get;set;} = "info";
    public string File {get;set;} = "";
}

class TServer
{
    public string Name {get;set;} = "server";
    public string Ip {get;set;} = "0.0.0.0";
    public int Port {get;set;} = 8888;
}

class WebAppConfigs 
{
    public TDatabase Database{get;set;}
    public TLogging Logging{get;set;}
    public List<TServer> Servers{get;set;}
}

// Then we can parse the YAML like follow
var webAppConfigs = new ConfigVar<WebAppConfigs>("Web App Config");
webAppConfigs.FromString(yaml);
// We can read config values by eg. webAppConfigs.Value.Database.Host
var host = webAppConfigs.Value.Database.Host;
```

## Multi-Thread Security

We add a `ReaderWriterLockSlim` in the getter and setter of each config variable

```csharp
protected readonly ReaderWriterLockSlim LockSlim = new ();
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
```

In this way, mutiple threads can read the config together but only one thread can modify the config at the same time.

## Config Management

There will be several configs in a system, so we need to find a way to manage it.

We design a `ConfigMan` to manage all configs, it can add, get the config. Providing the path of the config file, it can also update the config automatically with reflection in C#.

```csharp
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
```

By using reflection, we can use the according `FromString` method and it can update the config of any types of data.