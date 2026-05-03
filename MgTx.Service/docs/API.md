# HslCommunication.Service API 规范文档

## 1. 概述

HslCommunication.Service 提供基于 RESTful API 的工业设备通信服务，支持多种 PLC 协议和数据读写操作。

### 1.1 基础 URL
```
http://localhost:8080/api/v1
```

### 1.2 认证
当前版本暂不启用认证，后续版本将支持 JWT Bearer Token 认证。

### 1.3 通用响应格式

所有 API 响应遵循以下 JSON 格式：

**成功响应：**
```json
{
  "success": true,
  "data": { ... },
  "error": null,
  "timestamp": "2026-05-03T10:00:00Z"
}
```

**错误响应：**
```json
{
  "success": false,
  "data": null,
  "error": {
    "code": 1001,
    "message": "连接失败：设备无响应"
  },
  "timestamp": "2026-05-03T10:00:00Z"
}
```

**错误码定义：**
| 错误码 | 说明 |
|--------|------|
| 0 | 成功 |
| 1001 | 连接失败 |
| 1002 | 连接已存在 |
| 1003 | 连接不存在 |
| 1004 | 读取数据失败 |
| 1005 | 写入数据失败 |
| 1006 | 无效的地址格式 |
| 1007 | 无效的数据类型 |
| 1008 | 服务器内部错误 |
| 1009 | 超时 |
| 1010 | 权限不足 |

---

## 2. 健康检查

### GET /health

获取服务健康状态。

**响应示例：**
```json
{
  "success": true,
  "data": {
    "status": "Healthy",
    "uptime": "02:30:45",
    "version": "1.0.0",
    "activeConnections": 5
  }
}
```

---

## 3. PLC 连接管理

### POST /plc/connect

建立 PLC 连接。

**请求体：**
```json
{
  "deviceType": "SiemensS7",
  "ipAddress": "192.168.1.100",
  "port": 102,
  "cpuType": "S71200",
  "rack": 0,
  "slot": 1,
  "connectionName": "MyPLC",
  "connectTimeout": 5000,
  "readTimeout": 3000,
  "writeTimeout": 3000
}
```

**参数说明：**

| 参数 | 类型 | 必需 | 说明 |
|------|------|------|------|
| deviceType | string | 是 | 设备类型：SiemensS7, ModbusTcp, MelsecMc, OmronFins, AllenBradley, OmronCip |
| ipAddress | string | 是 | 设备 IP 地址 |
| port | int | 是 | 端口号 |
| cpuType | string | 否 | CPU 类型（Siemens 专用） |
| rack | int | 否 | 机架号（Siemens 专用） |
| slot | int | 否 | 槽号（Siemens 专用） |
| connectionName | string | 否 | 连接名称，用于标识 |
| connectTimeout | int | 否 | 连接超时（毫秒），默认 5000 |
| readTimeout | int | 否 | 读取超时（毫秒），默认 3000 |
| writeTimeout | int | 否 | 写入超时（毫秒），默认 3000 |

**响应示例：**
```json
{
  "success": true,
  "data": {
    "connectionId": "conn_abc123def456",
    "connectionName": "MyPLC",
    "deviceType": "SiemensS7",
    "ipAddress": "192.168.1.100",
    "port": 102,
    "status": "Connected",
    "connectedAt": "2026-05-03T10:00:00Z"
  }
}
```

---

### DELETE /plc/disconnect

断开 PLC 连接。

**请求参数：**
- `connectionId` (query): 连接 ID

**响应示例：**
```json
{
  "success": true,
  "data": {
    "connectionId": "conn_abc123def456",
    "status": "Disconnected"
  }
}
```

---

### DELETE /plc/disconnect-all

断开所有 PLC 连接。

**响应示例：**
```json
{
  "success": true,
  "data": {
    "disconnectedCount": 5
  }
}
```

---

### GET /plc/connections

获取所有活动连接列表。

**响应示例：**
```json
{
  "success": true,
  "data": [
    {
      "connectionId": "conn_abc123def456",
      "connectionName": "MyPLC",
      "deviceType": "SiemensS7",
      "ipAddress": "192.168.1.100",
      "port": 102,
      "status": "Connected",
      "connectedAt": "2026-05-03T10:00:00Z",
      "lastActivity": "2026-05-03T10:05:30Z"
    }
  ]
}
```

---

### GET /plc/status/{connectionId}

获取指定连接状态。

**路径参数：**
- `connectionId`: 连接 ID

**响应示例：**
```json
{
  "success": true,
  "data": {
    "connectionId": "conn_abc123def456",
    "status": "Connected",
    "isOnline": true,
    "responseTime": 15,
    "errorCount": 0,
    "lastError": null
  }
}
```

---

## 4. PLC 数据读写

### POST /plc/read

读取 PLC 数据。

**请求体：**
```json
{
  "connectionId": "conn_abc123def456",
  "address": "DB1.DBD0",
  "dataType": "Float",
  "length": 10
}
```

**参数说明：**

| 参数 | 类型 | 必需 | 说明 |
|------|------|------|------|
| connectionId | string | 是 | 连接 ID |
| address | string | 是 | 数据地址 |
| dataType | string | 是 | 数据类型：Bool, Byte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Float, Double, String, Bytes |
| length | int | 否 | 读取长度（用于数组或字符串），默认 1 |

**支持的地址格式：**

**Siemens S7:**
- DB块：`DB1.DBD0` (Double Word), `DB1.DBW0` (Word), `DB1.DBB0` (Byte), `DB1.DBX0.0` (Bit)
- 输入：`I0.0` - `IW100`
- 输出：`Q0.0` - `QW100`
- 标记：`M0.0` - `MW100`
- 计数器：`C0` - `C255`
- 定时器：`T0` - `T255`

**Modbus TCP:**
- 线圈：`0-9999`
- 离散输入：`10000-19999`
- 保持寄存器：`40000-49999`
- 输入寄存器：`30000-39999`

**三菱 MELSEC:**
- X, Y, M, L, F, V, B, D, W, etc.

**欧姆龙 FINS:**
- CIO, WR, HR, AR, DM, etc.

**响应示例（读取浮点数）：**
```json
{
  "success": true,
  "data": {
    "connectionId": "conn_abc123def456",
    "address": "DB1.DBD0",
    "dataType": "Float",
    "values": [123.45, 67.89, 0.0],
    "timestamp": "2026-05-03T10:00:00Z"
  }
}
```

---

### POST /plc/read-batch

批量读取多个地址。

**请求体：**
```json
{
  "connectionId": "conn_abc123def456",
  "reads": [
    { "address": "DB1.DBD0", "dataType": "Float" },
    { "address": "DB1.DBW4", "dataType": "Int16" },
    { "address": "DB1.DBX10.0", "dataType": "Bool" }
  ]
}
```

**响应示例：**
```json
{
  "success": true,
  "data": {
    "connectionId": "conn_abc123def456",
    "results": [
      { "address": "DB1.DBD0", "success": true, "value": 123.45 },
      { "address": "DB1.DBW4", "success": true, "value": -100 },
      { "address": "DB1.DBX10.0", "success": true, "value": true }
    ],
    "timestamp": "2026-05-03T10:00:00Z"
  }
}
```

---

### POST /plc/write

写入 PLC 数据。

**请求体（写入单个值）：**
```json
{
  "connectionId": "conn_abc123def456",
  "address": "DB1.DBD0",
  "dataType": "Float",
  "value": 123.45
}
```

**请求体（写入多个值）：**
```json
{
  "connectionId": "conn_abc123def456",
  "address": "DB1.DBD0",
  "dataType": "Float",
  "values": [123.45, 67.89, 0.0]
}
```

**响应示例：**
```json
{
  "success": true,
  "data": {
    "connectionId": "conn_abc123def456",
    "address": "DB1.DBD0",
    "dataType": "Float",
    "valuesWritten": 3,
    "timestamp": "2026-05-03T10:00:00Z"
  }
}
```

---

### POST /plc/write-batch

批量写入多个地址。

**请求体：**
```json
{
  "connectionId": "conn_abc123def456",
  "writes": [
    { "address": "DB1.DBD0", "dataType": "Float", "value": 123.45 },
    { "address": "DB1.DBW4", "dataType": "Int16", "value": -100 },
    { "address": "DB1.DBX10.0", "dataType": "Bool", "value": true }
  ]
}
```

**响应示例：**
```json
{
  "success": true,
  "data": {
    "connectionId": "conn_abc123def456",
    "results": [
      { "address": "DB1.DBD0", "success": true },
      { "address": "DB1.DBW4", "success": true },
      { "address": "DB1.DBX10.0", "success": true }
    ],
    "successCount": 3,
    "failureCount": 0,
    "timestamp": "2026-05-03T10:00:00Z"
  }
}
```

---

## 5. TCP/UDP 网络服务

### POST /network/tcp/start

启动 TCP 服务器。

**请求体：**
```json
{
  "port": 5000,
  "maxConnections": 100,
  "heartbeatInterval": 30000,
  "receiveBufferSize": 4096
}
```

**响应示例：**
```json
{
  "success": true,
  "data": {
    "serverId": "tcp_server_001",
    "type": "TcpServer",
    "port": 5000,
    "status": "Running"
  }
}
```

---

### POST /network/tcp/stop

停止 TCP 服务器。

**请求参数：**
- `serverId` (query): 服务器 ID

**响应示例：**
```json
{
  "success": true,
  "data": {
    "serverId": "tcp_server_001",
    "status": "Stopped",
    "connectionsCleared": 15
  }
}
```

---

### POST /network/tcp/send

通过 TCP 发送数据。

**请求体：**
```json
{
  "serverId": "tcp_server_001",
  "clientId": "client_001",
  "data": "SGVsbG8gV29ybGQ=",
  "encoding": "Base64"
}
```

**参数说明：**
- `encoding`: 数据编码格式，可选值：`Base64`, `UTF8`, `Hex`

**响应示例：**
```json
{
  "success": true,
  "data": {
    "bytesSent": 11,
    "timestamp": "2026-05-03T10:00:00Z"
  }
}
```

---

### GET /network/tcp/clients

获取 TCP 服务器的客户端列表。

**请求参数：**
- `serverId` (query): 服务器 ID

**响应示例：**
```json
{
  "success": true,
  "data": {
    "serverId": "tcp_server_001",
    "clients": [
      {
        "clientId": "client_001",
        "ipAddress": "192.168.1.101",
        "port": 54321,
        "connectedAt": "2026-05-03T09:00:00Z",
        "lastActivity": "2026-05-03T10:05:30Z"
      }
    ]
  }
}
```

---

### POST /network/udp/start

启动 UDP 服务。

**请求体：**
```json
{
  "port": 5001,
  "broadcastEnabled": true,
  "multicastEnabled": false
}
```

---

### POST /network/udp/send

通过 UDP 发送数据。

**请求体：**
```json
{
  "port": 5001,
  "ipAddress": "192.168.1.255",
  "data": "SGVsbG8gV29ybGQ=",
  "encoding": "Base64"
}
```

---

## 6. 文件传输

### POST /file/upload

上传文件。

**请求：** `multipart/form-data`
- `file`: 文件内容
- `destination`: 目标路径

**响应示例：**
```json
{
  "success": true,
  "data": {
    "fileId": "file_abc123",
    "fileName": "config.json",
    "size": 1024,
    "path": "/data/uploads/config.json",
    "uploadedAt": "2026-05-03T10:00:00Z"
  }
}
```

---

### GET /file/download/{fileId}

下载文件。

**路径参数：**
- `fileId`: 文件 ID

**响应：** 文件流

---

### GET /file/list

列出服务器上的文件。

**请求参数：**
- `path` (query): 目录路径
- `page` (query): 页码
- `pageSize` (query): 每页数量

**响应示例：**
```json
{
  "success": true,
  "data": {
    "files": [
      {
        "fileId": "file_abc123",
        "fileName": "config.json",
        "size": 1024,
        "modifiedAt": "2026-05-03T10:00:00Z"
      }
    ],
    "total": 100,
    "page": 1,
    "pageSize": 20
  }
}
```

---

## 7. 日志

### GET /logs

获取日志列表。

**请求参数：**
- `level` (query): 日志级别 (Debug, Info, Warning, Error)
- `from` (query): 开始时间 (ISO 8601)
- `to` (query): 结束时间 (ISO 8601)
- `page` (query): 页码
- `pageSize` (query): 每页数量

**响应示例：**
```json
{
  "success": true,
  "data": {
    "logs": [
      {
        "timestamp": "2026-05-03T10:00:00Z",
        "level": "Information",
        "message": "PLC connection established",
        "source": "PlcService",
        "connectionId": "conn_abc123def456"
      }
    ],
    "total": 1000,
    "page": 1,
    "pageSize": 20
  }
}
```

---

## 8. 配置

### GET /config

获取当前配置（敏感信息脱敏）。

**响应示例：**
```json
{
  "success": true,
  "data": {
    "service": {
      "host": "0.0.0.0",
      "port": 8080,
      "enableHttps": false
    },
    "logging": {
      "level": "Information",
      "consoleEnabled": true,
      "fileEnabled": true
    },
    "plcConnections": [
      {
        "name": "SiemensPLC1",
        "deviceType": "SiemensS7",
        "ipAddress": "192.168.1.100",
        "port": 102
      }
    ]
  }
}
```

---

### PUT /config

更新配置。

**请求体：**
```json
{
  "logging": {
    "level": "Debug"
  }
}
```

**响应示例：**
```json
{
  "success": true,
  "data": {
    "message": "Configuration updated successfully",
    "reloadRequired": true
  }
}
```

---

## 9. 错误响应

所有错误响应包含以下结构：

```json
{
  "success": false,
  "data": null,
  "error": {
    "code": 1001,
    "message": "连接失败：设备无响应",
    "details": {
      "deviceType": "SiemensS7",
      "ipAddress": "192.168.1.100",
      "reason": "Connection timeout"
    }
  },
  "timestamp": "2026-05-03T10:00:00Z"
}
```

### 常见错误码

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

## 10. 版本历史

| 版本 | 日期 | 说明 |
|------|------|------|
| 1.0.0 | 2026-05-03 | 初始版本 |

---

## 11. 技术支持

如有问题，请联系技术支持团队。
