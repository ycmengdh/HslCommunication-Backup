namespace MgTx.Service.Configuration;

public class ServiceConfig
{
    public string Host { get; set; } = "0.0.0.0";
    public int Port { get; set; } = 8080;
    public bool EnableHttps { get; set; } = false;
}

public class LoggingConfig
{
    public string Level { get; set; } = "Information";
    public bool ConsoleEnabled { get; set; } = true;
    public bool FileEnabled { get; set; } = true;
    public FileLoggingConfig? File { get; set; }
}

public class FileLoggingConfig
{
    public string Path { get; set; } = "./logs/service.log";
    public string RollingInterval { get; set; } = "Day";
}

public class PlcConnectionConfig
{
    public string? Name { get; set; }
    public string DeviceType { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 502;
    public string? CpuType { get; set; }
    public int Rack { get; set; } = 0;
    public int Slot { get; set; } = 1;
    public int ConnectTimeout { get; set; } = 5000;
    public int ReadTimeout { get; set; } = 3000;
    public int WriteTimeout { get; set; } = 3000;
}

public class AppSettings
{
    public ServiceConfig Service { get; set; } = new();
    public LoggingConfig Logging { get; set; } = new();
    public List<PlcConnectionConfig> PlcConnections { get; set; } = new();
}
