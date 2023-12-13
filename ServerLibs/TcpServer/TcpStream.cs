using System.Net.Security;
using System.Net.Sockets;

namespace ServerLibs.TcpServer;

public class TcpStream
{
    private readonly SslStream? _sslStream = null;
    private readonly NetworkStream? _networkStream = null;

    private enum StreamType
    {
        Ssl,
        Normal
    };

    private readonly StreamType _type;
    
    public TcpStream(NetworkStream ns)
    {
        _type = StreamType.Normal;
        _networkStream = ns;
    }

    public TcpStream(SslStream sslStream)
    {
        _type = StreamType.Ssl;
        _sslStream = sslStream;
    }

    public Task WaitAvailable(CancellationToken ct)
    {
        return Task.Run(() =>
        {
            while (true)
            {
                if (ct.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }
                if (_type == StreamType.Normal)
                {
                    if (_networkStream!.DataAvailable)
                    {
                        break;
                    }
                }else if (_type == StreamType.Ssl)
                {
                    throw new NotImplementedException("SSL Not Support");
                }
            }
        }, ct);
    }

    public int Read(byte[] buffer, int start, int count)
    {
        return ReadAsync(buffer, start, count).Result;
    }

    public void Write(byte[] buffer, int start, int count)
    {
        WriteAsync(buffer, start, count).Wait();
    }

    public Task<int> ReadAsync(byte[] buffer, int start, int count, CancellationToken ct = default)
    {
        if (_type == StreamType.Normal)
        {
            return _networkStream!.ReadAsync(buffer, start, count, ct);
        }
        else
        {
            // TODO Finish SSL Read Part
            throw new NotImplementedException("SSL Part Not Support Yet");
        }
    }

    public Task WriteAsync(byte[] buffer, int start, int count, CancellationToken ct = default)
    {
        if (_type == StreamType.Normal)
        {
            return _networkStream!.WriteAsync(buffer, start, count, ct);
        }
        else
        {
            // TODO Finish SSL Read Part
            throw new NotImplementedException("SSL Part Not Support Yet");
        }
    }
    
}