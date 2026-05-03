using MgTx.Core.Transfer;
using MgTx.Core.Types;
using MgTx.Plc;
using MgTx.Plc.Modbus;
using MgTx.Plc.Melsec;
using MgTx.Plc.Omron;
using MgTx.Plc.Siemens;
using MgTx.Service.Models;
using Microsoft.Extensions.Logging;

namespace MgTx.Service.Services;

public interface IPlcConnectionService
{
    Task<PlcConnectResponse> ConnectAsync(PlcConnectRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> DisconnectAsync(string connectionId);
    Task<ApiResponse> DisconnectAllAsync();
    Task<IEnumerable<ConnectionInfo>> GetConnectionsAsync();
    Task<PlcStatusResponse?> GetStatusAsync(string connectionId);
    Task<PlcReadResponse?> ReadAsync(PlcReadRequest request, CancellationToken cancellationToken = default);
    Task<PlcWriteResponse?> WriteAsync(PlcWriteRequest request, CancellationToken cancellationToken = default);
}

public class PlcConnectionService : IPlcConnectionService
{
    private readonly Dictionary<string, PlcConnection> _connections = new();
    private readonly object _lock = new();
    private readonly ILogger<PlcConnectionService> _logger;

    public PlcConnectionService(ILogger<PlcConnectionService> logger)
    {
        _logger = logger;
    }

    public async Task<PlcConnectResponse> ConnectAsync(PlcConnectRequest request, CancellationToken cancellationToken = default)
    {
        var connectionId = $"conn_{Guid.NewGuid():N}";
        
        _logger.LogInformation("正在建立 PLC 连接: {DeviceType} - {IpAddress}:{Port}", 
            request.DeviceType, request.IpAddress, request.Port);

        try
        {
            object? device = request.DeviceType.ToLower() switch
            {
                "siemenss7" => CreateSiemensDevice(request),
                "modbustcp" => CreateModbusDevice(request),
                "melsecmc" => CreateMelsecDevice(request),
                "omronfins" => CreateOmronDevice(request),
                _ => null
            };

            if (device == null)
            {
                _logger.LogError("不支持的设备类型: {DeviceType}", request.DeviceType);
                throw new InvalidOperationException($"不支持的设备类型: {request.DeviceType}");
            }

            var connectMethod = device.GetType().GetMethod("ConnectAsync");
            if (connectMethod == null)
            {
                throw new InvalidOperationException("设备对象缺少 ConnectAsync 方法");
            }

            var task = (Task)connectMethod.Invoke(device, new object[] { 
                request.IpAddress, 
                request.Port, 
                cancellationToken 
            })!;
            await task.ConfigureAwait(false);

            var connection = new PlcConnection
            {
                ConnectionId = connectionId,
                ConnectionName = request.ConnectionName ?? connectionId,
                DeviceType = request.DeviceType,
                IpAddress = request.IpAddress,
                Port = request.Port,
                Device = device,
                Status = "Connected",
                ConnectedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow
            };

            lock (_lock)
            {
                _connections[connectionId] = connection;
            }

            _logger.LogInformation("PLC 连接成功: {ConnectionId}", connectionId);

            return new PlcConnectResponse
            {
                ConnectionId = connectionId,
                ConnectionName = connection.ConnectionName,
                DeviceType = request.DeviceType,
                IpAddress = request.IpAddress,
                Port = request.Port,
                Status = "Connected",
                ConnectedAt = connection.ConnectedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PLC 连接失败: {DeviceType} - {IpAddress}:{Port}", 
                request.DeviceType, request.IpAddress, request.Port);
            throw;
        }
    }

    public async Task<ApiResponse> DisconnectAsync(string connectionId)
    {
        PlcConnection? connection;
        lock (_lock)
        {
            if (!_connections.TryGetValue(connectionId, out connection))
            {
                return ApiResponse.Fail("连接不存在", 1003);
            }
        }

        try
        {
            if (connection.Device is IDisposable disposable)
            {
                disposable.Dispose();
            }

            lock (_lock)
            {
                _connections.Remove(connectionId);
            }

            _logger.LogInformation("PLC 连接已断开: {ConnectionId}", connectionId);

            return ApiResponse.Ok(new { ConnectionId = connectionId, Status = "Disconnected" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "断开连接失败: {ConnectionId}", connectionId);
            return ApiResponse.Fail($"断开连接失败: {ex.Message}", 1008);
        }
    }

    public async Task<ApiResponse> DisconnectAllAsync()
    {
        int count;
        lock (_lock)
        {
            count = _connections.Count;
            foreach (var connection in _connections.Values)
            {
                if (connection.Device is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _connections.Clear();
        }

        _logger.LogInformation("已断开所有 PLC 连接，共 {Count} 个", count);

        await Task.CompletedTask;
        return ApiResponse.Ok(new { DisconnectedCount = count });
    }

    public Task<IEnumerable<ConnectionInfo>> GetConnectionsAsync()
    {
        List<ConnectionInfo> connections;
        lock (_lock)
        {
            connections = _connections.Values.Select(c => new ConnectionInfo
            {
                ConnectionId = c.ConnectionId,
                ConnectionName = c.ConnectionName,
                DeviceType = c.DeviceType,
                IpAddress = c.IpAddress,
                Port = c.Port,
                Status = c.Status,
                ConnectedAt = c.ConnectedAt,
                LastActivity = c.LastActivity
            }).ToList();
        }

        return Task.FromResult<IEnumerable<ConnectionInfo>>(connections);
    }

    public Task<PlcStatusResponse?> GetStatusAsync(string connectionId)
    {
        lock (_lock)
        {
            if (!_connections.TryGetValue(connectionId, out var connection))
            {
                return Task.FromResult<PlcStatusResponse?>(null);
            }

            return Task.FromResult<PlcStatusResponse?>(new PlcStatusResponse
            {
                ConnectionId = connectionId,
                Status = connection.Status,
                IsOnline = connection.Status == "Connected",
                ErrorCount = connection.ErrorCount,
                LastError = connection.LastError
            });
        }
    }

    public async Task<PlcReadResponse?> ReadAsync(PlcReadRequest request, CancellationToken cancellationToken = default)
    {
        PlcConnection? connection;
        lock (_lock)
        {
            if (!_connections.TryGetValue(connectionId: request.ConnectionId, out connection))
            {
                return null;
            }
        }

        try
        {
            connection.LastActivity = DateTime.UtcNow;
            var result = await ReadFromDeviceAsync(connection.Device, request, cancellationToken);
            
            if (result.IsSuccess)
            {
                return new PlcReadResponse
                {
                    ConnectionId = request.ConnectionId,
                    Address = request.Address,
                    DataType = request.DataType,
                    Values = result.Content,
                    Timestamp = DateTime.UtcNow
                };
            }
            else
            {
                connection.ErrorCount++;
                connection.LastError = result.Message;
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "读取 PLC 数据失败: {ConnectionId} - {Address}", 
                request.ConnectionId, request.Address);
            connection.ErrorCount++;
            connection.LastError = ex.Message;
            return null;
        }
    }

    public async Task<PlcWriteResponse?> WriteAsync(PlcWriteRequest request, CancellationToken cancellationToken = default)
    {
        PlcConnection? connection;
        lock (_lock)
        {
            if (!_connections.TryGetValue(connectionId: request.ConnectionId, out connection))
            {
                return null;
            }
        }

        try
        {
            connection.LastActivity = DateTime.UtcNow;
            var result = await WriteToDeviceAsync(connection.Device, request, cancellationToken);
            
            if (result.IsSuccess)
            {
                return new PlcWriteResponse
                {
                    ConnectionId = request.ConnectionId,
                    Address = request.Address,
                    DataType = request.DataType,
                    ValuesWritten = 1,
                    Timestamp = DateTime.UtcNow
                };
            }
            else
            {
                connection.ErrorCount++;
                connection.LastError = result.Message;
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "写入 PLC 数据失败: {ConnectionId} - {Address}", 
                request.ConnectionId, request.Address);
            connection.ErrorCount++;
            connection.LastError = ex.Message;
            return null;
        }
    }

    private SiemensS7Net CreateSiemensDevice(PlcConnectRequest request)
    {
        var device = new SiemensS7Net(_logger as ILogger<SiemensS7Net>);
        device.Rack = request.Rack;
        device.Slot = request.Slot;
        device.ConnectTimeout = request.ConnectTimeout;
        device.ReceiveTimeout = request.ReadTimeout;
        device.SendTimeout = request.WriteTimeout;
        return device;
    }

    private ModbusTcpNet CreateModbusDevice(PlcConnectRequest request)
    {
        var device = new ModbusTcpNet(_logger as ILogger<ModbusTcpNet>);
        device.ConnectTimeout = request.ConnectTimeout;
        device.ReceiveTimeout = request.ReadTimeout;
        device.SendTimeout = request.WriteTimeout;
        return device;
    }

    private MelsecMcNet CreateMelsecDevice(PlcConnectRequest request)
    {
        var device = new MelsecMcNet(_logger as ILogger<MelsecMcNet>);
        device.ConnectTimeout = request.ConnectTimeout;
        device.ReceiveTimeout = request.ReadTimeout;
        device.SendTimeout = request.WriteTimeout;
        return device;
    }

    private OmronFinsNet CreateOmronDevice(PlcConnectRequest request)
    {
        var device = new OmronFinsNet(_logger as ILogger<OmronFinsNet>);
        device.ConnectTimeout = request.ConnectTimeout;
        device.ReceiveTimeout = request.ReadTimeout;
        device.SendTimeout = request.WriteTimeout;
        return device;
    }

    private async Task<OperateResult<object>> ReadFromDeviceAsync(object device, PlcReadRequest request, CancellationToken cancellationToken)
    {
        if (device is SiemensS7Net siemens)
        {
            var result = await siemens.ReadAsync(request.Address, request.Length, cancellationToken);
            return result.Convert<object>();
        }
        else if (device is ModbusTcpNet modbus)
        {
            if (request.Address.StartsWith("4"))
            {
                var result = await modbus.ReadHoldingRegistersAsync(ushort.Parse(request.Address), (ushort)request.Length, cancellationToken);
                return result.Convert<object>();
            }
            else if (request.Address.StartsWith("3"))
            {
                var result = await modbus.ReadInputRegistersAsync(ushort.Parse(request.Address), (ushort)request.Length, cancellationToken);
                return result.Convert<object>();
            }
            else
            {
                var result = await modbus.ReadBytesAsync(ushort.Parse(request.Address), request.Length, cancellationToken);
                return result.Convert<object>();
            }
        }
        else if (device is MelsecMcNet melsec)
        {
            var result = await melsec.ReadAsync(request.Address, request.Length, cancellationToken);
            return result.Convert<object>();
        }
        else if (device is OmronFinsNet omron)
        {
            var result = await omron.ReadAsync(request.Address, request.Length, cancellationToken);
            return result.Convert<object>();
        }

        return OperateResult<object>.Fail("不支持的设备类型", 1007);
    }

    private async Task<OperateResult> WriteToDeviceAsync(object device, PlcWriteRequest request, CancellationToken cancellationToken)
    {
        var value = request.Values ?? request.Value;
        
        if (device is SiemensS7Net siemens)
        {
            byte[] data = ConvertToBytes(value, request.DataType);
            return await siemens.WriteAsync(request.Address, data, cancellationToken);
        }
        else if (device is ModbusTcpNet modbus)
        {
            byte[] data = ConvertToBytes(value, request.DataType);
            return await modbus.WriteBytesAsync(ushort.Parse(request.Address), data, cancellationToken);
        }
        else if (device is MelsecMcNet melsec)
        {
            byte[] data = ConvertToBytes(value, request.DataType);
            return await melsec.WriteAsync(request.Address, data, cancellationToken);
        }
        else if (device is OmronFinsNet omron)
        {
            byte[] data = ConvertToBytes(value, request.DataType);
            return await omron.WriteAsync(request.Address, data, cancellationToken);
        }

        return OperateResult.Fail("不支持的设备类型", 1007);
    }

    private byte[] ConvertToBytes(object? value, string dataType)
    {
        var transform = new RegularByteTransform();
        
        return dataType.ToLower() switch
        {
            "bool" => transform.GetBytes(Convert.ToBoolean(value)),
            "int16" => transform.GetBytes(Convert.ToInt16(value)),
            "uint16" => transform.GetBytes(Convert.ToUInt16(value)),
            "int32" => transform.GetBytes(Convert.ToInt32(value)),
            "uint32" => transform.GetBytes(Convert.ToUInt32(value)),
            "int64" => transform.GetBytes(Convert.ToInt64(value)),
            "uint64" => transform.GetBytes(Convert.ToUInt64(value)),
            "float" => transform.GetBytes(Convert.ToSingle(value)),
            "double" => transform.GetBytes(Convert.ToDouble(value)),
            "bytes" => (byte[])value!,
            _ => System.Text.Encoding.UTF8.GetBytes(value?.ToString() ?? "")
        };
    }

    private class PlcConnection
    {
        public string ConnectionId { get; set; } = string.Empty;
        public string? ConnectionName { get; set; }
        public string DeviceType { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; }
        public object Device { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public DateTime ConnectedAt { get; set; }
        public DateTime LastActivity { get; set; }
        public int ErrorCount { get; set; }
        public string? LastError { get; set; }
    }
}
