// MgTx - 猛攻通讯
// Copyright (c) 2026 MgTx Project
// Licensed under MIT License
// https://github.com/mgtx/mgtx-service

namespace MgTx.Plc.Enums;

/// <summary>
/// 设备类型枚举，用于标识各种工业自动化设备
/// </summary>
/// <remarks>
/// 支持 PLC、运动控制器、IO 模块等多种工业设备类型
/// </remarks>
public enum DeviceType
{
    /// <summary>西门子 S7-200 Smart</summary>
    SiemensS7200Smart,
    
    /// <summary>西门子 S7-200</summary>
    SiemensS7200,
    
    /// <summary>西门子 S7-300</summary>
    SiemensS7300,
    
    /// <summary>西门子 S7-400</summary>
    SiemensS7400,
    
    /// <summary>西门子 S7-1200</summary>
    SiemensS71200,
    
    /// <summary>西门子 S7-1500</summary>
    SiemensS71500,
    
    /// <summary>三菱 FX 系列</summary>
    MitsubishiFx,
    
    /// <summary>三菱 Q 系列</summary>
    MitsubishiQ,
    
    /// <summary>三菱 L 系列</summary>
    MitsubishiL,
    
    /// <summary>三菱 A 系列</summary>
    MitsubishiA,
    
    /// <summary>欧姆龙 CP 系列</summary>
    OmronCp,
    
    /// <summary>欧姆龙 CJ 系列</summary>
    OmronCj,
    
    /// <summary>欧姆龙 CS 系列</summary>
    OmronCs,
    
    /// <summary>倍福 CX 系列</summary>
    BeckhoffCx,
    
    /// <summary>罗克韦尔 AB 系列</summary>
    AllenBradleyAb,
    
    /// <summary>倍福 ADS 设备</summary>
    BeckhoffAds,
    
    /// <summary>Modbus TCP 设备</summary>
    ModbusTcp,
    
    /// <summary>Modbus RTU 设备</summary>
    ModbusRtu,
    
    /// <summary>Modbus ASCII 设备</summary>
    ModbusAscii,
    
    /// <summary>其他设备类型</summary>
    Other
}
