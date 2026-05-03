using Jint;
using Jint.Native;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Microsoft.Extensions.Logging;
using System.Dynamic;
using System.Text.Json;

namespace MgTx.Service.Scripting;

public interface IScriptEngine
{
    Task<ScriptResult> ExecuteScript(string scriptId, string scriptCode, ScriptContext context);
    Task<ScriptResult> ExecuteAsync(string scriptId, string script, ScriptContext context);
    void RegisterScript(string scriptId, string name, string code, string trigger);
    ScriptInfo? GetScript(string scriptId);
    List<ScriptInfo> GetScripts();
    void UnregisterScript(string scriptId);
    void RegisterCallback(string name, Delegate callback);
}

public class ScriptEngine : IScriptEngine
{
    private readonly ILogger<ScriptEngine> _logger;
    private readonly Dictionary<string, ScriptInfo> _registeredScripts = new();
    private readonly Dictionary<string, Delegate> _callbacks = new();
    private readonly object _lock = new();

    public ScriptEngine(ILogger<ScriptEngine> logger)
    {
        _logger = logger;
        RegisterBuiltInCallbacks();
    }

    public void RegisterCallback(string name, Delegate callback)
    {
        lock (_lock)
        {
            _callbacks[name] = callback;
        }
        _logger.LogDebug("注册回调: {Name}", name);
    }

    public void RegisterScript(string scriptId, string name, string code, string trigger)
    {
        lock (_lock)
        {
            _registeredScripts[scriptId] = new ScriptInfo
            {
                ScriptId = scriptId,
                Name = name,
                Code = code,
                Trigger = trigger,
                CreatedAt = DateTime.UtcNow
            };
        }
        _logger.LogInformation("注册脚本: {ScriptId} - {Name}", scriptId, name);
    }

    public ScriptInfo? GetScript(string scriptId)
    {
        lock (_lock)
        {
            return _registeredScripts.TryGetValue(scriptId, out var script) ? script : null;
        }
    }

    public List<ScriptInfo> GetScripts()
    {
        lock (_lock)
        {
            return _registeredScripts.Values.ToList();
        }
    }

    public void UnregisterScript(string scriptId)
    {
        lock (_lock)
        {
            _registeredScripts.Remove(scriptId);
        }
        _logger.LogInformation("注销脚本: {ScriptId}", scriptId);
    }

    public async Task<ScriptResult> ExecuteAsync(string scriptId, string scriptCode, ScriptContext context)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogInformation("执行脚本: {ScriptId}", scriptId);
            
            using var engine = new Engine(options =>
            {
                options.Strict();
                options.TimeoutInterval(TimeSpan.FromSeconds(30));
                options.AllowClr();
                options.LimitMemory(100000000);
            });
            
            engine.SetValue("context", context);
            engine.SetValue("log", new ScriptLogger(_logger));
            engine.SetValue("utils", new ScriptUtils());
            
            foreach (var callback in _callbacks)
            {
                engine.SetValue(callback.Key, callback.Value);
            }

            await Task.Run(() =>
            {
                var result = engine.Execute(scriptCode);
                return result;
            });
            
            var executionTime = DateTime.UtcNow - startTime;
            _logger.LogInformation("脚本执行完成: {ScriptId} - {Duration}ms", scriptId, executionTime.TotalMilliseconds);
            
            return ScriptResult.Ok(executionTime);
        }
        catch (JavaScriptException ex)
        {
            var executionTime = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "JavaScript执行错误: {ScriptId}", scriptId);
            return ScriptResult.Failed($"JavaScript执行错误: {ex.Message}", executionTime);
        }
        catch (Exception ex)
        {
            var executionTime = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "脚本执行错误: {ScriptId}", scriptId);
            return ScriptResult.Failed($"脚本执行错误: {ex.Message}", executionTime);
        }
    }

    public Task<ScriptResult> ExecuteScript(string scriptId, string scriptCode, ScriptContext context)
    {
        return ExecuteAsync(scriptId, scriptCode, context);
    }

    private void RegisterBuiltInCallbacks()
    {
        RegisterCallback("setTimeout", new Func<Action, int, int>((action, timeout) =>
        {
            Task.Delay(timeout).ContinueWith(_ => action());
            return 0;
        }));
        
        RegisterCallback("now", new Func<DateTime>(() => DateTime.UtcNow));
        
        RegisterCallback("format", new Func<string, object[], string>((fmt, args) => string.Format(fmt, args)));
    }
}

public class ScriptLogger
{
    private readonly ILogger _logger;

    public ScriptLogger(ILogger logger)
    {
        _logger = logger;
    }

    public void Info(string message) => _logger.LogInformation("[脚本] {Message}", message);
    public void Warn(string message) => _logger.LogWarning("[脚本] {Message}", message);
    public void Error(string message) => _logger.LogError("[脚本] {Message}", message);
    public void Debug(string message) => _logger.LogDebug("[脚本] {Message}", message);
}

public class ScriptUtils
{
    public int Round(double value, int decimals = 0)
    {
        return (int)Math.Round(value, decimals);
    }

    public double Min(params double[] values)
    {
        return values.Min();
    }

    public double Max(params double[] values)
    {
        return values.Max();
    }

    public double Avg(params double[] values)
    {
        return values.Average();
    }

    public double Sum(params double[] values)
    {
        return values.Sum();
    }

    public string Base64Encode(string text)
    {
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(text));
    }

    public string Base64Decode(string encodedText)
    {
        return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedText));
    }

    public string ToHex(byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", "");
    }

    public byte[] FromHex(string hex)
    {
        return Convert.FromHexString(hex);
    }
}

public class ScriptContext
{
    public Dictionary<string, object> Tags { get; set; } = new();
    
    public Dictionary<string, object> Inputs { get; set; } = new();
    
    public Dictionary<string, object> Outputs { get; set; } = new();
    
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    public dynamic Data { get; set; } = new ExpandoObject();
}

public class ScriptInfo
{
    public string ScriptId { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    
    public string Code { get; set; } = string.Empty;
    
    public string Trigger { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? LastExecutedAt { get; set; }
    
    public int ExecutionCount { get; set; }
}

public class ScriptResult
{
    public bool Success { get; set; }
    
    public string? Error { get; set; }
    
    public TimeSpan ExecutionTime { get; set; }
    
    public object? Result { get; set; }
    
    public static ScriptResult Ok(TimeSpan executionTime)
    {
        return new ScriptResult
        {
            Success = true,
            ExecutionTime = executionTime
        };
    }
    
    public static ScriptResult Failed(string error, TimeSpan executionTime)
    {
        return new ScriptResult
        {
            Success = false,
            Error = error,
            ExecutionTime = executionTime
        };
    }
}
