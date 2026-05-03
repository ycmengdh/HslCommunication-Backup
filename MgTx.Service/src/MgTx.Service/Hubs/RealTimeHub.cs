using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MgTx.Service.Hubs;

public class RealTimeHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> _connectedClients = new();
    private readonly ILogger<RealTimeHub> _logger;

    public RealTimeHub(ILogger<RealTimeHub> logger)
    {
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        var clientId = Context.ConnectionId;
        var clientInfo = Context.GetHttpContext()?.Request.Headers["X-Client-Name"].ToString() ?? "Unknown";
        _connectedClients[clientId] = clientInfo;
        
        _logger.LogInformation("客户端连接: {ClientId} - {ClientInfo}", clientId, clientInfo);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var clientId = Context.ConnectionId;
        _connectedClients.TryRemove(clientId, out _);
        
        _logger.LogInformation("客户端断开: {ClientId}", clientId);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task SubscribeToTag(string tagId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"tag_{tagId}");
        _logger.LogInformation("客户端 {ClientId} 订阅标签: {TagId}", Context.ConnectionId, tagId);
    }

    public async Task UnsubscribeFromTag(string tagId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tag_{tagId}");
        _logger.LogInformation("客户端 {ClientId} 取消订阅标签: {TagId}", Context.ConnectionId, tagId);
    }

    public async Task SubscribeToDevice(string deviceId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"device_{deviceId}");
        _logger.LogInformation("客户端 {ClientId} 订阅设备: {DeviceId}", Context.ConnectionId, deviceId);
    }

    public async Task UnsubscribeFromDevice(string deviceId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"device_{deviceId}");
        _logger.LogInformation("客户端 {ClientId} 取消订阅设备: {DeviceId}", Context.ConnectionId, deviceId);
    }

    public async Task SubscribeToEvents(string eventType = "all")
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"events_{eventType}");
        _logger.LogInformation("客户端 {ClientId} 订阅事件: {EventType}", Context.ConnectionId, eventType);
    }

    public async Task UnsubscribeFromEvents(string eventType = "all")
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"events_{eventType}");
        _logger.LogInformation("客户端 {ClientId} 取消订阅事件: {EventType}", Context.ConnectionId, eventType);
    }

    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }

    public async Task SendPrivateMessage(string userId, string message)
    {
        await Clients.User(userId).SendAsync("ReceivePrivateMessage", message);
    }
}

public static class RealTimeHubExtensions
{
    public static async Task BroadcastTagUpdate(this IHubContext<RealTimeHub> hubContext, 
        string tagId, object value, DateTime timestamp, string quality = "Good")
    {
        var update = new TagUpdate
        {
            TagId = tagId,
            Value = value,
            Timestamp = timestamp,
            Quality = quality
        };
        await hubContext.Clients.Group($"tag_{tagId}").SendAsync("TagUpdated", update);
    }

    public static async Task BroadcastDeviceEvent(this IHubContext<RealTimeHub> hubContext,
        string deviceId, DeviceEvent deviceEvent)
    {
        await hubContext.Clients.Group($"device_{deviceId}").SendAsync("DeviceEvent", deviceEvent);
        await hubContext.Clients.Group("events_all").SendAsync("DeviceEvent", deviceEvent);
    }

    public static async Task BroadcastScriptEvent(this IHubContext<RealTimeHub> hubContext,
        ScriptEvent scriptEvent)
    {
        await hubContext.Clients.Group("events_all").SendAsync("ScriptEvent", scriptEvent);
    }

    public static async Task BroadcastSystemStatus(this IHubContext<RealTimeHub> hubContext,
        SystemStatus status)
    {
        await hubContext.Clients.All.SendAsync("SystemStatus", status);
    }

    public static async Task BroadcastDataChange(this IHubContext<RealTimeHub> hubContext,
        DataChangeEvent dataChange)
    {
        await hubContext.Clients.Group("events_all").SendAsync("DataChange", dataChange);
    }
}

public class TagUpdate
{
    [JsonPropertyName("tagId")]
    public string TagId { get; set; } = string.Empty;
    
    [JsonPropertyName("value")]
    public object Value { get; set; } = new();
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
    
    [JsonPropertyName("quality")]
    public string Quality { get; set; } = "Good";
}

public class DeviceEvent
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;
    
    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = string.Empty;
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
    
    [JsonPropertyName("details")]
    public Dictionary<string, object>? Details { get; set; }
}

public class ScriptEvent
{
    [JsonPropertyName("scriptId")]
    public string ScriptId { get; set; } = string.Empty;
    
    [JsonPropertyName("scriptName")]
    public string ScriptName { get; set; } = string.Empty;
    
    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = string.Empty;
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("result")]
    public object? Result { get; set; }
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

public class SystemStatus
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("connectedDevices")]
    public int ConnectedDevices { get; set; }
    
    [JsonPropertyName("activeTags")]
    public int ActiveTags { get; set; }
    
    [JsonPropertyName("runningScripts")]
    public int RunningScripts { get; set; }
    
    [JsonPropertyName("uptime")]
    public string Uptime { get; set; } = string.Empty;
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

public class DataChangeEvent
{
    [JsonPropertyName("tagId")]
    public string TagId { get; set; } = string.Empty;
    
    [JsonPropertyName("oldValue")]
    public object? OldValue { get; set; }
    
    [JsonPropertyName("newValue")]
    public object? NewValue { get; set; }
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}
