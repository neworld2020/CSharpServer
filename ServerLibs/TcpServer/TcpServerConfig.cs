namespace ServerLibs.TcpServer;

/// <summary>
/// Config Class for TCP Server
/// </summary>
public class TcpServerConfig
{
    public string Id { get; set; } = "server00";
    public string Type { get; set; } = "http";
    public int BufSize { get; set; } = 1024;
    public int KeepAlive { get; set; } = 0;
    public int Timeout { get; set; } = 7200;
    public string Name { get; set; } = "server";
    public int Ssl { get; set; } = 0;
    public string? CertFile { get; set; } = null;
    public string? KeyFile { get; set; } = null;
    public Address? Address { get; set; } = null;
    public Dictionary<string, string>? Args { get; set; } = null;
}