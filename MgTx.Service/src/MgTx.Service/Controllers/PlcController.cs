using MgTx.Service.Models;
using MgTx.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace MgTx.Service.Controllers;

[ApiController]
[Route("api/v1/plc")]
public class PlcController : ControllerBase
{
    private readonly IPlcConnectionService _plcService;
    private readonly ILogger<PlcController> _logger;

    public PlcController(IPlcConnectionService plcService, ILogger<PlcController> logger)
    {
        _plcService = plcService;
        _logger = logger;
    }

    [HttpPost("connect")]
    public async Task<ActionResult<ApiResponse<PlcConnectResponse>>> Connect(
        [FromBody] PlcConnectRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.DeviceType))
            {
                return BadRequest(ApiResponse<PlcConnectResponse>.Fail("设备类型不能为空", 1006));
            }

            if (string.IsNullOrWhiteSpace(request.IpAddress))
            {
                return BadRequest(ApiResponse<PlcConnectResponse>.Fail("IP地址不能为空", 1006));
            }

            var result = await _plcService.ConnectAsync(request, cancellationToken);
            return Ok(ApiResponse<PlcConnectResponse>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PLC 连接失败");
            return StatusCode(500, ApiResponse<PlcConnectResponse>.Fail(ex.Message, 1008));
        }
    }

    [HttpDelete("disconnect")]
    public async Task<ActionResult<ApiResponse>> Disconnect([FromQuery] string connectionId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(connectionId))
            {
                return BadRequest(ApiResponse.Fail("连接ID不能为空", 1006));
            }

            var result = await _plcService.DisconnectAsync(connectionId);
            
            if (!result.Success && result.Error?.Code == 1003)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PLC 断开连接失败");
            return StatusCode(500, ApiResponse.Fail(ex.Message, 1008));
        }
    }

    [HttpDelete("disconnect-all")]
    public async Task<ActionResult<ApiResponse>> DisconnectAll()
    {
        try
        {
            var result = await _plcService.DisconnectAllAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "断开所有 PLC 连接失败");
            return StatusCode(500, ApiResponse.Fail(ex.Message, 1008));
        }
    }

    [HttpGet("connections")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Models.ConnectionInfo>>>> GetConnections()
    {
        try
        {
            var connections = await _plcService.GetConnectionsAsync();
            return Ok(ApiResponse<IEnumerable<Models.ConnectionInfo>>.Ok(connections));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取连接列表失败");
            return StatusCode(500, ApiResponse<IEnumerable<Models.ConnectionInfo>>.Fail(ex.Message, 1008));
        }
    }

    [HttpGet("status/{connectionId}")]
    public async Task<ActionResult<ApiResponse<PlcStatusResponse>>> GetStatus(string connectionId)
    {
        try
        {
            var status = await _plcService.GetStatusAsync(connectionId);
            
            if (status == null)
            {
                return NotFound(ApiResponse<PlcStatusResponse>.Fail("连接不存在", 1003));
            }

            return Ok(ApiResponse<PlcStatusResponse>.Ok(status));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取连接状态失败");
            return StatusCode(500, ApiResponse<PlcStatusResponse>.Fail(ex.Message, 1008));
        }
    }

    [HttpPost("read")]
    public async Task<ActionResult<ApiResponse<PlcReadResponse>>> Read(
        [FromBody] PlcReadRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ConnectionId))
            {
                return BadRequest(ApiResponse<PlcReadResponse>.Fail("连接ID不能为空", 1006));
            }

            if (string.IsNullOrWhiteSpace(request.Address))
            {
                return BadRequest(ApiResponse<PlcReadResponse>.Fail("地址不能为空", 1006));
            }

            var result = await _plcService.ReadAsync(request, cancellationToken);
            
            if (result == null)
            {
                return BadRequest(ApiResponse<PlcReadResponse>.Fail("读取数据失败", 1004));
            }

            return Ok(ApiResponse<PlcReadResponse>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "读取 PLC 数据失败");
            return StatusCode(500, ApiResponse<PlcReadResponse>.Fail(ex.Message, 1008));
        }
    }

    [HttpPost("write")]
    public async Task<ActionResult<ApiResponse<PlcWriteResponse>>> Write(
        [FromBody] PlcWriteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ConnectionId))
            {
                return BadRequest(ApiResponse<PlcWriteResponse>.Fail("连接ID不能为空", 1006));
            }

            if (string.IsNullOrWhiteSpace(request.Address))
            {
                return BadRequest(ApiResponse<PlcWriteResponse>.Fail("地址不能为空", 1006));
            }

            if (request.Value == null && request.Values == null)
            {
                return BadRequest(ApiResponse<PlcWriteResponse>.Fail("写入值不能为空", 1006));
            }

            var result = await _plcService.WriteAsync(request, cancellationToken);
            
            if (result == null)
            {
                return BadRequest(ApiResponse<PlcWriteResponse>.Fail("写入数据失败", 1005));
            }

            return Ok(ApiResponse<PlcWriteResponse>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "写入 PLC 数据失败");
            return StatusCode(500, ApiResponse<PlcWriteResponse>.Fail(ex.Message, 1008));
        }
    }
}
