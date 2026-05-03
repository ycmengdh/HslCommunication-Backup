// MgTx - 猛攻通讯
// Copyright (c) 2026 MgTx Project
// Licensed under MIT License
// https://github.com/mgtx/mgtx-service

namespace MgTx.Core.Types;

/// <summary>
/// 数据类型枚举，用于标识 PLC 数据类型
/// </summary>
/// <remarks>
/// 支持工业自动化中常用的各种数据类型
/// </remarks>
public enum DataType
{
    /// <summary>布尔类型（Bit）</summary>
    Bool,
    
    /// <summary>字节类型（8位）</summary>
    Byte,
    
    /// <summary>短整型（16位有符号）</summary>
    Int16,
    
    /// <summary>无符号短整型（16位）</summary>
    UInt16,
    
    /// <summary>整型（32位有符号）</summary>
    Int32,
    
    /// <summary>无符号整型（32位）</summary>
    UInt32,
    
    /// <summary>长整型（64位有符号）</summary>
    Int64,
    
    /// <summary>无符号长整型（64位）</summary>
    UInt64,
    
    /// <summary>浮点型（32位）</summary>
    Float,
    
    /// <summary>双精度浮点型（64位）</summary>
    Double,
    
    /// <summary>字符串类型</summary>
    String,
    
    /// <summary>二进制数据类型</summary>
    Binary
}
