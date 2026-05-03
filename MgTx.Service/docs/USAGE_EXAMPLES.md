# HslCommunication.Service API 使用示例

本文档提供了 HslCommunication.Service API 的详细使用示例。

## 目录

- [PLC 连接管理](#plc-连接管理)
- [数据读写](#数据读写)
- [网络服务](#网络服务)
- [健康检查](#健康检查)
- [C# 客户端示例](#c-客户端示例)
- [Python 客户端示例](#python-客户端示例)

---

## PLC 连接管理

### 1. 建立 Modbus TCP 连接

```bash
curl -X POST http://localhost:8080/api/v1/plc/connect \
  -H "Content-Type: application/json" \
  -d '{
    "deviceType": "ModbusTcp",
    "ipAddress": "192.168.1.100",
    "port": 502,
    "connectionName": "ModbusDevice1",
    "connectTimeout": 5000,
    "readTimeout": 3000,
    "writeTimeout": 3000
  }'
```

**响应示例：**
```json
{
  "success": true,
  "data": {
    "connectionId": "conn_a1b2c3d4e5f6",
    "connectionName": "ModbusDevice1",
    "deviceType": "ModbusTcp",
    "ipAddress": "192.168.1.100",
    "port": 502,
    "status": "Connected",
    "connectedAt": "2026-05-03T10:00:00Z"
  }
}
```

### 2. 建立 Siemens S7 连接

```bash
curl -X POST http://localhost:8080/api/v1/plc/connect \
  -H "Content-Type: application/json" \
  -d '{
    "deviceType": "SiemensS7",
    "ipAddress": "192.168.1.101",
    "port": 102,
    "cpuType": "S71200",
    "rack": 0,
    "slot": 1,
    "connectionName": "SiemensPLC1"
  }'
```

### 3. 建立三菱 MELSEC MC 连接

```bash
curl -X POST http://localhost:8080/api/v1/plc/connect \
  -H "Content-Type: application/json" \
  -d '{
    "deviceType": "MelsecMc",
    "ipAddress": "192.168.1.102",
    "port": 5002,
    "connectionName": "MelsecPLC1"
  }'
```

### 4. 获取所有连接列表

```bash
curl http://localhost:8080/api/v1/plc/connections
```

**响应示例：**
```json
{
  "success": true,
  "data": [
    {
      "connectionId": "conn_a1b2c3d4e5f6",
      "connectionName": "ModbusDevice1",
      "deviceType": "ModbusTcp",
      "ipAddress": "192.168.1.100",
      "port": 502,
      "status": "Connected",
      "connectedAt": "2026-05-03T10:00:00Z",
      "lastActivity": "2026-05-03T10:05:30Z"
    }
  ]
}
```

### 5. 获取连接状态

```bash
curl http://localhost:8080/api/v1/plc/status/conn_a1b2c3d4e5f6
```

### 6. 断开连接

```bash
curl -X DELETE http://localhost:8080/api/v1/plc/disconnect?connectionId=conn_a1b2c3d4e5f6
```

### 7. 断开所有连接

```bash
curl -X DELETE http://localhost:8080/api/v1/plc/disconnect-all
```

---

## 数据读写

### 1. 读取保持寄存器 (Modbus)

```bash
curl -X POST http://localhost:8080/api/v1/plc/read \
  -H "Content-Type: application/json" \
  -d '{
    "connectionId": "conn_a1b2c3d4e5f6",
    "address": "400001",
    "dataType": "UInt16",
    "length": 10
  }'
```

### 2. 读取 DB 块数据 (Siemens)

```bash
curl -X POST http://localhost:8080/api/v1/plc/read \
  -H "Content-Type: application/json" \
  -d '{
    "connectionId": "conn_b2c3d4e5f6g7",
    "address": "DB1.DBD0",
    "dataType": "Float",
    "length": 1
  }'
```

**响应示例：**
```json
{
  "success": true,
  "data": {
    "connectionId": "conn_b2c3d4e5f6g7",
    "address": "DB1.DBD0",
    "dataType": "Float",
    "values": [123.45],
    "timestamp": "2026-05-03T10:05:30Z"
  }
}
```

### 3. 读取位数据 (Siemens)

```bash
curl -X POST http://localhost:8080/api/v1/plc/read \
  -H "Content-Type: application/json" \
  -d '{
    "connectionId": "conn_b2c3d4e5f6g7",
    "address": "DB1.DBX10.0",
    "dataType": "Bool",
    "length": 8
  }'
```

### 4. 写入单个值

```bash
curl -X POST http://localhost:8080/api/v1/plc/write \
  -H "Content-Type: application/json" \
  -d '{
    "connectionId": "conn_a1b2c3d4e5f6",
    "address": "400001",
    "dataType": "UInt16",
    "value": 1234
  }'
```

### 5. 写入多个值

```bash
curl -X POST http://localhost:8080/api/v1/plc/write \
  -H "Content-Type: application/json" \
  -d '{
    "connectionId": "conn_a1b2c3d4e5f6",
    "address": "400001",
    "dataType": "Float",
    "values": [123.45, 67.89, 0.0]
  }'
```

### 6. 写入 Siemens DB 块

```bash
curl -X POST http://localhost:8080/api/v1/plc/write \
  -H "Content-Type: application/json" \
  -d '{
    "connectionId": "conn_b2c3d4e5f6g7",
    "address": "DB1.DBD0",
    "dataType": "Float",
    "value": 999.99
  }'
```

### 7. 批量读取

```bash
curl -X POST http://localhost:8080/api/v1/plc/read-batch \
  -H "Content-Type: application/json" \
  -d '{
    "connectionId": "conn_b2c3d4e5f6g7",
    "reads": [
      { "address": "DB1.DBD0", "dataType": "Float" },
      { "address": "DB1.DBD4", "dataType": "Float" },
      { "address": "DB1.DBW8", "dataType": "Int16" }
    ]
  }'
```

---

## 网络服务

### 1. 启动 TCP 服务器

```bash
curl -X POST http://localhost:8080/api/v1/network/tcp/start \
  -H "Content-Type: application/json" \
  -d '{
    "port": 5000,
    "maxConnections": 100,
    "heartbeatInterval": 30000,
    "receiveBufferSize": 4096
  }'
```

### 2. 获取客户端列表

```bash
curl http://localhost:8080/api/v1/network/tcp/clients?serverId=tcp_server_001
```

### 3. 发送数据到客户端

```bash
curl -X POST http://localhost:8080/api/v1/network/tcp/send \
  -H "Content-Type: application/json" \
  -d '{
    "serverId": "tcp_server_001",
    "clientId": "client_001",
    "data": "SGVsbG8gV29ybGQ=",
    "encoding": "Base64"
  }'
```

### 4. 停止 TCP 服务器

```bash
curl -X POST http://localhost:8080/api/v1/network/tcp/stop?serverId=tcp_server_001
```

---

## 健康检查

### 1. 获取服务健康状态

```bash
curl http://localhost:8080/api/v1/health
```

**响应示例：**
```json
{
  "success": true,
  "data": {
    "status": "Healthy",
    "uptime": "02:30:45",
    "version": "1.0.0",
    "activeConnections": 3
  }
}
```

---

## C# 客户端示例

```csharp
using System.Net.Http.Json;
using System.Text.Json;

var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:8080") };

// 1. 连接 PLC
var connectRequest = new
{
    deviceType = "ModbusTcp",
    ipAddress = "192.168.1.100",
    port = 502,
    connectionName = "TestPLC"
};

var connectResponse = await httpClient.PostAsJsonAsync("/api/v1/plc/connect", connectRequest);
var connectResult = await connectResponse.Content.ReadFromJsonAsync<JsonElement>();

if (connectResult.GetProperty("success").GetBoolean())
{
    var connectionId = connectResult.GetProperty("data").GetProperty("connectionId").GetString();
    Console.WriteLine($"连接成功: {connectionId}");

    // 2. 读取数据
    var readRequest = new
    {
        connectionId = connectionId,
        address = "400001",
        dataType = "UInt16",
        length = 10
    };

    var readResponse = await httpClient.PostAsJsonAsync("/api/v1/plc/read", readRequest);
    var readResult = await readResponse.Content.ReadFromJsonAsync<JsonElement>();

    if (readResult.GetProperty("success").GetBoolean())
    {
        var values = readResult.GetProperty("data").GetProperty("values");
        Console.WriteLine($"读取成功: {values}");
    }

    // 3. 写入数据
    var writeRequest = new
    {
        connectionId = connectionId,
        address = "400001",
        dataType = "UInt16",
        value = 1234
    };

    var writeResponse = await httpClient.PostAsJsonAsync("/api/v1/plc/write", writeRequest);
    var writeResult = await writeResponse.Content.ReadFromJsonAsync<JsonElement>();

    Console.WriteLine($"写入结果: {writeResult.GetProperty("success").GetBoolean()}");

    // 4. 断开连接
    await httpClient.DeleteAsync($"/api/v1/plc/disconnect?connectionId={connectionId}");
}
```

---

## Python 客户端示例

```python
import requests
import json

class HslCommunicationClient:
    def __init__(self, base_url="http://localhost:8080"):
        self.base_url = base_url
        self.session = requests.Session()
    
    def connect(self, device_type, ip_address, port=502, **kwargs):
        """建立 PLC 连接"""
        data = {
            "deviceType": device_type,
            "ipAddress": ip_address,
            "port": port,
            **kwargs
        }
        response = self.session.post(f"{self.base_url}/api/v1/plc/connect", json=data)
        result = response.json()
        
        if result.get("success"):
            return result["data"]["connectionId"]
        else:
            raise Exception(result["error"]["message"])
    
    def disconnect(self, connection_id):
        """断开 PLC 连接"""
        response = self.session.delete(f"{self.base_url}/api/v1/plc/disconnect?connectionId={connection_id}")
        return response.json()
    
    def read(self, connection_id, address, data_type="Float", length=1):
        """读取数据"""
        data = {
            "connectionId": connection_id,
            "address": address,
            "dataType": data_type,
            "length": length
        }
        response = self.session.post(f"{self.base_url}/api/v1/plc/read", json=data)
        result = response.json()
        
        if result.get("success"):
            return result["data"]["values"]
        else:
            raise Exception(result["error"]["message"])
    
    def write(self, connection_id, address, value, data_type="Float"):
        """写入数据"""
        data = {
            "connectionId": connection_id,
            "address": address,
            "dataType": data_type,
            "value": value
        }
        response = self.session.post(f"{self.base_url}/api/v1/plc/write", json=data)
        result = response.json()
        
        if not result.get("success"):
            raise Exception(result["error"]["message"])
        return result
    
    def health(self):
        """健康检查"""
        response = self.session.get(f"{self.base_url}/api/v1/health")
        return response.json()


# 使用示例
if __name__ == "__main__":
    client = HslCommunicationClient()
    
    try:
        # 连接 PLC
        connection_id = client.connect(
            device_type="ModbusTcp",
            ip_address="192.168.1.100",
            port=502,
            connection_name="TestPLC"
        )
        print(f"连接成功: {connection_id}")
        
        # 读取数据
        values = client.read(connection_id, "400001", data_type="UInt16", length=10)
        print(f"读取数据: {values}")
        
        # 写入数据
        client.write(connection_id, "400001", 1234, data_type="UInt16")
        print("写入成功")
        
        # 健康检查
        health = client.health()
        print(f"健康状态: {health['data']['status']}")
        
    except Exception as e:
        print(f"错误: {e}")
    finally:
        if 'connection_id' in locals():
            client.disconnect(connection_id)
            print("连接已断开")
```

---

## 错误码参考

| 错误码 | HTTP 状态码 | 说明 |
|--------|-------------|------|
| 0 | 200 | 成功 |
| 1001 | 400 | 连接失败 |
| 1002 | 409 | 连接已存在 |
| 1003 | 404 | 连接不存在 |
| 1004 | 400 | 读取数据失败 |
| 1005 | 400 | 写入数据失败 |
| 1006 | 400 | 无效的地址格式 |
| 1007 | 400 | 无效的数据类型 |
| 1008 | 500 | 服务器内部错误 |
| 1009 | 408 | 超时 |
| 1010 | 403 | 权限不足 |

---

## 支持的数据类型

| 数据类型 | 说明 | 示例 |
|----------|------|------|
| Bool | 布尔值 | true/false |
| Byte | 字节 | 0-255 |
| Int16 | 16位整数 | -32768 ~ 32767 |
| UInt16 | 16位无符号整数 | 0 ~ 65535 |
| Int32 | 32位整数 | -2147483648 ~ 2147483647 |
| UInt32 | 32位无符号整数 | 0 ~ 4294967295 |
| Int64 | 64位整数 | -9223372036854775808 ~ 9223372036854775807 |
| UInt64 | 64位无符号整数 | 0 ~ 18446744073709551615 |
| Float | 单精度浮点数 | ±1.5×10^-45 ~ ±3.4×10^38 |
| Double | 双精度浮点数 | ±5.0×10^-324 ~ ±1.7×10^308 |
| String | 字符串 | "Hello World" |
| Bytes | 字节数组 | [0x01, 0x02, 0x03] |

---

## 最佳实践

1. **连接管理**
   - 建议使用连接池管理 PLC 连接
   - 长时间不使用的连接应该主动断开
   - 定期检查连接状态

2. **数据读写**
   - 批量读取比多次单独读取更高效
   - 避免频繁的小数据读写操作
   - 使用合适的数据类型

3. **错误处理**
   - 总是检查 API 响应的 `success` 字段
   - 实现重试机制处理临时性错误
   - 记录详细的错误日志

4. **性能优化**
   - 使用异步 API 提高吞吐量
   - 合理设置超时时间
   - 监控连接状态和响应时间

5. **安全建议**
   - 生产环境启用 HTTPS
   - 配置 API 认证
   - 使用防火墙限制访问
