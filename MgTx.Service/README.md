# MgTx.Service - 猛攻通讯

基于 .NET 10 的工业通信后台服务，提供 RESTful API、实时接口、Web UI 以及脚本引擎，支持多种 PLC 设备的连接和数据读写操作。

## 功能特性

### 支持的 PLC 协议
- **Siemens S7**: S7-200/300/400/1200/1500 系列
- **Modbus TCP/RTU**: 标准 Modbus 协议支持
- **Mitsubishi MELSEC**: MC 协议（Binary/ASCII）
- **Omron FINS**: FINS TCP/UDP 协议
- **Allen Bradley**: EtherNet/IP
- **Keyence**: Keyence PLC 通信
- **Panasonic**: Panasonic PLC 通信
- **Fuji Electric**: Fuji PLC 通信
- **Delta**: Delta PLC 通信
- **LS Electric**: LS PLC 通信

### 网络服务
- TCP 服务器/客户端
- UDP 服务器/客户端
- 文件传输服务

### 实时通信
- **SignalR WebSocket**: 实时数据推送
- **订阅机制**: 支持标签、设备、事件订阅
- **数据变更通知**: 实时数据变更广播

### Web UI
- **设备管理**: 设备配置和连接管理
- **标签监控**: 实时数据可视化
- **OPC配置**: 类似 OPC 的配置界面
- **脚本管理**: 脚本编辑和执行
- **日志查看**: 系统日志查看

### 脚本引擎
- **JavaScript 执行**: 支持 Jint 引擎
- **Python 执行**: 支持 IronPython
- **事件处理**: 通讯事件处理
- **数据解析**: 自定义数据解析
- **扩展函数**: 内置实用工具函数

### 服务特性
- 跨平台支持（Windows/Linux）
- 异步 API 设计
- 连接池管理
- 结构化日志
- 健康检查
- Docker 容器化部署
- Linux Systemd 服务支持

## 项目结构

```
MgTx.Service/
├── src/
│   ├── MgTx.Core/              # 核心库
│   ├── MgTx.Plc/               # PLC 通信模块
│   ├── MgTx.Network/           # 网络服务模块
│   └── MgTx.Service/           # 后台服务主程序
├── tests/
│   └── MgTx.Tests/             # 单元测试
├── docs/
│   ├── architecture.md         # 技术架构文档
│   └── API.md                  # API 规范文档
├── LICENSE                     # MIT 许可证
├── COPYRIGHT.md                # 版权声明
├── NOTICE.md                   # 第三方声明
└── README.md                   # 项目说明
```

## 快速开始

### 环境要求
- .NET 10 SDK
- Windows 10/11 或 Linux (Ubuntu 20.04+ / CentOS 8+)

### 构建项目

```bash
cd MgTx.Service
dotnet restore
dotnet build
```

### 运行服务

```bash
cd src/MgTx.Service
dotnet run
```

服务将在 http://localhost:8080 启动

### Docker 部署

```bash
docker build -t mgtx-service .
docker run -p 8080:8080 mgtx-service
```

## API 使用示例

### 建立 PLC 连接

```bash
curl -X POST http://localhost:8080/api/v1/plc/connect \
  -H "Content-Type: application/json" \
  -d '{
    "deviceType": "SiemensS7",
    "ipAddress": "192.168.1.100",
    "port": 102,
    "cpuType": "S71200",
    "rack": 0,
    "slot": 1,
    "connectionName": "MyPLC"
  }'
```

### 读取 PLC 数据

```bash
curl -X POST http://localhost:8080/api/v1/plc/read \
  -H "Content-Type: application/json" \
  -d '{
    "connectionId": "conn_abc123def456",
    "address": "DB1.DBD0",
    "dataType": "Float",
    "length": 1
  }'
```

### 写入 PLC 数据

```bash
curl -X POST http://localhost:8080/api/v1/plc/write \
  -H "Content-Type: application/json" \
  -d '{
    "connectionId": "conn_abc123def456",
    "address": "DB1.DBD0",
    "dataType": "Float",
    "value": 123.45
  }'
```

### 健康检查

```bash
curl http://localhost:8080/api/v1/health
```

## 配置文件

编辑 `appsettings.json` 来自定义服务配置：

```json
{
  "Service": {
    "Host": "0.0.0.0",
    "Port": 8080,
    "EnableHttps": false
  },
  "Logging": {
    "Level": "Information",
    "ConsoleEnabled": true,
    "FileEnabled": true
  },
  "PlcConnections": []
}
```

## 开发指南

### 添加新的 PLC 协议支持

1. 在 `MgTx.Plc` 项目中创建新的协议实现类
2. 实现设备连接和数据读写方法
3. 在 `PlcConnectionService` 中注册新的设备类型
4. 添加相应的 API 控制器方法

### 代码规范

- 使用 C# 14 / .NET 10 的新特性
- 优先使用异步 API
- 遵循 RESTful 设计原则
- 完整的错误处理和日志记录
- 单元测试覆盖率要求 > 80%

## 性能指标

- 单 PLC 连接：< 10ms 读写延迟
- 并发连接数：1000+
- API 响应时间：< 50ms (P95)
- 内存占用：< 100MB (空闲状态)

## 安全建议

- 生产环境建议启用 HTTPS
- 配置 API 认证机制
- 使用防火墙限制访问
- 定期更新依赖包

## 版权与许可证

### 版权声明

© 2026 MgTx - 猛攻通讯. 保留所有权利。

本项目为完全独立开发的工业通信框架，与任何其他第三方通信库无关联。

### 许可证

本项目采用 **MIT 许可证** 开源。详见 [LICENSE](LICENSE) 文件。

### 第三方依赖

| 组件 | 许可证 | 用途 |
|------|--------|------|
| .NET 10 | MIT | 运行时框架 |
| Serilog | Apache-2.0 | 日志系统 |
| Jint | BSD-2-Clause | JavaScript 引擎 |
| IronPython | Apache-2.0 | Python 引擎 |
| SignalR | MIT | 实时通信 |

### 协议标准

本项目仅实现公开的工业标准通信协议，不包含任何专有算法：
- **Modbus TCP** - IEC 61158 标准
- **Siemens S7** - 西门子公开协议
- **Mitsubishi MC** - 三菱公开协议
- **Omron FINS** - 欧姆龙公开协议

## 技术支持

如有问题，请通过以下方式联系：
- 提交 Issue
- 发送邮件至技术支持邮箱

## 贡献指南

欢迎贡献代码！请查看 [COPYRIGHT.md](COPYRIGHT.md) 和 [NOTICE.md](NOTICE.md) 了解项目版权说明。

## 版本历史

- **1.0.0** (2026-05-03): 初始版本，支持 .NET 10
