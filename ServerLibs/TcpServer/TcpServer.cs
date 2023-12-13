using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;

using ServerLibs.LogSystem;

namespace ServerLibs.TcpServer;

public class TcpServer(TcpServerConfig config)
{
    // Name&ID of the Server
    public string Name { get; init; } = config.Name;
    public string Id { get; init; } = config.Id;
    // Buffer Size
    public int BufSize { get; private set; } = config.BufSize;
    // Keep Alive Property of the server
    public bool KeepAlive { get; private set; } = (config.KeepAlive != 0);
    public int Timeout { get; private set; } = config.Timeout;
    // SSL Configurations
    public bool Ssl { get; private set; } = config.Ssl != 0;
    public string? CertFile { get; private set; } = config.CertFile;
    public string? KeyFile { get; private set; } = config.KeyFile;
    // Addresses
    public Address? Address { get; private set; } = config.Address;
    // Other Args
    public Dictionary<string, string>? Args { get; private set; } = config.Args;
    
    // Tcp Sockets: Listen and Client
    protected TcpListener? TcpListenSocket = null;
    protected readonly List<TcpClient> TcpClients = new List<TcpClient>();
    
    // The Server's running status
    protected bool IsRunning = false;
    protected CancellationTokenSource GlobalCts = new CancellationTokenSource();
    
    // Logger
    protected readonly Logger TcpServerLogger = new Logger(config.Name,
        LoggerMan.GetInstance().GetRootLogger());

    /// <summary>
    /// Bind the Address to the Socket
    /// </summary>
    /// <returns>Whether the address has been bound successfully</returns>
    protected virtual bool Bind()
    {
        Address ??= new Address();
        try
        {
            // We use IP Address to Bind the TcpListener
            var ipAddress = IPAddress.Parse(Address.Ip);
            TcpListenSocket = new TcpListener(ipAddress, Address.Port);
        }
        catch (Exception e)
        {
            TcpServerLogger.Error(e.Message);
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Start the server, this method should be used after using Bind()
    /// </summary>
    public virtual async Task Run()
    {
        if (IsRunning) return;
        if (TcpListenSocket is null)
        {
            if (!Bind())
            {
                return;
            }
        }
        
        GlobalCts = new CancellationTokenSource();
        TcpListenSocket.Start();
        IsRunning = true;
        
        while (IsRunning)
        {
            // when there's new connection to accept
            TcpServerLogger.Info("Waiting for Client to Connect");
            var newClient = await TcpListenSocket.AcceptTcpClientAsync(GlobalCts.Token);
            TcpClients.Add(newClient);
            TcpServerLogger.Info("A New Client Connected");
            if (GlobalCts.IsCancellationRequested)
            {
                // Cancelled, stop running
                break;
            }
            await OnAcceptAsync(new TcpStream(newClient.GetStream()), GlobalCts.Token);
            // call OnReceived when receiving data from socket
            _ = HandleClient(newClient);
        }
    }
    
    private async Task HandleClient(TcpClient client)
    {
        // 绑定接收信息事件
        TcpStream? clientStream = null;
        while (true)
        {
            using var timeoutCts = new CancellationTokenSource();
            timeoutCts.CancelAfter(Timeout * 1000);
            // Wrap Stream
            if (Ssl)
            {
                var sslStream = new SslStream(client.GetStream());
                clientStream = new TcpStream(sslStream);
            }
            else
            {
                clientStream = new TcpStream(client.GetStream());
            }
            // Combine TimeoutCts and Global Cts
            var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, 
                GlobalCts.Token);
            try
            {
                await clientStream.WaitAvailable(combinedCts.Token);
                await OnReceivedAsync(clientStream, GlobalCts.Token);
            }
            catch (OperationCanceledException e)
            {
                if (!GlobalCts.IsCancellationRequested)
                {
                    // Cancelled Due to Timeout
                    await OnTimeoutAsync(client, GlobalCts.Token);
                }
            }
            
        }
    }


    /// <summary>
    /// Stop the server
    /// </summary>
    public virtual void Stop()
    {
        if (IsRunning)
        {
            IsRunning = false;
            GlobalCts.Cancel();
            TcpListenSocket!.Stop();
            
            // Close Clients Connections
            foreach (var client in TcpClients)
            {
                client.Close();
            }
            TcpClients.Clear();
            
            // Release GlobalCts
            GlobalCts.Dispose();
        }
    }

    /// <summary>
    /// Things to do when receiving from a socket
    /// </summary>
    /// <param name="stream">the stream for normal TCP and TCP with SSL</param>
    /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
    protected virtual async Task OnReceivedAsync(TcpStream stream, CancellationToken ct) { }

    /// <summary>
    /// Things to do when a socket is cancelled due to timeout
    /// </summary>
    /// <param name="client">The timeout client</param>
    /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
    protected virtual async Task OnTimeoutAsync(TcpClient client, CancellationToken ct) { }

    /// <summary>
    /// Things to do after accepting a new connection
    /// </summary>
    /// <param name="stream">New Connected Client Stream</param>
    /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
    protected virtual async Task OnAcceptAsync(TcpStream stream, CancellationToken ct) { }
}