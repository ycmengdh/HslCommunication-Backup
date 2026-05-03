// MgTx - 猛攻通讯
// Copyright (c) 2026 MgTx Project
// Licensed under MIT License
// https://github.com/mgtx/mgtx-service

namespace MgTx.Service.Models;

/// <summary>
/// 串口连接请求模型
/// </summary>
public class SerialConnectRequest
{
    /// <summary>
    /// 串口名称（如 COM1 或 /dev/ttyS0）
    /// </summary>
    public string PortName { get; set; } = "COM1";
    
    /// <summary>
    /// 波特率
    /// </summary>
    public int BaudRate { get; set; } = 9600;
    
    /// <summary>
    /// 数据位
    /// </summary>
    public int DataBits { get; set; } = 8;
    
    /// <summary>
    /// 停止位
    /// </summary>
    public int StopBits { get; set; } = 1;
    
    /// <summary>
    /// 校验位（0=None, 1=Odd, 2=Even）
    /// </summary>
    public int Parity { get; set; } = 0;
    
    /// <summary>
    /// 协议类型（RTU 或 ASCII）
    /// </summary>
    public string Protocol { get; set; } = "RTU";
    
    /// <summary>
    /// 从站地址
    /// </summary>
    public byte StationId { get; set; } = 1;
    
    /// <summary>
    /// 连接名称（可选）
    /// </summary>
    public string ConnectionName { get; set; } = "";
}

/// <summary>
/// Modbus 读取请求模型
/// </summary>
public class ModbusReadRequest
{
    /// <summary>
    /// 连接 ID
    /// </summary>
    public string ConnectionId { get; set; } = "";
    
    /// <summary>
    /// 功能码（01=线圈, 02=离散输入, 03=保持寄存器, 04=输入寄存器）
    /// </summary>
    public byte FunctionCode { get; set; } = 03;
    
    /// <summary>
    /// 起始地址
    /// </summary>
    public ushort Address { get; set; } = 0;
    
    /// <summary>
    /// 读取数量
    /// </summary>
    public ushort Length { get; set; } = 10;
}

/// <summary>
/// Modbus 单个写入请求模型
/// </summary>
public class ModbusSingleWriteRequest
{
    /// <summary>
    /// 连接 ID
    /// </summary>
    public string ConnectionId { get; set; } = "";
    
    /// <summary>
    /// 功能码（05=单线圈, 06=单寄存器）
    /// </summary>
    public byte FunctionCode { get; set; } = 06;
    
    /// <summary>
    /// 地址
    /// </summary>
    public ushort Address { get; set; } = 0;
    
    /// <summary>
    /// 写入值（对于线圈 true/false，对于寄存器 0-65535）
    /// </summary>
    public ushort Value { get; set; } = 0;
    
    /// <summary>
    /// 是否是布尔值（用于线圈）
    /// </summary>
    public bool IsBoolean { get; set; } = false;
}

/// <summary>
/// Modbus 批量写入请求模型
/// </summary>
public class ModbusBatchWriteRequest
{
    /// <summary>
    /// 连接 ID
    /// </summary>
    public string ConnectionId { get; set; } = "";
    
    /// <summary>
    /// 功能码（15=多线圈, 16=多寄存器）
    /// </summary>
    public byte FunctionCode { get; set; } = 16;
    
    /// <summary>
    /// 起始地址
    /// </summary>
    public ushort Address { get; set; } = 0;
    
    /// <summary>
    /// 写入值（线圈为 bool 数组，寄存器为 ushort 数组）
    /// </summary>
    public ushort[] Values { get; set; } = new ushort[0];
    
    /// <summary>
    /// 线圈值（如果写入线圈）
    /// </summary>
    public bool[] BoolValues { get; set; } = new bool[0];
}

/// <summary>
/// 串口状态响应模型
/// </summary>
public class SerialStatusResponse
{
    /// <summary>
    /// 连接 ID
    /// </summary>
    public string ConnectionId { get; set; } = "";
    
    /// <summary>
    /// 是否连接
    /// </summary>
    public bool IsConnected { get; set; }
    
    /// <summary>
    /// 串口名称
    /// </summary>
    public string PortName { get; set; } = "";
    
    /// <summary>
    /// 波特率
    /// </summary>
    public int BaudRate { get; set; }
    
    /// <summary>
    /// 协议类型
    /// </summary>
    public string Protocol { get; set; } = "";
    
    /// <summary>
    /// 从站地址
    /// </summary>
    public byte StationId { get; set; }
}
