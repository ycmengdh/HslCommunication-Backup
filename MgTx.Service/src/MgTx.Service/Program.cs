using MgTx.Service.Configuration;
using MgTx.Service.Hubs;
using MgTx.Service.Scripting;
using MgTx.Service.Services;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

var serviceConfig = new ServiceConfig();
builder.Configuration.GetSection("Service").Bind(serviceConfig);

var loggingConfig = new LoggingConfig();
builder.Configuration.GetSection("Logging").Bind(loggingConfig);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Is(loggingConfig.Level.ToLower() switch
    {
        "debug" => LogEventLevel.Debug,
        "warning" => LogEventLevel.Warning,
        "error" => LogEventLevel.Error,
        _ => LogEventLevel.Information
    })
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSignalR();

builder.Services.AddSingleton<IPlcConnectionService, PlcConnectionService>();
builder.Services.AddSingleton<ScriptEngine>();

builder.Services.AddSingleton(serviceConfig);
builder.Services.AddSingleton(loggingConfig);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("*");
    });
});

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

builder.Services.AddHealthChecks();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(serviceConfig.Port);
});

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseCors();

app.UseResponseCompression();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<RealTimeHub>("/hubs/realtime");
app.MapHealthChecks("/health");

Log.Information("========================================");
Log.Information("  MgTx Service - 猛攻通讯");
Log.Information("========================================");
Log.Information("  版本: 1.0.0");
Log.Information("  监听端口: {Port}", serviceConfig.Port);
Log.Information("  REST API: http://localhost:{Port}/api/v1", serviceConfig.Port);
Log.Information("  Web UI: http://localhost:{Port}/", serviceConfig.Port);
Log.Information("  SignalR: http://localhost:{Port}/hubs/realtime", serviceConfig.Port);
Log.Information("========================================");
Log.Information("支持的PLC协议:");
Log.Information("  - Siemens S7 (S7-200/300/400/1200/1500)");
Log.Information("  - Modbus TCP/RTU");
Log.Information("  - Mitsubishi MELSEC MC");
Log.Information("  - Omron FINS");
Log.Information("  - Allen Bradley (EtherNet/IP)");
Log.Information("========================================");

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "服务启动失败");
}
finally
{
    Log.CloseAndFlush();
}
