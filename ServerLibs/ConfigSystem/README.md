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

TODO