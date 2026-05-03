// MgTx - 猛攻通讯
// Copyright (c) 2026 MgTx Project
// Licensed under MIT License
// https://github.com/mgtx/mgtx-service

using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace MgTx.Core.Net;

/// <summary>
/// 网络通信基类，提供基础的 TCP 连接和数据收发功能
/// </summary>
/// <remarks>
/// 所有网络相关类的基类，封装了 Socket 操作和日志记录
/// </remarks>
public abstract class NetworkBase
{
    /// <summary>
    /// 日志记录器
    /// </summary>
    protected readonly ILogger? Logger;
    
    /// <summary>
    /// Socket 连接对象
    /// </summary>
    protected Socket? Socket;
    
    /// <summary>
    /// 获取当前连接状态
    /// </summary>
    public bool IsConnected => Socket?.Connected ?? false;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger">可选的日志记录器</param>
    protected NetworkBase(ILogger? logger = null)
    {
        Logger = logger;
    }

    /// <summary>
    /// 建立 TCP 连接
    /// </summary>
    /// <param name="ipAddress">目标 IP 地址</param>
    /// <param name="port">目标端口号</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>连接操作结果</returns>
    public async Task<OperateResult> ConnectAsync(string ipAddress, int port, CancellationToken cancellationToken = default)
    {
        try
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            
            await Socket.ConnectAsync(ipAddress, port, cancellationToken);
            Logger?.LogInformation("网络连接成功: {Ip}:{Port}", ipAddress, port);
            
            return OperateResult.Success();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "网络连接失败: {Ip}:{Port}", ipAddress, port);
            return OperateResult.Fail(ex.Message);
        }
    }

    /// <summary>
    /// 发送数据到服务器
    /// </summary>
    /// <param name="data">要发送的字节数据</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>发送操作结果，包含实际发送的字节数</returns>
    public async Task<OperateResult<int>> SendAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        if (Socket == null || !IsConnected)
        {
            return OperateResult<int>.Fail("连接未建立");
        }

        try
        {
            int sent = await Socket.SendAsync(data, SocketFlags.None, cancellationToken);
            return OperateResult<int>.Success(sent);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "发送数据失败");
            return OperateResult<int>.Fail(ex.Message);
        }
    }

    /// <summary>
    /// 从服务器接收数据
    /// </summary>
    /// <param name="length">期望接收的字节数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>接收操作结果，包含接收到的字节数据</returns>
    public async Task<OperateResult<byte[]>> ReceiveAsync(int length, CancellationToken cancellationToken = default)
    {
        if (Socket == null || !IsConnected)
        {
            return OperateResult<byte[]>.Fail("连接未建立");
        }

        try
        {
            byte[] buffer = new byte[length];
            int received = await Socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);

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
            Logger?.LogError(ex, "接收数据失败");
            return OperateResult<byte[]>.Fail(ex.Message);
        }
    }

    /// <summary>
    /// 断开当前连接
    /// </summary>
    /// <remarks>
    /// 会先尝试优雅关闭连接，然后释放 Socket 资源
    /// </remarks>
    public virtual void Disconnect()
    {
        if (Socket != null)
        {
            try
            {
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Close();
                Socket.Dispose();
                Socket = null;
                Logger?.LogInformation("连接已断开");
            }
            catch (Exception ex)
            {
                Logger?.LogWarning(ex, "断开连接时出错");
            }
        }
    }
}
