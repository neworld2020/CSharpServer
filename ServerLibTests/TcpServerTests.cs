using System.Net;
using System.Net.Sockets;
using ServerLibs.LogSystem;

namespace ServerLibTests;
using ServerLibs.TcpServer;

public class TcpServerTests
{
    private class EchoTcpServer(TcpServerConfig config) : TcpServer(config)
    {
        protected override async Task OnReceivedAsync(TcpStream stream, CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return;
             // Send back
             var bytes = new byte[1024];
             var bytesRead = await stream.ReadAsync(bytes, 0, 1024, ct);
             await stream.WriteAsync(bytes, 0, bytesRead, ct);
        }
    }

    private static TcpServerConfig _config = new TcpServerConfig
    {
        Address = new Address
        {
            Ip = "127.0.0.1",
            Port = 1234
        }
    };

    private readonly EchoTcpServer _server = new EchoTcpServer(_config);
    private readonly TcpClient _client = new TcpClient(AddressFamily.InterNetwork);
    private readonly Logger _logger = new Logger("TcpServerTest", 
                                                 LoggerMan.GetInstance().GetRootLogger());
    
    [SetUp]
    public void SetUp()
    {
    }

    [Test]
    public void EchoTcpServerTests()
    {
        _ = _server.Run();
        
        _client.Connect(IPAddress.Parse(_config.Address.Ip), _config.Address.Port);
        byte[] buffer = new byte[1024];
        
        _client.GetStream().Write("hello"u8);
        var bytesRead = _client.GetStream().Read(buffer, 0, 1024);
        _logger.Info(System.Text.Encoding.ASCII.GetString(buffer.AsSpan(0, bytesRead)));
        _client.GetStream().Write("world"u8);
        bytesRead = _client.GetStream().Read(buffer, 0, 1024);
        _logger.Info(System.Text.Encoding.ASCII.GetString(buffer.AsSpan(0, bytesRead)));
        
        _server.Stop();
    }
}