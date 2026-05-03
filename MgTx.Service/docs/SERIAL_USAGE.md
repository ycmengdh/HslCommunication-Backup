# MgTx 串口通信使用指南

## 概述

MgTx 框架提供了完整的串口通信支持，包括 Modbus RTU 和 Modbus ASCII 协议。

## 支持的功能

### Modbus RTU
- ✅ CRC 校验支持
- ✅ 线圈读写（功能码 01, 05, 15）
- ✅ 离散输入读取（功能码 02）
- ✅ 保持寄存器读写（功能码 03, 06, 16）
- ✅ 输入寄存器读取（功能码 04）

### Modbus ASCII
- ✅ LRC 校验支持
- ✅ 与 Modbus RTU 相同的功能支持

## 快速开始

### 基本使用示例

#### Modbus RTU

```csharp
using MgTx.Plc.Modbus;

// 创建 Modbus RTU 客户端
var modbusRtu = new ModbusRtuNet();

// 配置串口参数
modbusRtu.PortName = "COM1";
modbusRtu.BaudRate = 9600;
modbusRtu.DataBits = 8;
modbusRtu.StopBits = StopBits.One;
modbusRtu.Parity = Parity.None;
modbusRtu.StationId = 1;

// 打开串口
var openResult = modbusRtu.Open();
if (!openResult.IsSuccess)
{
    Console.WriteLine($"打开串口失败: {openResult.Message}");
    return;
}

try
{
    // 读取保持寄存器（功能码 03）
    var readResult = modbusRtu.ReadHoldingRegisters(0, 10);
    if (readResult.IsSuccess)
    {
        Console.WriteLine("读取寄存器: " + string.Join(", ", readResult.Content!);
    }
    else
    {
        Console.WriteLine($"读取失败: {readResult.Message}");
    }
    
    // 写入单个寄存器（功能码 06）
    var writeResult = modbusRtu.WriteSingleRegister(0, 12345);
    if (!writeResult.IsSuccess)
    {
        Console.WriteLine("写入成功");
    }
}
finally
{
    // 关闭串口
    modbusRtu.Close();
}
```

#### Modbus ASCII

```csharp
using MgTx.Plc.Modbus;

// 创建 Modbus ASCII 客户端
var modbusAscii = new ModbusAsciiNet();

modbusAscii.PortName = "COM1";
modbusAscii.BaudRate = 9600;
modbusAscii.StationId = 1;

var openResult = modbusAscii.Open();
if (!openResult.IsSuccess)
{
    Console.WriteLine($"打开串口失败: {openResult.Message}");
    return;
}

// 读取线圈
var readResult = modbusAscii.ReadCoils(0, 8);
if (readResult.IsSuccess)
{
    Console.WriteLine("线圈状态: " + string.Join(", ", readResult.Content!));
}

modbusAscii.Close();
```

## 功能详解

### 串口配置参数

| 参数 | 说明 | 常用值 |
|------|------|--------|
| PortName | 串口名称 | Windows: COM1, COM2... Linux: /dev/ttyS0, /dev/ttyUSB0 |
| BaudRate | 波特率 | 9600, 19200, 38400, 115200 |
| DataBits | 数据位 | 8 |
| StopBits | 停止位 | StopBits.One |
| Parity | 校验位 | Parity.None |

### Modbus 地址和功能码

| 功能码 | 说明 | 操作 |
|--------|------|------|
| 01 | 读取线圈 | 读取数字输出 |
| 02 | 读取离散输入 | 读取数字输入 |
| 03 | 读取保持寄存器 | 读取模拟量数据 |
| 04 | 读取输入寄存器 | 读取模拟输入 |
| 05 | 写入单个线圈 | 控制单个数字输出 |
| 06 | 写入单个寄存器 | 写入单个保持寄存器 |
| 15 | 写入多个线圈 | 控制多个数字输出 |
| 16 | 写入多个寄存器 | 写入多个保持寄存器 |

### 静态方法

#### ModbusRtuNet

```csharp
// 计算 CRC
ushort crc = ModbusRtuNet.CalculateCRC(data);

// 添加 CRC
byte[] fullCommand = ModbusRtuNet.AddCRC(command);

// 验证 CRC
bool valid = ModbusRtuNet.VerifyCRC(response);
```

#### ModbusAsciiNet

```csharp
// 计算 LRC
byte lrc = ModbusAsciiNet.CalculateLRC(data);

// 添加 LRC 和 ASCII 格式
byte[] fullFrame = ModbusAsciiNet.AddLRC(command);

// 解析和验证
var dataResult = ModbusAsciiNet.RemoveLRC(asciiData);
```

## 常见问题

### Q: 如何获取可用的串口列表？

```csharp
string[] ports = MgTx.Core.Net.SerialBase.GetPortNames();
foreach (string port in ports)
{
    Console.WriteLine("找到串口: " + port);
}
```

### Q: RTU 和 ASCII 的区别？

- **Modbus RTU**: 二进制格式，使用 CRC 校验，传输效率高
- **Modbus ASCII**: ASCII 编码格式，使用 LRC 校验，可读性好，调试方便

### Q: 在 Linux 上的串口权限？

确保你的用户有串口访问权限，通常需要加入 dialout 或 uucp 用户组：

```bash
sudo usermod -aG dialout your_username
```

## 注意事项

1. **串口独占：同一时刻只能有一个应用访问同一串口
2. **超时设置**: 根据网络情况调整超时参数
3. **地址范围：Modbus 地址通常从 0 或 1 开始，根据设备文档说明
4. **数据格式：读写时注意数据大小端格式

## 相关文档

- [项目 README](../README.md)
- [项目概述
