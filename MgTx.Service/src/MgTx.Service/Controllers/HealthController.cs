using MgTx.Service.Models;
using MgTx.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace MgTx.Service.Controllers;

[ApiController]
[Route("api/v1")]
public class HealthController : ControllerBase
{
    private readonly IPlcConnectionService _plcService;
    private readonly DateTime _startTime = DateTime.UtcNow;

    public HealthController(IPlcConnectionService plcService)
    {
        _plcService = plcService;
    }

    [HttpGet("health")]
    public async Task<ActionResult<ApiResponse<HealthResponse>>> GetHealth()
    {
        var connections = await _plcService.GetConnectionsAsync();
        
        var response = new HealthResponse
        {
            Status = "Healthy",
            Uptime = (DateTime.UtcNow - _startTime).ToString(@"hh\:mm\:ss"),
            Version = "1.0.0",
            ActiveConnections = connections.Count()
        };

        return Ok(ApiResponse<HealthResponse>.Ok(response));
    }
}
