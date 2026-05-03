namespace MgTx.Service.Models;

public class OpcTag
{
    public string TagId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsReadWrite { get; set; } = true;
    public string Description { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public decimal Deadband { get; set; } = 0;
    public decimal EngineeringMin { get; set; } = 0;
    public decimal EngineeringMax { get; set; } = 100;
    public string EngineeringUnits { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsAlarm { get; set; } = false;
    public string AlarmText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class OpcGroup
{
    public string GroupId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int UpdateRate { get; set; } = 1000;
    public bool IsActive { get; set; } = true;
    public List<OpcTag> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class OpcDevice
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 502;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int Timeout { get; set; } = 5000;
    public List<OpcGroup> Groups { get; set; } = new();
    public Dictionary<string, string> Properties { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class OpcServer
{
    public string ServerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string EndpointUrl { get; set; } = string.Empty;
    public string SecurityPolicy { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int ReconnectRate { get; set; } = 5000;
    public List<OpcDevice> Devices { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class TagValueHistory
{
    public string TagId { get; set; } = string.Empty;
    public object Value { get; set; } = new();
    public string Quality { get; set; } = "Good";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
