namespace ServerLibs.TcpServer;

/// <summary>
/// Aggregate Class for Network Address: IPv4, IPv6
/// </summary>
public class Address
{
    public string Ip { get; set; } = "127.0.0.1";
    public string? Host { get; set; } = null;
    public int Port { get; set; } = 80;
}