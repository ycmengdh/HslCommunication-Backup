using MgTx.Network;
using MgTx.Service.Models;
using Microsoft.AspNetCore.Mvc;

namespace MgTx.Service.Controllers;

[ApiController]
[Route("api/v1/network")]
public class NetworkController : ControllerBase
{
    private readonly TcpServerService _tcpServer;
    private readonly ILogger<NetworkController> _logger;

    public NetworkController(ILogger<NetworkController> logger)
    {
        _logger = logger;
        _tcpServer = new TcpServerService(_logger as ILogger<TcpServerService>);
    }

    [HttpPost("tcp/start")]
    public async Task<ActionResult<ApiResponse<TcpServerResponse>>> StartTcpServer(
        [FromBody] TcpServerStartRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (_tcpServer.IsRunning)
            {
                return Conflict(ApiResponse<TcpServerResponse>.Fail("TCP 服务器已在运行", 1002));
            }

            _tcpServer.MaxConnections = request.MaxConnections;
            _tcpServer.ReceiveBufferSize = request.ReceiveBufferSize;

            var result = await _tcpServer.StartAsync(request.Port, cancellationToken);
            
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<TcpServerResponse>.Fail(result.Message!, 1008));
            }

            var response = new TcpServerResponse
            {
                ServerId = "tcp_server_001",
                Type = "TcpServer",
                Port = request.Port,
                Status = "Running"
            };

            _logger.LogInformation("TCP 服务器已启动: {Port}", request.Port);
            return Ok(ApiResponse<TcpServerResponse>.Ok(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动 TCP 服务器失败");
            return StatusCode(500, ApiResponse<TcpServerResponse>.Fail(ex.Message, 1008));
        }
    }

    [HttpPost("tcp/stop")]
    public ActionResult<ApiResponse<TcpServerResponse>> StopTcpServer([FromQuery] string serverId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serverId))
            {
                return BadRequest(ApiResponse<TcpServerResponse>.Fail("服务器ID不能为空", 1006));
            }

            _tcpServer.Stop();

            var response = new TcpServerResponse
            {
                ServerId = serverId,
                Type = "TcpServer",
                Port = _tcpServer.Port,
                Status = "Stopped"
            };

            _logger.LogInformation("TCP 服务器已停止: {ServerId}", serverId);
            return Ok(ApiResponse<TcpServerResponse>.Ok(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "停止 TCP 服务器失败");
            return StatusCode(500, ApiResponse<TcpServerResponse>.Fail(ex.Message, 1008));
        }
    }

    [HttpPost("tcp/send")]
    public async Task<ActionResult<ApiResponse>> SendToTcpClient(
        [FromBody] TcpSendRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ServerId) || string.IsNullOrWhiteSpace(request.ClientId))
            {
                return BadRequest(ApiResponse.Fail("服务器ID和客户端ID不能为空", 1006));
            }

            byte[] data = request.Encoding.ToLower() switch
            {
                "utf8" => System.Text.Encoding.UTF8.GetBytes(request.Data),
                "hex" => Convert.FromHexString(request.Data),
                _ => Convert.FromBase64String(request.Data)
            };

            var result = await _tcpServer.SendToClientAsync(request.ClientId, data, cancellationToken);
            
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse.Fail(result.Message!, 1005));
            }

            return Ok(ApiResponse.Ok(new { BytesSent = data.Length, Timestamp = DateTime.UtcNow }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送 TCP 数据失败");
            return StatusCode(500, ApiResponse.Fail(ex.Message, 1008));
        }
    }

    [HttpGet("tcp/clients")]
    public ActionResult<ApiResponse<IEnumerable<MgTx.Network.TcpClientInfo>>> GetTcpClients([FromQuery] string serverId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serverId))
            {
                return BadRequest(ApiResponse<IEnumerable<MgTx.Network.TcpClientInfo>>.Fail("服务器ID不能为空", 1006));
            }

            var clients = _tcpServer.GetConnectedClients().Select(c => new MgTx.Network.TcpClientInfo
            {
                ClientId = c.ClientId,
                IpAddress = c.IpAddress,
                ConnectedAt = c.ConnectedAt,
                LastActivity = c.LastActivity
            });

            return Ok(ApiResponse<IEnumerable<MgTx.Network.TcpClientInfo>>.Ok(clients));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取 TCP 客户端列表失败");
            return StatusCode(500, ApiResponse<IEnumerable<MgTx.Network.TcpClientInfo>>.Fail(ex.Message, 1008));
        }
    }
}
