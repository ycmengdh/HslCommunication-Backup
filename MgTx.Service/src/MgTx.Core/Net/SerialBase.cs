// MgTx - 猛攻通讯
// Copyright (c) 2026 MgTx Project
// Licensed under MIT License
// https://github.com/mgtx/mgtx-service

using System.IO.Ports;
using System.Text;
using Microsoft.Extensions.Logging;

namespace MgTx.Core.Net;

/// <summary>
/// 串口通信基类，提供基础的串口连接和数据收发功能
/// </summary>
/// <remarks>
/// 封装了 SerialPort 操作和日志记录
/// </remarks>
public abstract class SerialBase
{
    /// <summary>
    /// 日志记录器
    /// </summary>
    protected readonly ILogger? Logger;
    
    /// <summary>
    /// 串口对象
    /// </summary>
    protected SerialPort? SerialPort;

    /// <summary>
    /// 获取当前串口连接状态
    /// </summary>
    public bool IsConnected => SerialPort?.IsOpen ?? false;

    /// <summary>
    /// 获取或设置串口名称（如 COM1、/dev/ttyS0）
    /// </summary>
    public string PortName { get; set; } = "COM1";

    /// <summary>
    /// 获取或设置波特率
    /// </summary>
    public int BaudRate { get; set; } = 9600;

    /// <summary>
    /// 获取或设置数据位
    /// </summary>
    public int DataBits { get; set; } = 8;

    /// <summary>
    /// 获取或设置停止位
    /// </summary>
    public StopBits StopBits { get; set; } = StopBits.One;

    /// <summary>
    /// 获取或设置校验位
    /// </summary>
    public Parity Parity { get; set; } = Parity.None;

    /// <summary>
    /// 获取或设置接收超时时间（毫秒）
    /// </summary>
    public int ReceiveTimeout { get; set; } = 5000;

    /// <summary>
    /// 获取或设置发送超时时间（毫秒）
    /// </summary>
    public int SendTimeout { get; set; } = 5000;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger">可选的日志记录器</param>
    protected SerialBase(ILogger? logger = null)
    {
        Logger = logger;
    }

    /// <summary>
    /// 打开串口连接
    /// </summary>
    /// <returns>连接操作结果</returns>
    public virtual OperateResult Open()
    {
        try
        {
            if (SerialPort != null && SerialPort.IsOpen)
            {
                return OperateResult.Fail("串口已打开");
            }

            SerialPort = new SerialPort(
                PortName,
                BaudRate,
                Parity,
                DataBits,
                StopBits
            );
            SerialPort.ReadTimeout = ReceiveTimeout;
            SerialPort.WriteTimeout = SendTimeout;
            
            SerialPort.Open();
            Logger?.LogInformation("串口打开成功: {PortName}, {BaudRate}bps", PortName, BaudRate);
            
            return OperateResult.Success();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "串口打开失败: {PortName}", PortName);
            return OperateResult.Fail(ex.Message);
        }
    }

    /// <summary>
    /// 关闭串口连接
    /// </summary>
    public virtual void Close()
    {
        if (SerialPort != null && SerialPort.IsOpen)
        {
            try
            {
                SerialPort.Close();
                SerialPort.Dispose();
                SerialPort = null;
                Logger?.LogInformation("串口已关闭: {PortName}", PortName);
            }
            catch (Exception ex)
            {
                Logger?.LogWarning(ex, "关闭串口时出错");
            }
        }
    }

    /// <summary>
    /// 发送数据到串口
    /// </summary>
    /// <param name="data">要发送的字节数据</param>
    /// <returns>发送操作结果</returns>
    public virtual OperateResult Send(byte[] data)
    {
        if (SerialPort == null || !SerialPort.IsOpen)
        {
            return OperateResult.Fail("串口未打开");
        }

        try
        {
            SerialPort.Write(data, 0, data.Length);
            return OperateResult.Success();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "串口发送数据失败");
            return OperateResult.Fail(ex.Message);
        }
    }

    /// <summary>
    /// 从串口接收指定长度的数据
    /// </summary>
    /// <param name="length">期望接收的字节数</param>
    /// <returns>接收操作结果，包含接收到的数据</returns>
    public virtual OperateResult<byte[]> Receive(int length)
    {
        if (SerialPort == null || !SerialPort.IsOpen)
        {
            return OperateResult<byte[]>.Fail("串口未打开");
        }

        try
        {
            byte[] buffer = new byte[length];
            int received = SerialPort.Read(buffer, 0, length);

            if (received < length)
            {
                byte[] actual = new byte[received];
                Array.Copy(buffer, actual, received);
                return OperateResult<byte[]>.Success(actual);
            }

            return OperateResult<byte[]>.Success(buffer);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "串口接收数据失败");
            return OperateResult<byte[]>.Fail(ex.Message);
        }
    }

    /// <summary>
    /// 读取串口输入缓冲区中所有可用的数据
    /// </summary>
    /// <returns>接收到的数据</returns>
    public virtual OperateResult<byte[]> ReadExisting()
    {
        if (SerialPort == null || !SerialPort.IsOpen)
        {
            return OperateResult<byte[]>.Fail("串口未打开");
        }

        try
        {
            string data = SerialPort.ReadExisting();
            return OperateResult<byte[]>.Success(Encoding.UTF8.GetBytes(data));
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "读取串口缓冲区失败");
            return OperateResult<byte[]>.Fail(ex.Message);
        }
    }

    /// <summary>
    /// 获取可用的串口列表
    /// </summary>
    /// <returns>串口名称数组</returns>
    public static string[] GetPortNames()
    {
        return SerialPort.GetPortNames();
    }

    /// <summary>
    /// 清理串口输入缓冲区
    /// </summary>
    public void DiscardInBuffer()
    {
        SerialPort?.DiscardInBuffer();
    }

    /// <summary>
    /// 清理串口输出缓冲区
    /// </summary>
    public void DiscardOutBuffer()
    {
        SerialPort?.DiscardOutBuffer();
    }
}
