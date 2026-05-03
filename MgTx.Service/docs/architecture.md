# HslCommunication .NET 10 后台服务技术架构文档

## 1. 项目概述

### 1.1 项目名称
HslCommunication.Service - 工业通信库 .NET 10 重构版

### 1.2 项目目标
将现有的 HslCommunication 工业通信库完全重构为基于 .NET 10 的后台服务，支持跨平台部署（Windows/Linux），提供 RESTful API 接口供第三方应用调用。

### 1.3 核心功能范围
- PLC 设备通信（Modbus TCP/RTU, Siemens S7, Mitsubishi Melsec, Omron Fins, Allen Bradley 等）
- 网络通信服务（TCP/UDP 服务器/客户端）
- 数据读写服务
- 文件传输服务
- Redis 缓存集成
- 日志服务

## 2. 技术架构

### 2.1 架构设计原则
- **模块化设计**：高度解耦的微服务架构
- **跨平台支持**：Windows 和 Linux 原生支持
- **高性能**：充分利用 .NET 10 的性能优化
- **易扩展**：插件化设计支持新设备类型

### 2.2 项目结构
```
HslCommunication.Service/
├── src/
│   ├── HslCommunication.Core/           # 核心库
│   │   ├── Types/                       # 数据类型
│   │   ├── Net/                         # 网络基础
│   │   ├── Transfer/                   # 数据转换
│   │   └── Address/                     # 地址解析
│   ├── HslCommunication.Plc/             # PLC 通信模块
│   │   ├── Modbus/                      # Modbus 协议
│   │   ├── Siemens/                     # 西门子 S7
│   │   ├── Melsec/                      # 三菱 MELSEC
│   │   ├── Omron/                       # 欧姆龙
│   │   └── AllenBradley/                # AB PLC
│   ├── HslCommunication.Network/        # 网络服务模块
│   │   ├── Tcp/                         # TCP 服务
│   │   ├── Udp/                         # UDP 服务
│   │   └── FileTransfer/                # 文件传输
│   ├── HslCommunication.Service/        # 后台服务主程序
│   │   ├── Controllers/                 # API 控制器
│   │   ├── Services/                    # 业务服务
│   │   ├── Configuration/               # 配置管理
│   │   └── Program.cs                    # 入口点
│   └── HslCommunication.Api/             # API 契约
│       ├── Models/                       # 数据模型
│       └── Interfaces/                  # API 接口定义
├── tests/
│   └── HslCommunication.Tests/           # 单元测试
└── docs/
    └── API.md                           # API 文档
```

### 2.3 技术栈
- **运行时**：.NET 10
- **Web 框架**：ASP.NET Core Minimal API
- **依赖注入**：Microsoft.Extensions.DependencyInjection
- **配置**：Microsoft.Extensions.Configuration
- **日志**：Microsoft.Extensions.Logging + Serilog
- **JSON**：System.Text.Json（.NET 10 内置）
- **异步**：async/await + ValueTask
- **AOT**：支持 Native AOT 编译

## 3. 核心模块设计

### 3.1 核心库 (HslCommunication.Core)

#### 3.1.1 数据类型 (Types)
```csharp
// 核心结果类型 - 使用 record 提高性能
public record OperateResult(bool IsSuccess, string? Message = null, int ErrorCode = 0);
public record OperateResult<T>(bool IsSuccess, T? Content, string? Message = null, int ErrorCode = 0);
```

#### 3.1.2 网络基础 (Net)
- NetworkBase：异步 TCP/UDP 基类
- 使用 Span<T> 和 Memory<T> 优化内存
- CancellationToken 支持
- 连接池管理

#### 3.1.3 数据转换 (Transfer)
- ByteTransform：字节序转换
- 支持 BigEndian/LittleEndian
- DataFormat 配置

### 3.2 PLC 通信模块 (HslCommunication.Plc)

#### 3.2.1 Modbus TCP
- 读/写线圈 (Coil)
- 读/写寄存器 (Register)
- 支持多个从站
- 连接池管理

#### 3.2.2 Siemens S7
- S7-200/300/400/1200/1500 支持
- 区域访问 (DB/M/I/Q)
- 读/写字节/字/双字/浮点数

#### 3.2.3 Mitsubishi Melsec
- MC 协议支持
- ASCII/Binary 格式
- QnA 3E 协议

#### 3.2.4 Omron Fins
- FINS TCP/UDP
- 内存区域访问
- CIP 协议支持

#### 3.2.5 Allen Bradley
- EtherNet/IP (CIP)
- Tag 读写
- UDT 支持

### 3.3 网络服务模块 (HslCommunication.Network)

#### 3.3.1 TCP 服务
- 异步接受连接
- 心跳检测
- 多客户端管理
- 数据加密选项

#### 3.3.2 UDP 服务
- 单播/多播支持
- 数据校验

#### 3.3.3 文件传输
- 分块传输
- 断点续传
- MD5 校验

### 3.4 后台服务 (HslCommunication.Service)

#### 3.4.1 API 控制器
```
POST   /api/v1/plc/connect          # 建立 PLC 连接
DELETE /api/v1/plc/disconnect       # 断开连接
POST   /api/v1/plc/read             # 读取数据
POST   /api/v1/plc/write            # 写入数据
GET    /api/v1/plc/status/{id}      # 获取连接状态
POST   /api/v1/network/tcp/start    # 启动 TCP 服务器
POST   /api/v1/network/tcp/stop     # 停止 TCP 服务器
POST   /api/v1/file/upload          # 上传文件
GET    /api/v1/file/download/{id}    # 下载文件
GET    /api/v1/health               # 健康检查
```

#### 3.4.2 配置管理
- YAML 配置文件
- 环境变量覆盖
- 热重载支持

#### 3.4.3 后台服务
- Windows 服务支持
- Linux systemd 支持
- 健康检查接口
- 优雅关闭

## 4. API 规范

### 4.1 通用响应格式
```json
{
  "success": true,
  "data": { },
  "error": {
    "code": 0,
    "message": "操作成功"
  },
  "timestamp": "2026-05-03T10:00:00Z"
}
```

### 4.2 PLC 连接请求
```json
POST /api/v1/plc/connect
{
  "deviceType": "SiemensS7",
  "ipAddress": "192.168.1.100",
  "port": 102,
  "cpuType": "S71200",
  "rack": 0,
  "slot": 1,
  "connectionName": "MyPLC"
}
```

### 4.3 数据读取请求
```json
POST /api/v1/plc/read
{
  "connectionId": "abc123",
  "address": "DB1.DBD0",
  "dataType": "Float",
  "length": 10
}
```

## 5. 配置示例

### 5.1 appsettings.json
```json
{
  "Service": {
    "Host": "0.0.0.0",
    "Port": 8080,
    "EnableHttps": false
  },
  "Logging": {
    "Level": "Information",
    "File": {
      "Enabled": true,
      "Path": "./logs/service.log",
      "RollingInterval": "Day"
    }
  },
  "PlcConnections": [
    {
      "Name": "SiemensPLC1",
      "DeviceType": "SiemensS7",
      "IpAddress": "192.168.1.100",
      "Port": 102,
      "CpuType": "S71200",
      "Rack": 0,
      "Slot": 1
    }
  ]
}
```

## 6. 部署要求

### 6.1 Windows
- .NET 10 Runtime
- Windows 10/11 或 Windows Server 2019+
- 可选：安装为 Windows 服务

### 6.2 Linux
- .NET 10 Runtime
- Ubuntu 20.04+ / CentOS 8+ / Debian 11+
- 可选：配置 systemd 服务

## 7. 性能目标

- 单 PLC 连接：< 10ms 读写延迟
- 并发连接数：1000+
- API 响应时间：< 50ms (P95)
- 内存占用：< 100MB (空闲状态)
- CPU 占用：< 5% (空闲状态)

## 8. 安全考虑

- API 认证（JWT Bearer Token）
- PLC 连接加密
- 日志脱敏
- 输入验证
- CORS 配置

## 9. 监控和诊断

- Prometheus 指标导出
- 结构化日志
- 健康检查端点
- 性能计数器
- 连接状态监控

## 10. 未来扩展

- gRPC 接口支持
- WebSocket 实时数据推送
- OPC UA 支持
- MQTT 集成
- 云原生支持（Kubernetes）
