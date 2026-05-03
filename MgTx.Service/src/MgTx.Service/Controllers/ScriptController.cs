using MgTx.Service.Models;
using MgTx.Service.Scripting;
using Microsoft.AspNetCore.Mvc;

namespace MgTx.Service.Controllers;

[ApiController]
[Route("api/v1/scripts")]
public class ScriptController : ControllerBase
{
    private readonly ScriptEngine _scriptEngine;
    private readonly ILogger<ScriptController> _logger;

    public ScriptController(ScriptEngine scriptEngine, ILogger<ScriptController> logger)
    {
        _scriptEngine = scriptEngine;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<ApiResponse<List<ScriptInfo>>> GetScripts()
    {
        var scripts = _scriptEngine.GetScripts();
        return Ok(ApiResponse<List<ScriptInfo>>.Ok(scripts));
    }

    [HttpGet("{scriptId}")]
    public ActionResult<ApiResponse<ScriptInfo?>> GetScript(string scriptId)
    {
        var script = _scriptEngine.GetScript(scriptId);
        if (script == null)
        {
            return NotFound(ApiResponse<ScriptInfo?>.Fail("脚本不存在", 1003));
        }
        return Ok(ApiResponse<ScriptInfo?>.Ok(script));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ScriptInfo>>> CreateScript([FromBody] CreateScriptRequest request)
    {
        try
        {
            var scriptId = Guid.NewGuid().ToString("N");
            _scriptEngine.RegisterScript(scriptId, request.Name, request.Code, request.Trigger);
            
            var script = _scriptEngine.GetScript(scriptId);
            _logger.LogInformation("脚本创建成功: {ScriptId}", scriptId);
            
            return Ok(ApiResponse<ScriptInfo>.Ok(script!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建脚本失败");
            return StatusCode(500, ApiResponse<ScriptInfo>.Fail(ex.Message, 1008));
        }
    }

    [HttpPost("{scriptId}/execute")]
    public async Task<ActionResult<ApiResponse<ScriptResult>>> ExecuteScript(string scriptId, [FromBody] Dictionary<string, object> inputs)
    {
        try
        {
            var script = _scriptEngine.GetScript(scriptId);
            if (script == null)
            {
                return NotFound(ApiResponse<ScriptResult>.Fail("脚本不存在", 1003));
            }

            var context = new ScriptContext
            {
                Inputs = inputs,
                Tags = new Dictionary<string, object>(),
                Outputs = new Dictionary<string, object>(),
                Metadata = new Dictionary<string, object>()
            };

            var result = await _scriptEngine.ExecuteAsync(scriptId, script.Code, context);
            return Ok(ApiResponse<ScriptResult>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行脚本失败: {ScriptId}", scriptId);
            return StatusCode(500, ApiResponse<ScriptResult>.Fail(ex.Message, 1008));
        }
    }

    [HttpDelete("{scriptId}")]
    public async Task<ActionResult<ApiResponse>> DeleteScript(string scriptId)
    {
        try
        {
            _scriptEngine.UnregisterScript(scriptId);
            _logger.LogInformation("脚本删除成功: {ScriptId}", scriptId);
            return Ok(ApiResponse.Ok(new { ScriptId = scriptId }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除脚本失败: {ScriptId}", scriptId);
            return StatusCode(500, ApiResponse.Fail(ex.Message, 1008));
        }
    }
}

public class CreateScriptRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Trigger { get; set; } = string.Empty;
}
