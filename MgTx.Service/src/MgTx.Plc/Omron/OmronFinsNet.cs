using MgTx.Core.Net;
using MgTx.Core.Transfer;
using MgTx.Core.Types;
using Microsoft.Extensions.Logging;
using System.Text;

namespace MgTx.Plc.Omron;

public class OmronFinsNet : NetworkDeviceBase
{
    private byte _da1 = 0x00;
    private byte _da2 = 0x00;
    private byte _sa1 = 0x00;
    private byte _sa2 = 0x00;
    private byte _sno = 0x00;
    private int _sid = 0x00;
    private int _connectTimeout = 5000;
    private int _receiveTimeout = 3000;
    private int _sendTimeout = 3000;

    public OmronFinsNet(ILogger? logger = null) : base(logger)
    {
        Port = 9600;
        WordLength = 1;
        ByteTransform = new RegularByteTransform();
    }

    public byte DA1
    {
        get => _da1;
        set => _da1 = value;
    }

    public byte DA2
    {
        get => _da2;
        set => _da2 = value;
    }

    public byte SA1
    {
        get => _sa1;
        set => _sa1 = value;
    }

    public byte SA2
    {
        get => _sa2;
        set => _sa2 = value;
    }

    public byte SNO
    {
        get => _sno;
        set => _sno = value;
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

            byte[] command = BuildReadCommand(addressResult.Content.typeCode, addressResult.Content.bitCode, addressResult.Content.address, length);
            var readResult = await ReadFromCoreServerAsync(command, cancellationToken);
            if (!readResult.IsSuccess) return readResult;

            if (readResult.Content!.Length < 22) return OperateResult<byte[]>.Fail("响应数据过短");

            int byteCount = (int)(length * 2);
            byte[] result = new byte[byteCount];
            Array.Copy(readResult.Content, 22, result, 0, byteCount);
            return OperateResult<byte[]>.Success(result);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "读取欧姆龙 PLC 失败: {Address}", address);
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
            byte[] command = BuildWriteCommand(addressResult.Content.typeCode, addressResult.Content.bitCode, addressResult.Content.address, (ushort)length, data);
            var readResult = await ReadFromCoreServerAsync(command, cancellationToken);
            if (!readResult.IsSuccess) return readResult;

            return OperateResult.Success();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "写入欧姆龙 PLC 失败: {Address}", address);
            return OperateResult.Fail(ex.Message);
        }
    }

    private byte[] BuildReadCommand(byte typeCode, byte bitCode, int address, ushort length)
    {
        byte[] command = new byte[22];
        command[0] = 0x46;
        command[1] = 0x49;
        command[2] = 0x4E;
        command[3] = 0x53;
        command[4] = 0x00;
        command[5] = 0x00;
        command[6] = 0x00;
        command[7] = 0x0C;
        command[8] = 0x00;
        command[9] = 0x00;
        command[10] = 0x00;
        command[11] = 0x00;

        command[12] = 0x80;
        command[13] = 0x00;
        command[14] = 0x02;
        command[15] = 0x00;
        command[16] = DA1;
        command[17] = DA2;
        command[18] = SNO;
        command[19] = SA1;
        command[20] = SA2;
        command[21] = 0x00;

        command[22] = 0x01;
        command[23] = 0x01;

        command[24] = typeCode;
        command[25] = bitCode;
        ByteTransform.GetBytes((ushort)address).CopyTo(command, 26);
        ByteTransform.GetBytes((ushort)length).CopyTo(command, 29);

        return command;
    }

    private byte[] BuildWriteCommand(byte typeCode, byte bitCode, int address, ushort length, byte[] data)
    {
        byte[] command = new byte[22 + data.Length];
        command[0] = 0x46;
        command[1] = 0x49;
        command[2] = 0x4E;
        command[3] = 0x53;
        command[4] = 0x00;
        command[5] = 0x00;
        command[6] = 0x00;
        command[7] = 0x0C;
        command[8] = 0x00;
        command[9] = 0x00;
        command[10] = 0x00;
        command[11] = 0x00;

        command[12] = 0x80;
        command[13] = 0x00;
        command[14] = 0x02;
        command[15] = 0x00;
        command[16] = DA1;
        command[17] = DA2;
        command[18] = SNO;
        command[19] = SA1;
        command[20] = SA2;
        command[21] = 0x00;

        command[22] = 0x01;
        command[23] = 0x02;

        command[24] = typeCode;
        command[25] = bitCode;
        ByteTransform.GetBytes((ushort)address).CopyTo(command, 26);
        ByteTransform.GetBytes((ushort)length).CopyTo(command, 29);

        Array.Copy(data, 0, command, 32, data.Length);

        return command;
    }

    private OperateResult<(byte typeCode, byte bitCode, int address)> AnalysisAddress(string address)
    {
        try
        {
            address = address.ToUpper();
            byte typeCode = 0x82;
            byte bitCode = 0x00;
            int startAddress = 0;

            if (address.StartsWith("CIO"))
            {
                typeCode = 0xB0;
                startAddress = int.Parse(address.Substring(3));
            }
            else if (address.StartsWith("W"))
            {
                typeCode = 0x31;
                startAddress = int.Parse(address.Substring(1));
            }
            else if (address.StartsWith("H"))
            {
                typeCode = 0x32;
                startAddress = int.Parse(address.Substring(1));
            }
            else if (address.StartsWith("A"))
            {
                typeCode = 0x33;
                startAddress = int.Parse(address.Substring(1));
            }
            else if (address.StartsWith("DM"))
            {
                typeCode = 0x82;
                startAddress = int.Parse(address.Substring(2));
            }
            else if (address.StartsWith("D"))
            {
                typeCode = 0x82;
                startAddress = int.Parse(address.Substring(1));
            }
            else if (address.StartsWith("E"))
            {
                typeCode = 0xA0;
                startAddress = int.Parse(address.Substring(1));
            }

            return OperateResult<(byte, byte, int)>.Success((typeCode, bitCode, startAddress));
        }
        catch (Exception ex)
        {
            return OperateResult<(byte, byte, int)>.Fail(ex.Message);
        }
    }

    public override string ToString()
    {
        return $"OmronFinsNet[{IpAddress}:{Port}]";
    }
}
