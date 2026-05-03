using MgTx.Service.Models;
using Microsoft.AspNetCore.Mvc;

namespace MgTx.Service.Controllers;

[ApiController]
[Route("api/v1/opc")]
public class OpcController : ControllerBase
{
    private readonly ILogger<OpcController> _logger;
    private static readonly List<OpcServer> _servers = new();

    public OpcController(ILogger<OpcController> logger)
    {
        _logger = logger;
    }

    [HttpGet("servers")]
    public ActionResult<ApiResponse<List<OpcServer>>> GetServers()
    {
        return Ok(ApiResponse<List<OpcServer>>.Ok(_servers));
    }

    [HttpGet("servers/{serverId}")]
    public ActionResult<ApiResponse<OpcServer?>> GetServer(string serverId)
    {
        var server = _servers.FirstOrDefault(s => s.ServerId == serverId);
        if (server == null)
        {
            return NotFound(ApiResponse<OpcServer?>.Fail("服务器不存在", 1003));
        }
        return Ok(ApiResponse<OpcServer?>.Ok(server));
    }

    [HttpPost("servers")]
    public async Task<ActionResult<ApiResponse<OpcServer>>> CreateServer([FromBody] CreateOpcServerRequest request)
    {
        try
        {
            var serverId = Guid.NewGuid().ToString("N");
            var server = new OpcServer
            {
                ServerId = serverId,
                Name = request.Name,
                EndpointUrl = request.EndpointUrl,
                SecurityPolicy = request.SecurityPolicy,
                UserName = request.UserName,
                Password = request.Password,
                IsActive = true,
                ReconnectRate = request.ReconnectRate,
                Devices = new List<OpcDevice>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            _servers.Add(server);
            _logger.LogInformation("OPC服务器创建成功: {ServerId}", serverId);
            
            return Ok(ApiResponse<OpcServer>.Ok(server));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建OPC服务器失败");
            return StatusCode(500, ApiResponse<OpcServer>.Fail(ex.Message, 1008));
        }
    }

    [HttpPost("servers/{serverId}/connect")]
    public async Task<ActionResult<ApiResponse>> ConnectServer(string serverId)
    {
        var server = _servers.FirstOrDefault(s => s.ServerId == serverId);
        if (server == null)
        {
            return NotFound(ApiResponse.Fail("服务器不存在", 1003));
        }
        
        _logger.LogInformation("尝试连接OPC服务器: {ServerId}", serverId);
        return Ok(ApiResponse.Ok(new { ServerId = serverId, Status = "Connecting" }));
    }

    [HttpPost("servers/{serverId}/disconnect")]
    public async Task<ActionResult<ApiResponse>> DisconnectServer(string serverId)
    {
        var server = _servers.FirstOrDefault(s => s.ServerId == serverId);
        if (server == null)
        {
            return NotFound(ApiResponse.Fail("服务器不存在", 1003));
        }
        
        _logger.LogInformation("断开OPC服务器连接: {ServerId}", serverId);
        return Ok(ApiResponse.Ok(new { ServerId = serverId, Status = "Disconnected" }));
    }

    [HttpDelete("servers/{serverId}")]
    public async Task<ActionResult<ApiResponse>> DeleteServer(string serverId)
    {
        var server = _servers.FirstOrDefault(s => s.ServerId == serverId);
        if (server == null)
        {
            return NotFound(ApiResponse.Fail("服务器不存在", 1003));
        }
        
        _servers.Remove(server);
        _logger.LogInformation("OPC服务器删除成功: {ServerId}", serverId);
        return Ok(ApiResponse.Ok(new { ServerId = serverId }));
    }

    [HttpPost("servers/{serverId}/devices")]
    public async Task<ActionResult<ApiResponse<OpcDevice>>> AddDevice(string serverId, [FromBody] CreateOpcDeviceRequest request)
    {
        var server = _servers.FirstOrDefault(s => s.ServerId == serverId);
        if (server == null)
        {
            return NotFound(ApiResponse<OpcDevice>.Fail("服务器不存在", 1003));
        }
        
        var deviceId = Guid.NewGuid().ToString("N");
        var device = new OpcDevice
        {
            DeviceId = deviceId,
            DeviceType = request.DeviceType,
            Name = request.Name,
            IpAddress = request.IpAddress,
            Port = request.Port,
            Description = request.Description,
            IsActive = true,
            Groups = new List<OpcGroup>(),
            Properties = new Dictionary<string, string>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        server.Devices.Add(device);
        return Ok(ApiResponse<OpcDevice>.Ok(device));
    }

    [HttpPost("servers/{serverId}/devices/{deviceId}/groups")]
    public async Task<ActionResult<ApiResponse<OpcGroup>>> AddGroup(string serverId, string deviceId, [FromBody] CreateOpcGroupRequest request)
    {
        var server = _servers.FirstOrDefault(s => s.ServerId == serverId);
        var device = server?.Devices.FirstOrDefault(d => d.DeviceId == deviceId);
        if (device == null)
        {
            return NotFound(ApiResponse<OpcGroup>.Fail("设备不存在", 1003));
        }
        
        var groupId = Guid.NewGuid().ToString("N");
        var group = new OpcGroup
        {
            GroupId = groupId,
            Name = request.Name,
            Description = request.Description,
            UpdateRate = request.UpdateRate,
            IsActive = true,
            Tags = new List<OpcTag>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        device.Groups.Add(group);
        return Ok(ApiResponse<OpcGroup>.Ok(group));
    }

    [HttpPost("servers/{serverId}/devices/{deviceId}/groups/{groupId}/tags")]
    public async Task<ActionResult<ApiResponse<OpcTag>>> AddTag(string serverId, string deviceId, string groupId, [FromBody] CreateOpcTagRequest request)
    {
        var server = _servers.FirstOrDefault(s => s.ServerId == serverId);
        var device = server?.Devices.FirstOrDefault(d => d.DeviceId == deviceId);
        var group = device?.Groups.FirstOrDefault(g => g.GroupId == groupId);
        if (group == null)
        {
            return NotFound(ApiResponse<OpcTag>.Fail("组不存在", 1003));
        }
        
        var tagId = Guid.NewGuid().ToString("N");
        var tag = new OpcTag
        {
            TagId = tagId,
            Name = request.Name,
            Address = request.Address,
            DataType = request.DataType,
            IsReadWrite = request.IsReadWrite,
            Description = request.Description,
            GroupId = groupId,
            Deadband = request.Deadband,
            EngineeringMin = request.EngineeringMin,
            EngineeringMax = request.EngineeringMax,
            EngineeringUnits = request.EngineeringUnits,
            IsActive = true,
            IsAlarm = request.IsAlarm,
            AlarmText = request.AlarmText,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        group.Tags.Add(tag);
        return Ok(ApiResponse<OpcTag>.Ok(tag));
    }
}

public class CreateOpcServerRequest
{
    public string Name { get; set; } = string.Empty;
    public string EndpointUrl { get; set; } = string.Empty;
    public string SecurityPolicy { get; set; } = "None";
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int ReconnectRate { get; set; } = 5000;
}

public class CreateOpcDeviceRequest
{
    public string DeviceType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 502;
    public string Description { get; set; } = string.Empty;
}

public class CreateOpcGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int UpdateRate { get; set; } = 1000;
}

public class CreateOpcTagRequest
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DataType { get; set; } = "Float";
    public bool IsReadWrite { get; set; } = true;
    public string Description { get; set; } = string.Empty;
    public decimal Deadband { get; set; } = 0;
    public decimal EngineeringMin { get; set; } = 0;
    public decimal EngineeringMax { get; set; } = 100;
    public string EngineeringUnits { get; set; } = string.Empty;
    public bool IsAlarm { get; set; } = false;
    public string AlarmText { get; set; } = string.Empty;
}
