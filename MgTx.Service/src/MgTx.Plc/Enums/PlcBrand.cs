// MgTx - 猛攻通讯
// Copyright (c) 2026 MgTx Project
// Licensed under MIT License
// https://github.com/mgtx/mgtx-service

namespace MgTx.Plc.Enums;

/// <summary>
/// PLC 品牌枚举，支持市场上主流的工业自动化设备品牌
/// </summary>
/// <remarks>
/// 覆盖了西门子、三菱、欧姆龙、倍福等国际知名品牌
/// </remarks>
public enum PlcBrand
{
    /// <summary>西门子 Siemens</summary>
    Siemens,
    
    /// <summary>三菱 Mitsubishi</summary>
    Mitsubishi,
    
    /// <summary>欧姆龙 Omron</summary>
    Omron,
    
    /// <summary>欧姆龙 Fins 协议</summary>
    OmronFins,
    
    /// <summary>倍福 Beckhoff</summary>
    Beckhoff,
    
    /// <summary>罗克韦尔 Allen-Bradley</summary>
    AllenBradley,
    
    /// <summary>施耐德 Schneider</summary>
    Schneider,
    
    /// <summary>ABB</summary>
    ABB,
    
    /// <summary>台达 Delta</summary>
    Delta,
    
    /// <summary>信捷 Xinje</summary>
    Xinje,
    
    /// <summary>汇川 Inovance</summary>
    Inovance,
    
    /// <summary>永宏 Fatek</summary>
    Fatek,
    
    /// <summary>富士 Fuji</summary>
    Fuji,
    
    /// <summary>松下 Panasonic</summary>
    Panasonic,
    
    /// <summary>基恩士 Keyence</summary>
    Keyence,
    
    /// <summary>欧姆龙 HostLink 协议</summary>
    OmronHostLink,
    
    /// <summary>欧姆龙 Mewtocol 协议</summary>
    OmronMewtocol,
    
    /// <summary>台达 Modbus 协议</summary>
    DeltaModbus,
    
    /// <summary>信捷 Modbus 协议</summary>
    XinjeModbus,
    
    /// <summary>汇川 Modbus 协议</summary>
    InovanceModbus,
    
    /// <summary>永宏 Modbus 协议</summary>
    FatekModbus,
    
    /// <summary>Profinet 协议</summary>
    Profinet,
    
    /// <summary>倍福 ADS 协议</summary>
    BeckhoffAds,
    
    /// <summary>AB CIP 协议</summary>
    AllenBradleyCip,
    
    /// <summary>施耐德 Modbus 协议</summary>
    SchneiderModbus,
    
    /// <summary>Modbus TCP 协议</summary>
    ModbusTcp,
    
    /// <summary>Modbus RTU 协议</summary>
    ModbusRtu,
    
    /// <summary>Modbus ASCII 协议</summary>
    ModbusAscii,
    
    /// <summary>其他品牌</summary>
    Other
}
