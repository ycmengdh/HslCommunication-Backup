// MgTx - 猛攻通讯
// Copyright (c) 2026 MgTx Project
// Licensed under MIT License
// https://github.com/mgtx/mgtx-service

using System.Text;
using Microsoft.Extensions.Logging;
using MgTx.Core.Transfer;
using MgTx.Core.Types;

namespace MgTx.Core.Net;

public abstract class NetworkDeviceBase : NetworkBase
{
    protected IByteTransform ByteTransform { get; set; } = new RegularByteTransform();
    public string IpAddress { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 502;
    protected int WordLength { get; set; } = 1;

    protected NetworkDeviceBase(ILogger? logger = null) : base(logger)
    {
    }

    public virtual async Task<OperateResult> ConnectServerAsync(CancellationToken cancellationToken = default)
    {
        var connectResult = await ConnectAsync(IpAddress, Port, cancellationToken);
        if (!connectResult.IsSuccess)
        {
            return connectResult;
        }

        var initResult = await InitializationOnConnectAsync(cancellationToken);
        if (!initResult.IsSuccess)
        {
            Disconnect();
            return initResult;
        }

        return OperateResult.Success();
    }

    protected virtual Task<OperateResult> InitializationOnConnectAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(OperateResult.Success());
    }

    protected virtual Task<OperateResult> ExtraOnDisconnectAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(OperateResult.Success());
    }

    public override void Disconnect()
    {
        ExtraOnDisconnectAsync().Wait();
        base.Disconnect();
    }

    protected virtual async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(byte[] send, CancellationToken cancellationToken = default)
    {
        if (Socket == null || !IsConnected)
        {
            return OperateResult<byte[]>.Fail("未连接", 1003);
        }

        var sendResult = await SendAsync(send, cancellationToken);
        if (!sendResult.IsSuccess)
        {
            return OperateResult<byte[]>.Fail(sendResult.Message ?? "发送失败", sendResult.ErrorCode);
        }

        var receiveResult = await ReceiveAsync(1024, cancellationToken);
        return receiveResult;
    }

    public abstract Task<OperateResult<byte[]>> ReadAsync(string address, ushort length, CancellationToken cancellationToken = default);

    public virtual async Task<OperateResult<bool>> ReadBoolAsync(string address, CancellationToken cancellationToken = default)
    {
        var readResult = await ReadAsync(address, 1, cancellationToken);
        if (!readResult.IsSuccess)
        {
            return OperateResult<bool>.Fail(readResult.Message ?? "读取失败", readResult.ErrorCode);
        }

        if (readResult.Content == null)
        {
            return OperateResult<bool>.Fail("内容为空");
        }
        
        return OperateResult<bool>.Success(ByteTransform.TransBool(readResult.Content, 0));
    }

    public virtual async Task<OperateResult<short[]>> ReadInt16Async(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var readResult = await ReadAsync(address, length, cancellationToken);
        if (!readResult.IsSuccess)
        {
            return OperateResult<short[]>.Fail(readResult.Message ?? "读取失败", readResult.ErrorCode);
        }

        if (readResult.Content == null)
        {
            return OperateResult<short[]>.Fail("内容为空");
        }
        
        return OperateResult<short[]>.Success(ByteTransform.TransInt16Array(readResult.Content, 0, length));
    }

    public virtual async Task<OperateResult<ushort[]>> ReadUInt16Async(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var readResult = await ReadAsync(address, length, cancellationToken);
        if (!readResult.IsSuccess)
        {
            return OperateResult<ushort[]>.Fail(readResult.Message ?? "读取失败", readResult.ErrorCode);
        }

        if (readResult.Content == null)
        {
            return OperateResult<ushort[]>.Fail("内容为空");
        }
        
        return OperateResult<ushort[]>.Success(ByteTransform.TransUInt16Array(readResult.Content, 0, length));
    }

    public virtual async Task<OperateResult<int[]>> ReadInt32Async(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var readResult = await ReadAsync(address, length, cancellationToken);
        if (!readResult.IsSuccess)
        {
            return OperateResult<int[]>.Fail(readResult.Message ?? "读取失败", readResult.ErrorCode);
        }

        if (readResult.Content == null)
        {
            return OperateResult<int[]>.Fail("内容为空");
        }
        
        return OperateResult<int[]>.Success(ByteTransform.TransInt32Array(readResult.Content, 0, length));
    }

    public virtual async Task<OperateResult<uint[]>> ReadUInt32Async(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var readResult = await ReadAsync(address, length, cancellationToken);
        if (!readResult.IsSuccess)
        {
            return OperateResult<uint[]>.Fail(readResult.Message ?? "读取失败", readResult.ErrorCode);
        }

        if (readResult.Content == null)
        {
            return OperateResult<uint[]>.Fail("内容为空");
        }
        
        return OperateResult<uint[]>.Success(ByteTransform.TransUInt32Array(readResult.Content, 0, length));
    }

    public virtual async Task<OperateResult<float[]>> ReadFloatAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var readResult = await ReadAsync(address, length, cancellationToken);
        if (!readResult.IsSuccess)
        {
            return OperateResult<float[]>.Fail(readResult.Message ?? "读取失败", readResult.ErrorCode);
        }

        if (readResult.Content == null)
        {
            return OperateResult<float[]>.Fail("内容为空");
        }
        
        return OperateResult<float[]>.Success(ByteTransform.TransSingleArray(readResult.Content, 0, length));
    }

    public virtual async Task<OperateResult<string>> ReadStringAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        var readResult = await ReadAsync(address, length, cancellationToken);
        if (!readResult.IsSuccess)
        {
            return OperateResult<string>.Fail(readResult.Message ?? "读取失败", readResult.ErrorCode);
        }

        if (readResult.Content == null)
        {
            return OperateResult<string>.Fail("内容为空");
        }
        
        return OperateResult<string>.Success(Encoding.UTF8.GetString(readResult.Content));
    }

    public abstract Task<OperateResult> WriteAsync(string address, byte[] data, CancellationToken cancellationToken = default);

    public virtual Task<OperateResult> WriteAsync(string address, short[] values, CancellationToken cancellationToken = default)
    {
        return WriteAsync(address, ByteTransform.GetBytes(values), cancellationToken);
    }

    public virtual Task<OperateResult> WriteAsync(string address, ushort[] values, CancellationToken cancellationToken = default)
    {
        return WriteAsync(address, ByteTransform.GetBytes(values), cancellationToken);
    }

    public virtual Task<OperateResult> WriteAsync(string address, int[] values, CancellationToken cancellationToken = default)
    {
        return WriteAsync(address, ByteTransform.GetBytes(values), cancellationToken);
    }

    public virtual Task<OperateResult> WriteAsync(string address, uint[] values, CancellationToken cancellationToken = default)
    {
        return WriteAsync(address, ByteTransform.GetBytes(values), cancellationToken);
    }

    public virtual Task<OperateResult> WriteAsync(string address, float[] values, CancellationToken cancellationToken = default)
    {
        return WriteAsync(address, ByteTransform.GetBytes(values), cancellationToken);
    }

    public virtual Task<OperateResult> WriteAsync(string address, string value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(address, Encoding.UTF8.GetBytes(value), cancellationToken);
    }
}
