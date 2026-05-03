using MgTx.Core.Net;
using MgTx.Core.Transfer;
using MgTx.Core.Types;
using Microsoft.Extensions.Logging;
using System.Text;

namespace MgTx.Plc.Mitsubishi;

public enum MitsubishiMcType
{
    A1E,
    Qna3E,
    Qna3EAscii,
}

public class MitsubishiMcNet : NetworkDeviceBase
{
    private MitsubishiMcType _mcType = MitsubishiMcType.Qna3E;
    private byte _networkNumber = 0x00;
    private byte _pcNumber = 0xFF;
    private ushort _requestDestinationModuleIoNumber = 0x03FF;
    private byte _requestDestinationModuleStationNumber = 0x00;
    private int _connectTimeout = 5000;
    private int _receiveTimeout = 3000;
    private int _sendTimeout = 3000;

    public MitsubishiMcNet(MitsubishiMcType mcType = MitsubishiMcType.Qna3E, ILogger? logger = null) : base(logger)
    {
        _mcType = mcType;
        Port = 5007;
        WordLength = 1;
        ByteTransform = new RegularByteTransform();
    }

    public MitsubishiMcType McType
    {
        get => _mcType;
        set => _mcType = value;
    }

    public byte NetworkNumber
    {
        get => _networkNumber;
        set => _networkNumber = value;
    }

    public byte PcNumber
    {
        get => _pcNumber;
        set => _pcNumber = value;
    }

    public ushort RequestDestinationModuleIoNumber
    {
        get => _requestDestinationModuleIoNumber;
        set => _requestDestinationModuleIoNumber = value;
    }

    public byte RequestDestinationModuleStationNumber
    {
        get => _requestDestinationModuleStationNumber;
        set => _requestDestinationModuleStationNumber = value;
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

    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        try
        {
            var addressResult = AnalysisAddress(address);
            if (!addressResult.IsSuccess) return addressResult.Convert<byte[]>();

            byte[] command = BuildReadCommand(addressResult.Content.typeCode, addressResult.Content.address, length);
            var readResult = await ReadFromCoreServerAsync(command, cancellationToken);
            if (!readResult.IsSuccess) return readResult;

            if (_mcType == MitsubishiMcType.A1E)
            {
                if (readResult.Content!.Length < 4) return OperateResult<byte[]>.Fail("响应数据过短");

                int byteCount = (int)(length * 2);
                byte[] result = new byte[byteCount];
                Array.Copy(readResult.Content, 2, result, 0, byteCount);
                return OperateResult<byte[]>.Success(result);
            }
            else
            {
                if (readResult.Content!.Length < 11) return OperateResult<byte[]>.Fail("响应数据过短");

                int byteCount = (int)(length * 2);
                byte[] result = new byte[byteCount];
                Array.Copy(readResult.Content, 11, result, 0, byteCount);
                return OperateResult<byte[]>.Success(result);
            }
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "读取三菱 PLC 失败: {Address}", address);
            return OperateResult<byte[]>.Fail(ex.Message);
        }
    }

    public override async Task<OperateResult> WriteAsync(string address, byte[] data, CancellationToken cancellationToken = default)
    {
        try
        {
            var addressResult = AnalysisAddress(address);
            if (!addressResult.IsSuccess) return addressResult;

            int length = (data.Length + 1) / 2;
            byte[] command = BuildWriteCommand(addressResult.Content.typeCode, addressResult.Content.address, (ushort)length, data);
            var readResult = await ReadFromCoreServerAsync(command, cancellationToken);
            if (!readResult.IsSuccess) return readResult;

            return OperateResult.Success();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "写入三菱 PLC 失败: {Address}", address);
            return OperateResult.Fail(ex.Message);
        }
    }

    private byte[] BuildReadCommand(byte typeCode, int address, ushort length)
    {
        if (_mcType == MitsubishiMcType.A1E)
        {
            byte[] command = new byte[12];
            command[0] = 0x50;
            command[1] = 0x00;
            ByteTransform.GetBytes((ushort)address).CopyTo(command, 2);
            ByteTransform.GetBytes((ushort)length).CopyTo(command, 4);
            command[6] = typeCode;
            return command;
        }
        else
        {
            byte[] command = new byte[21];
            command[0] = 0x50;
            command[1] = 0x00;
            command[2] = _networkNumber;
            command[3] = _pcNumber;
            ByteTransform.GetBytes(_requestDestinationModuleIoNumber).CopyTo(command, 4);
            command[6] = _requestDestinationModuleStationNumber;
            command[7] = 0x0C;
            command[8] = 0x00;
            command[9] = 0x00;
            command[10] = 0x01;
            command[11] = 0x04;
            ByteTransform.GetBytes((ushort)address).CopyTo(command, 12);
            ByteTransform.GetBytes((ushort)length).CopyTo(command, 15);
            command[17] = typeCode;
            return command;
        }
    }

    private byte[] BuildWriteCommand(byte typeCode, int address, ushort length, byte[] data)
    {
        if (_mcType == MitsubishiMcType.A1E)
        {
            byte[] command = new byte[12 + data.Length];
            command[0] = 0x51;
            command[1] = 0x00;
            ByteTransform.GetBytes((ushort)address).CopyTo(command, 2);
            ByteTransform.GetBytes((ushort)length).CopyTo(command, 4);
            command[6] = typeCode;
            Array.Copy(data, 0, command, 12, data.Length);
            return command;
        }
        else
        {
            byte[] command = new byte[21 + data.Length];
            command[0] = 0x51;
            command[1] = 0x00;
            command[2] = _networkNumber;
            command[3] = _pcNumber;
            ByteTransform.GetBytes(_requestDestinationModuleIoNumber).CopyTo(command, 4);
            command[6] = _requestDestinationModuleStationNumber;
            command[7] = 0x0C;
            command[8] = 0x00;
            command[9] = 0x00;
            command[10] = 0x01;
            command[11] = 0x14;
            ByteTransform.GetBytes((ushort)address).CopyTo(command, 12);
            ByteTransform.GetBytes((ushort)length).CopyTo(command, 15);
            command[17] = typeCode;
            Array.Copy(data, 0, command, 21, data.Length);
            return command;
        }
    }

    private OperateResult<(byte typeCode, int address)> AnalysisAddress(string address)
    {
        try
        {
            address = address.ToUpper();
            byte typeCode = 0x90;
            int startAddress = 0;

            if (address.StartsWith("M"))
            {
                typeCode = 0x90;
                startAddress = int.Parse(address.Substring(1));
            }
            else if (address.StartsWith("D"))
            {
                typeCode = 0xA8;
                startAddress = int.Parse(address.Substring(1));
            }
            else if (address.StartsWith("W"))
            {
                typeCode = 0xB4;
                startAddress = int.Parse(address.Substring(1));
            }
            else if (address.StartsWith("X"))
            {
                typeCode = 0x9C;
                startAddress = int.Parse(address.Substring(1));
            }
            else if (address.StartsWith("Y"))
            {
                typeCode = 0x9D;
                startAddress = int.Parse(address.Substring(1));
            }
            else if (address.StartsWith("R"))
            {
                typeCode = 0xAF;
                startAddress = int.Parse(address.Substring(1));
            }

            return OperateResult<(byte, int)>.Success((typeCode, startAddress));
        }
        catch (Exception ex)
        {
            return OperateResult<(byte, int)>.Fail(ex.Message);
        }
    }

    public override string ToString()
    {
        return $"MitsubishiMcNet[{IpAddress}:{Port}]";
    }
}
