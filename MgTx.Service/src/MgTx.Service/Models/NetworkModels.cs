namespace MgTx.Service.Models;

public class HealthResponse
{
    public string Status { get; set; } = "Healthy";
    public string Uptime { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public int ActiveConnections { get; set; }
}

public class TcpServerStartRequest
{
    public int Port { get; set; } = 5000;
    public int MaxConnections { get; set; } = 100;
    public int HeartbeatInterval { get; set; } = 30000;
    public int ReceiveBufferSize { get; set; } = 4096;
}

public class TcpServerResponse
{
    public string ServerId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class TcpSendRequest
{
    public string ServerId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public string Encoding { get; set; } = "Base64";
}

public class TcpClientInfo
{
    public string ClientId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; }
    public DateTime ConnectedAt { get; set; }
    public DateTime LastActivity { get; set; }
}
