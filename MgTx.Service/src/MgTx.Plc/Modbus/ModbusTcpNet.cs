using MgTx.Core.Net;
using MgTx.Core.Transfer;
using MgTx.Core.Types;
using Microsoft.Extensions.Logging;

namespace MgTx.Plc.Modbus;

public class ModbusTcpNet : NetworkDeviceBase
{
    private byte _unitIdentifier = 1;
    private ushort _transactionId = 0;
    private int _connectTimeout = 5000;
    private int _receiveTimeout = 3000;
    private int _sendTimeout = 3000;

    public ModbusTcpNet(ILogger? logger = null) : base(logger)
    {
        Port = 502;
        WordLength = 1;
        ByteTransform = new RegularByteTransform();
    }

    public byte UnitIdentifier
    {
        get => _unitIdentifier;
        set => _unitIdentifier = value;
    }

    public int ConnectTimeout
    {
        get => _connectTimeout;
        set => _connectTimeout = value;
    }

    public int ReceiveTimeout
    {
        get => _receiveTimeout;
        set => _receiveTimeout = value;
    }

    public int SendTimeout
    {
        get => _sendTimeout;
        set => _sendTimeout = value;
    }

    public async Task<OperateResult> ConnectServerAsync(string host, int port = 502, byte unitIdentifier = 1, CancellationToken cancellationToken = default)
    {
        IpAddress = host;
        Port = port;
        _unitIdentifier = unitIdentifier;
        return await ConnectServerAsync(cancellationToken);
    }

    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        if (ushort.TryParse(address, out ushort startAddress))
        {
            var result = await ReadHoldingRegistersAsync(startAddress, length, cancellationToken);
            if (!result.IsSuccess) return OperateResult<byte[]>.Fail(result.Message, result.ErrorCode);

            byte[] bytes = new byte[length * 2];
            for (int i = 0; i < length; i++)
            {
                byte[] registerBytes = ByteTransform.GetBytes(result.Content![i]);
                Array.Copy(registerBytes, 0, bytes, i * 2, 2);
            }
            return OperateResult<byte[]>.Success(bytes);
        }
        return OperateResult<byte[]>.Fail("地址格式错误");
    }

    public override async Task<OperateResult> WriteAsync(string address, byte[] data, CancellationToken cancellationToken = default)
    {
        if (ushort.TryParse(address, out ushort startAddress))
        {
            int registerCount = (data.Length + 1) / 2;
            ushort[] registers = new ushort[registerCount];

            for (int i = 0; i < registerCount; i++)
            {
                if (i * 2 + 1 < data.Length)
                {
                    registers[i] = ByteTransform.TransUInt16(data, i * 2);
                }
                else
                {
                    registers[i] = data[i * 2];
                }
            }

            return await WriteMultipleRegistersAsync(startAddress, registers, cancellationToken);
        }
        return OperateResult.Fail("地址格式错误");
    }

    public async Task<OperateResult<ushort[]>> ReadHoldingRegistersAsync(ushort startAddress, ushort length, CancellationToken cancellationToken = default)
    {
        try
        {
            byte[] command = BuildReadCommand(3, startAddress, length);
            var readResult = await ReadFromCoreServerAsync(command, cancellationToken);
            if (!readResult.IsSuccess) return OperateResult<ushort[]>.Fail(readResult.Message, readResult.ErrorCode);

            if (readResult.Content!.Length < 9) return OperateResult<ushort[]>.Fail("响应数据过短");

            ushort[] values = ByteTransform.TransUInt16Array(readResult.Content, 9, length);
            return OperateResult<ushort[]>.Success(values);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "读取保持寄存器失败");
            return OperateResult<ushort[]>.Fail(ex.Message);
        }
    }

    public async Task<OperateResult<ushort[]>> ReadInputRegistersAsync(ushort startAddress, ushort length, CancellationToken cancellationToken = default)
    {
        try
        {
            byte[] command = BuildReadCommand(4, startAddress, length);
            var readResult = await ReadFromCoreServerAsync(command, cancellationToken);
            if (!readResult.IsSuccess) return OperateResult<ushort[]>.Fail(readResult.Message, readResult.ErrorCode);

            if (readResult.Content!.Length < 9) return OperateResult<ushort[]>.Fail("响应数据过短");

            ushort[] values = ByteTransform.TransUInt16Array(readResult.Content, 9, length);
            return OperateResult<ushort[]>.Success(values);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "读取输入寄存器失败");
            return OperateResult<ushort[]>.Fail(ex.Message);
        }
    }

    public async Task<OperateResult<bool[]>> ReadCoilsAsync(ushort startAddress, ushort length, CancellationToken cancellationToken = default)
    {
        try
        {
            byte[] command = BuildReadCommand(1, startAddress, length);
            var readResult = await ReadFromCoreServerAsync(command, cancellationToken);
            if (!readResult.IsSuccess) return OperateResult<bool[]>.Fail(readResult.Message, readResult.ErrorCode);

            if (readResult.Content!.Length < 9) return OperateResult<bool[]>.Fail("响应数据过短");

            bool[] values = new bool[length];
            for (int i = 0; i < length; i++)
            {
                int byteIndex = 9 + i / 8;
                int bitIndex = i % 8;
                if (byteIndex < readResult.Content.Length)
                {
                    values[i] = (readResult.Content[byteIndex] & (1 << bitIndex)) != 0;
                }
            }

            return OperateResult<bool[]>.Success(values);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "读取线圈失败");
            return OperateResult<bool[]>.Fail(ex.Message);
        }
    }

    public async Task<OperateResult<bool[]>> ReadDiscreteInputsAsync(ushort startAddress, ushort length, CancellationToken cancellationToken = default)
    {
        try
        {
            byte[] command = BuildReadCommand(2, startAddress, length);
            var readResult = await ReadFromCoreServerAsync(command, cancellationToken);
            if (!readResult.IsSuccess) return OperateResult<bool[]>.Fail(readResult.Message, readResult.ErrorCode);

            if (readResult.Content!.Length < 9) return OperateResult<bool[]>.Fail("响应数据过短");

            bool[] values = new bool[length];
            for (int i = 0; i < length; i++)
            {
                int byteIndex = 9 + i / 8;
                int bitIndex = i % 8;
                if (byteIndex < readResult.Content.Length)
                {
                    values[i] = (readResult.Content[byteIndex] & (1 << bitIndex)) != 0;
                }
            }

            return OperateResult<bool[]>.Success(values);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "读取离散输入失败");
            return OperateResult<bool[]>.Fail(ex.Message);
        }
    }

    public async Task<OperateResult> WriteSingleRegisterAsync(ushort address, ushort value, CancellationToken cancellationToken = default)
    {
        try
        {
            byte[] command = BuildWriteSingleRegisterCommand(address, value);
            var readResult = await ReadFromCoreServerAsync(command, cancellationToken);
            if (!readResult.IsSuccess) return readResult;

            return OperateResult.Success();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "写入单个寄存器失败");
            return OperateResult.Fail(ex.Message);
        }
    }

    public async Task<OperateResult> WriteMultipleRegistersAsync(ushort startAddress, ushort[] values, CancellationToken cancellationToken = default)
    {
        try
        {
            byte[] command = BuildWriteMultipleRegistersCommand(startAddress, values);
            var readResult = await ReadFromCoreServerAsync(command, cancellationToken);
            if (!readResult.IsSuccess) return readResult;

            return OperateResult.Success();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "写入多个寄存器失败");
            return OperateResult.Fail(ex.Message);
        }
    }

    public async Task<OperateResult> WriteSingleCoilAsync(ushort address, bool value, CancellationToken cancellationToken = default)
    {
        try
        {
            byte[] command = BuildWriteSingleCoilCommand(address, value);
            var readResult = await ReadFromCoreServerAsync(command, cancellationToken);
            if (!readResult.IsSuccess) return readResult;

            return OperateResult.Success();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "写入单个线圈失败");
            return OperateResult.Fail(ex.Message);
        }
    }

    public async Task<OperateResult> WriteMultipleCoilsAsync(ushort startAddress, bool[] values, CancellationToken cancellationToken = default)
    {
        try
        {
            byte[] command = BuildWriteMultipleCoilsCommand(startAddress, values);
            var readResult = await ReadFromCoreServerAsync(command, cancellationToken);
            if (!readResult.IsSuccess) return readResult;

            return OperateResult.Success();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "写入多个线圈失败");
            return OperateResult.Fail(ex.Message);
        }
    }

    private byte[] BuildMbapHeader(ushort length)
    {
        byte[] mbap = new byte[7];
        BitConverter.GetBytes(_transactionId++).CopyTo(mbap, 0);
        BitConverter.GetBytes((ushort)0x0000).CopyTo(mbap, 2);
        BitConverter.GetBytes((ushort)(length + 1)).CopyTo(mbap, 4);
        mbap[6] = _unitIdentifier;
        return mbap;
    }

    private byte[] BuildReadCommand(byte functionCode, ushort startAddress, ushort length)
    {
        byte[] command = new byte[12];
        byte[] mbap = BuildMbapHeader(6);
        mbap.CopyTo(command, 0);
        command[7] = functionCode;
        ByteTransform.GetBytes(startAddress).CopyTo(command, 8);
        ByteTransform.GetBytes(length).CopyTo(command, 10);
        return command;
    }

    private byte[] BuildWriteSingleRegisterCommand(ushort address, ushort value)
    {
        byte[] command = new byte[12];
        byte[] mbap = BuildMbapHeader(6);
        mbap.CopyTo(command, 0);
        command[7] = 6;
        ByteTransform.GetBytes(address).CopyTo(command, 8);
        ByteTransform.GetBytes(value).CopyTo(command, 10);
        return command;
    }

    private byte[] BuildWriteMultipleRegistersCommand(ushort startAddress, ushort[] values)
    {
        int byteCount = values.Length * 2;
        byte[] command = new byte[13 + byteCount];
        byte[] mbap = BuildMbapHeader((ushort)(7 + byteCount));
        mbap.CopyTo(command, 0);
        command[7] = 16;
        ByteTransform.GetBytes(startAddress).CopyTo(command, 8);
        ByteTransform.GetBytes((ushort)values.Length).CopyTo(command, 10);
        command[12] = (byte)byteCount;

        for (int i = 0; i < values.Length; i++)
        {
            ByteTransform.GetBytes(values[i]).CopyTo(command, 13 + i * 2);
        }

        return command;
    }

    private byte[] BuildWriteSingleCoilCommand(ushort address, bool value)
    {
        byte[] command = new byte[12];
        byte[] mbap = BuildMbapHeader(6);
        mbap.CopyTo(command, 0);
        command[7] = 5;
        ByteTransform.GetBytes(address).CopyTo(command, 8);
        command[10] = value ? (byte)0xFF : (byte)0x00;
        command[11] = 0x00;
        return command;
    }

    private byte[] BuildWriteMultipleCoilsCommand(ushort startAddress, bool[] values)
    {
        int byteCount = (int)Math.Ceiling(values.Length / 8.0);
        byte[] command = new byte[13 + byteCount];
        byte[] mbap = BuildMbapHeader((ushort)(7 + byteCount));
        mbap.CopyTo(command, 0);
        command[7] = 15;
        ByteTransform.GetBytes(startAddress).CopyTo(command, 8);
        ByteTransform.GetBytes((ushort)values.Length).CopyTo(command, 10);
        command[12] = (byte)byteCount;

        for (int i = 0; i < values.Length; i++)
        {
            if (values[i])
            {
                command[13 + i / 8] |= (byte)(1 << (i % 8));
            }
        }

        return command;
    }

    protected override async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(byte[] send, CancellationToken cancellationToken = default)
    {
        if (Socket == null || !IsConnected)
        {
            return OperateResult<byte[]>.Fail("未连接", 1003);
        }

        var sendResult = await SendAsync(send, cancellationToken);
        if (!sendResult.IsSuccess)
        {
            return OperateResult<byte[]>.Fail(sendResult.Message, sendResult.ErrorCode);
        }

        var receiveResult = await ReceiveAsync(7, cancellationToken);
        if (!receiveResult.IsSuccess) return receiveResult;

        ushort remainingLength = (ushort)(ByteTransform.TransUInt16(receiveResult.Content!, 4) - 1);
        if (remainingLength > 0)
        {
            var moreResult = await ReceiveAsync(remainingLength, cancellationToken);
            if (!moreResult.IsSuccess) return moreResult;

            byte[] result = new byte[7 + remainingLength];
            Array.Copy(receiveResult.Content, result, 7);
            Array.Copy(moreResult.Content, 0, result, 7, remainingLength);
            return OperateResult<byte[]>.Success(result);
        }

        return receiveResult;
    }

    public override string ToString()
    {
        return $"ModbusTcpNet[{IpAddress}:{Port}]";
    }
}
