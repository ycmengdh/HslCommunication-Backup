// MgTx - 猛攻通讯
// Copyright (c) 2026 MgTx Project
// Licensed under MIT License
// https://github.com/mgtx/mgtx-service

using MgTx.Core.Net;
using MgTx.Core.Transfer;
using MgTx.Core.Types;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace MgTx.Plc.Modbus;

/// <summary>
/// Modbus RTU 通信类，用于串口连接的 Modbus 设备
/// </summary>
/// <remarks>
/// 支持 Modbus RTU 协议的各种操作，包括线圈、寄存器的读写
/// </remarks>
public class ModbusRtuNet : SerialBase
{
    private byte _stationId = 1;

    /// <summary>
    /// 获取或设置从站地址
    /// </summary>
    public byte StationId
    {
        get => _stationId;
        set => _stationId = value;
    }

    /// <summary>
    /// 获取字节转换对象
    /// </summary>
    protected IByteTransform ByteTransform { get; set; } = new RegularByteTransform();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger">可选的日志记录器</param>
    public ModbusRtuNet(ILogger? logger = null) : base(logger)
    {
        PortName = "COM1";
        BaudRate = 9600;
        DataBits = 8;
        StopBits = System.IO.Ports.StopBits.One;
        Parity = System.IO.Ports.Parity.None;
    }

    /// <summary>
    /// 计算 CRC 校验码
    /// </summary>
    /// <param name="data">要校验的数据</param>
    /// <returns>CRC 校验码</returns>
    public static ushort CalculateCRC(byte[] data)
    {
        ushort crc = 0xFFFF;
        
        for (int i = 0; i < data.Length; i++)
        {
            crc ^= data[i];
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x0001) != 0)
                {
                    crc >>= 1;
                    crc ^= 0xA001;
                }
                else
                {
                    crc >>= 1;
                }
            }
        }
        
        return crc;
    }

    /// <summary>
    /// 添加 CRC 校验码到命令末尾
    /// </summary>
    /// <param name="command">原始命令</param>
    /// <returns>带 CRC 的完整命令</returns>
    public static byte[] AddCRC(byte[] command)
    {
        ushort crc = CalculateCRC(command);
        byte[] result = new byte[command.Length + 2];
        Array.Copy(command, result, command.Length);
        result[result.Length - 2] = (byte)(crc & 0xFF);
        result[result.Length - 1] = (byte)(crc >> 8);
        return result;
    }

    /// <summary>
    /// 验证响应数据的 CRC 校验码
    /// </summary>
    /// <param name="response">响应数据</param>
    /// <returns>校验是否通过</returns>
    public static bool VerifyCRC(byte[] response)
    {
        if (response.Length < 2) return false;
        
        ushort crc = CalculateCRC(response.Take(response.Length - 2).ToArray());
        ushort receivedCrc = (ushort)((response[response.Length - 1] << 8) | response[response.Length - 2]);
        
        return crc == receivedCrc;
    }

    /// <summary>
    /// 发送 Modbus RTU 命令并接收响应
    /// </summary>
    /// <param name="command">命令数据（不含 CRC）</param>
    /// <param name="expectedLength">期望的响应长度</param>
    /// <returns>响应数据</returns>
    protected virtual OperateResult<byte[]> SendAndReceive(byte[] command, int expectedLength = 256)
    {
        try
        {
            byte[] fullCommand = AddCRC(command);
            Logger?.LogDebug("发送 Modbus RTU 命令: {Command}", BitConverter.ToString(fullCommand).Replace("-", " "));
            
            var sendResult = Send(fullCommand);
            if (!sendResult.IsSuccess)
            {
                return OperateResult<byte[]>.Fail(sendResult.Message);
            }
            
            Thread.Sleep(50);
            
            var receiveResult = ReadExisting();
            if (!receiveResult.IsSuccess)
            {
                return receiveResult;
            }
            
            Logger?.LogDebug("接收 Modbus RTU 响应: {Response}", BitConverter.ToString(receiveResult.Content!).Replace("-", " "));
            
            if (!VerifyCRC(receiveResult.Content!))
            {
                return OperateResult<byte[]>.Fail("CRC 校验失败");
            }
            
            return OperateResult<byte[]>.Success(receiveResult.Content!.Take(receiveResult.Content.Length - 2).ToArray());
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Modbus RTU 通信失败");
            return OperateResult<byte[]>.Fail(ex.Message);
        }
    }

    /// <summary>
    /// 读取线圈（功能码 01）
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="length">读取数量</param>
    /// <returns>读取结果</returns>
    public OperateResult<bool[]> ReadCoils(ushort address, ushort length)
    {
        byte[] command = BuildReadCommand(0x01, address, length);
        var result = SendAndReceive(command);
        
        if (!result.IsSuccess)
        {
            return OperateResult<bool[]>.Fail(result.Message);
        }
        
        return ParseBoolResponse(result.Content!);
    }

    /// <summary>
    /// 读取离散输入（功能码 02）
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="length">读取数量</param>
    /// <returns>读取结果</returns>
    public OperateResult<bool[]> ReadDiscreteInputs(ushort address, ushort length)
    {
        byte[] command = BuildReadCommand(0x02, address, length);
        var result = SendAndReceive(command);
        
        if (!result.IsSuccess)
        {
            return OperateResult<bool[]>.Fail(result.Message);
        }
        
        return ParseBoolResponse(result.Content!);
    }

    /// <summary>
    /// 读取保持寄存器（功能码 03）
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="length">读取数量</param>
    /// <returns>读取结果</returns>
    public OperateResult<ushort[]> ReadHoldingRegisters(ushort address, ushort length)
    {
        byte[] command = BuildReadCommand(0x03, address, length);
        var result = SendAndReceive(command);
        
        if (!result.IsSuccess)
        {
            return OperateResult<ushort[]>.Fail(result.Message);
        }
        
        return ParseRegisterResponse(result.Content!);
    }

    /// <summary>
    /// 读取输入寄存器（功能码 04）
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="length">读取数量</param>
    /// <returns>读取结果</returns>
    public OperateResult<ushort[]> ReadInputRegisters(ushort address, ushort length)
    {
        byte[] command = BuildReadCommand(0x04, address, length);
        var result = SendAndReceive(command);
        
        if (!result.IsSuccess)
        {
            return OperateResult<ushort[]>.Fail(result.Message);
        }
        
        return ParseRegisterResponse(result.Content!);
    }

    /// <summary>
    /// 写入单个线圈（功能码 05）
    /// </summary>
    /// <param name="address">线圈地址</param>
    /// <param name="value">写入值</param>
    /// <returns>操作结果</returns>
    public OperateResult WriteSingleCoil(ushort address, bool value)
    {
        byte[] command = new byte[8];
        command[0] = _stationId;
        command[1] = 0x05;
        ByteTransform.GetBytes(address).CopyTo(command, 2);
        command[4] = value ? (byte)0xFF : (byte)0x00;
        command[5] = 0x00;
        
        var result = SendAndReceive(command);
        return result.IsSuccess ? OperateResult.Success() : OperateResult.Fail(result.Message);
    }

    /// <summary>
    /// 写入单个寄存器（功能码 06）
    /// </summary>
    /// <param name="address">寄存器地址</param>
    /// <param name="value">写入值</param>
    /// <returns>操作结果</returns>
    public OperateResult WriteSingleRegister(ushort address, ushort value)
    {
        byte[] command = new byte[8];
        command[0] = _stationId;
        command[1] = 0x06;
        ByteTransform.GetBytes(address).CopyTo(command, 2);
        ByteTransform.GetBytes(value).CopyTo(command, 4);
        
        var result = SendAndReceive(command);
        return result.IsSuccess ? OperateResult.Success() : OperateResult.Fail(result.Message);
    }

    /// <summary>
    /// 写入多个线圈（功能码 15）
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="values">写入值</param>
    /// <returns>操作结果</returns>
    public OperateResult WriteMultipleCoils(ushort address, bool[] values)
    {
        int byteCount = (values.Length + 7) / 8;
        byte[] command = new byte[9 + byteCount];
        command[0] = _stationId;
        command[1] = 0x0F;
        ByteTransform.GetBytes(address).CopyTo(command, 2);
        ByteTransform.GetBytes((ushort)values.Length).CopyTo(command, 4);
        command[6] = (byte)byteCount;
        
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i])
            {
                command[7 + i / 8] |= (byte)(1 << (i % 8));
            }
        }
        
        var result = SendAndReceive(command);
        return result.IsSuccess ? OperateResult.Success() : OperateResult.Fail(result.Message);
    }

    /// <summary>
    /// 写入多个寄存器（功能码 16）
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="values">写入值</param>
    /// <returns>操作结果</returns>
    public OperateResult WriteMultipleRegisters(ushort address, ushort[] values)
    {
        byte[] command = new byte[9 + values.Length * 2];
        command[0] = _stationId;
        command[1] = 0x10;
        ByteTransform.GetBytes(address).CopyTo(command, 2);
        ByteTransform.GetBytes((ushort)values.Length).CopyTo(command, 4);
        command[6] = (byte)(values.Length * 2);
        
        for (int i = 0; i < values.Length; i++)
        {
            ByteTransform.GetBytes(values[i]).CopyTo(command, 7 + i * 2);
        }
        
        var result = SendAndReceive(command);
        return result.IsSuccess ? OperateResult.Success() : OperateResult.Fail(result.Message);
    }

    private byte[] BuildReadCommand(byte functionCode, ushort address, ushort length)
    {
        byte[] command = new byte[8];
        command[0] = _stationId;
        command[1] = functionCode;
        ByteTransform.GetBytes(address).CopyTo(command, 2);
        ByteTransform.GetBytes(length).CopyTo(command, 4);
        return command;
    }

    private OperateResult<bool[]> ParseBoolResponse(byte[] response)
    {
        if (response.Length < 3) return OperateResult<bool[]>.Fail("响应数据过短");
        
        int byteCount = response[2];
        bool[] values = new bool[byteCount * 8];
        
        for (int i = 0; i < byteCount; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                int index = i * 8 + j;
                if (index < values.Length)
                {
                    values[index] = (response[3 + i] & (1 << j)) != 0;
                }
            }
        }
        
        return OperateResult<bool[]>.Success(values);
    }

    private OperateResult<ushort[]> ParseRegisterResponse(byte[] response)
    {
        if (response.Length < 3) return OperateResult<ushort[]>.Fail("响应数据过短");
        
        int byteCount = response[2];
        int registerCount = byteCount / 2;
        ushort[] values = new ushort[registerCount];
        
        for (int i = 0; i < registerCount; i++)
        {
            values[i] = ByteTransform.TransUInt16(response, 3 + i * 2);
        }
        
        return OperateResult<ushort[]>.Success(values);
    }
}
