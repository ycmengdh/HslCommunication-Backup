namespace MgTx.Service.Models;

public class PlcConnectRequest
{
    public string DeviceType { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 502;
    public string? CpuType { get; set; }
    public int Rack { get; set; } = 0;
    public int Slot { get; set; } = 1;
    public string? ConnectionName { get; set; }
    public int ConnectTimeout { get; set; } = 5000;
    public int ReadTimeout { get; set; } = 3000;
    public int WriteTimeout { get; set; } = 3000;
}

public class PlcConnectResponse
{
    public string ConnectionId { get; set; } = string.Empty;
    public string? ConnectionName { get; set; }
    public string DeviceType { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Status { get; set; } = "Connected";
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
}

public class PlcReadRequest
{
    public string ConnectionId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DataType { get; set; } = "Float";
    public int Length { get; set; } = 1;
}

public class PlcReadResponse
{
    public string ConnectionId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public object? Values { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class PlcWriteRequest
{
    public string ConnectionId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DataType { get; set; } = "Float";
    public object? Value { get; set; }
    public object? Values { get; set; }
}

public class PlcWriteResponse
{
    public string ConnectionId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public int ValuesWritten { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class PlcStatusResponse
{
    public string ConnectionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public int ResponseTime { get; set; }
    public int ErrorCount { get; set; }
    public string? LastError { get; set; }
}

public class ConnectionInfo
{
    public string ConnectionId { get; set; } = string.Empty;
    public string? ConnectionName { get; set; }
    public string DeviceType { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; }
    public DateTime LastActivity { get; set; }
}
