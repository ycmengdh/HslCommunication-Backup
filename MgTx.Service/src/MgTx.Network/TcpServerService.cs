using MgTx.Core.Net;
using Microsoft.Extensions.Logging;

namespace MgTx.Network;

public class TcpServerService : NetworkServerBase
{
    public event EventHandler<TcpClientConnectedEventArgs>? ClientConnected;
    public event EventHandler<TcpClientDisconnectedEventArgs>? ClientDisconnected;
    public event EventHandler<TcpDataReceivedEventArgs>? DataReceived;

    public TcpServerService(ILogger? logger = null) : base(logger)
    {
    }

    protected override void OnClientConnected(string clientId, System.Net.Sockets.Socket client)
    {
        ClientConnected?.Invoke(this, new TcpClientConnectedEventArgs
        {
            ClientId = clientId,
            IpAddress = client.RemoteEndPoint?.ToString() ?? "Unknown"
        });
    }

    protected override void OnClientDisconnected(string clientId)
    {
        ClientDisconnected?.Invoke(this, new TcpClientDisconnectedEventArgs
        {
            ClientId = clientId
        });
    }

    protected override void OnDataReceived(string clientId, byte[] data)
    {
        DataReceived?.Invoke(this, new TcpDataReceivedEventArgs
        {
            ClientId = clientId,
            Data = data
        });
    }

    public IEnumerable<TcpClientInfo> GetConnectedClients()
    {
        return Clients.Select(kvp => new TcpClientInfo
        {
            ClientId = kvp.Key,
            IpAddress = kvp.Value.RemoteEndPoint?.ToString() ?? "Unknown",
            ConnectedAt = DateTime.UtcNow
        }).ToList();
    }
}

public class TcpClientConnectedEventArgs : EventArgs
{
    public string ClientId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
}

public class TcpClientDisconnectedEventArgs : EventArgs
{
    public string ClientId { get; set; } = string.Empty;
}

public class TcpDataReceivedEventArgs : EventArgs
{
    public string ClientId { get; set; } = string.Empty;
    public byte[] Data { get; set; } = Array.Empty<byte>();
}

public class TcpClientInfo
{
    public string ClientId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; }
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
}
