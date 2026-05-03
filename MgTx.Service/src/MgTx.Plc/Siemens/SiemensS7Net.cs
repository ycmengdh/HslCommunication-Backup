using MgTx.Core.Net;
using MgTx.Core.Transfer;
using MgTx.Core.Types;
using Microsoft.Extensions.Logging;
using System.Text;

namespace MgTx.Plc.Siemens;

public enum SiemensPLCS
{
    S1200,
    S300,
    S400,
    S1500,
    S200Smart,
    S200,
}

public class SiemensS7Net : NetworkDeviceBase
{
    private SiemensPLCS _plc;
    private int _connectionId = 1;
    private int _pduLength = 240;
    private int _connectTimeout = 5000;
    private int _receiveTimeout = 3000;
    private int _sendTimeout = 3000;

    public SiemensS7Net(SiemensPLCS plc = SiemensPLCS.S1200, ILogger? logger = null) : base(logger)
    {
        _plc = plc;
        Port = 102;
        WordLength = 1;
        ByteTransform = new RegularByteTransform();
    }

    public SiemensPLCS PLC
    {
        get => _plc;
        set => _plc = value;
    }

    public byte Rack { get; set; } = 0;

    public byte Slot { get; set; } = 0;

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

    protected override async Task<OperateResult> InitializationOnConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Logger?.LogInformation("开始西门子 PLC 握手: {Type}", _plc);

            byte[] connectRequest = BuildConnectRequest();
            var connectResult = await ReadFromCoreServerAsync(connectRequest, cancellationToken);
            if (!connectResult.IsSuccess) return connectResult;

            if (!CheckResponse(connectResult.Content!))
            {
                return OperateResult.Fail("PLC 连接响应异常");
            }

            _connectionId = ByteTransform.TransUInt16(connectResult.Content!, 17);
            Logger?.LogInformation("西门子 PLC 握手成功，连接ID: {ConnectionId}", _connectionId);

            return OperateResult.Success();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "西门子 PLC 握手失败");
            return OperateResult.Fail(ex.Message);
        }
    }

    private byte[] BuildConnectRequest()
    {
        List<byte> command = new List<byte>();
        byte[] head = new byte[22];
        head[0] = 0x03;
        head[1] = 0x00;
        head[2] = 0x00;
        head[3] = 0x16;
        head[4] = 0x11;
        head[5] = 0xE0;
        head[6] = 0x00;
        head[7] = 0x00;
        head[8] = 0x00;
        head[9] = 0x01;
        head[10] = 0x00;
        head[11] = 0xC1;
        head[12] = 0x02;
        head[13] = 0x01;
        head[14] = 0x00;
        head[15] = 0xC2;
        head[16] = 0x02;
        head[17] = 0x01;
        head[18] = 0x02;
        head[19] = 0xC0;
        head[20] = 0x01;
        head[21] = 0x0A;

        if (_plc == SiemensPLCS.S200Smart)
        {
            head[13] = 0x00;
            head[17] = 0x00;
            head[21] = 0x01;
        }

        command.AddRange(head);
        return command.ToArray();
    }

    private bool CheckResponse(byte[] content)
    {
        if (content.Length < 22) return false;
        return true;
    }

    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length, CancellationToken cancellationToken = default)
    {
        try
        {
            var addressResult = AnalysisAddress(address);
            if (!addressResult.IsSuccess) return OperateResult<byte[]>.Fail(addressResult.Message, addressResult.ErrorCode);

            byte[] command = BuildReadCommand(addressResult.Content.type, addressResult.Content.dbBlock, addressResult.Content.startAddress, length);
            var readResult = await ReadFromCoreServerAsync(command, cancellationToken);
            if (!readResult.IsSuccess) return readResult;

            if (readResult.Content!.Length < 21) return OperateResult<byte[]>.Fail("响应数据过短");

            byte[] result = new byte[readResult.Content.Length - 21];
            Array.Copy(readResult.Content, 21, result, 0, result.Length);
            return OperateResult<byte[]>.Success(result);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "读取西门子 PLC 失败: {Address}", address);
            return OperateResult<byte[]>.Fail(ex.Message);
        }
    }

    public override async Task<OperateResult> WriteAsync(string address, byte[] data, CancellationToken cancellationToken = default)
    {
        try
        {
            var addressResult = AnalysisAddress(address);
            if (!addressResult.IsSuccess) return addressResult;

            byte[] command = BuildWriteCommand(addressResult.Content.type, addressResult.Content.dbBlock, addressResult.Content.startAddress, data);
            var readResult = await ReadFromCoreServerAsync(command, cancellationToken);
            if (!readResult.IsSuccess) return readResult;

            return OperateResult.Success();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "写入西门子 PLC 失败: {Address}", address);
            return OperateResult.Fail(ex.Message);
        }
    }

    private byte[] BuildReadCommand(byte typeCode, int dbBlock, int startAddress, ushort length)
    {
        byte[] command = new byte[31];
        command[0] = 0x03;
        command[1] = 0x00;
        command[2] = 0x00;
        command[3] = 0x1F;
        command[4] = 0x02;
        command[5] = 0xF0;
        command[6] = 0x80;
        command[7] = 0x32;
        command[8] = 0x01;
        command[9] = 0x00;
        command[10] = 0x00;
        command[11] = 0x01;
        command[12] = 0x00;
        command[13] = 0x0E;
        command[14] = 0x00;
        command[15] = 0x04;
        command[16] = 0x01;
        command[17] = 0x12;
        command[18] = 0x0A;
        command[19] = 0x10;
        command[20] = 0x02;

        ByteTransform.GetBytes((ushort)length).CopyTo(command, 22);

        if (typeCode == 0x84)
        {
            ByteTransform.GetBytes((ushort)dbBlock).CopyTo(command, 24);
        }

        command[26] = typeCode;

        ByteTransform.GetBytes((ushort)(startAddress * 8)).CopyTo(command, 27);

        return command;
    }

    private byte[] BuildWriteCommand(byte typeCode, int dbBlock, int startAddress, byte[] data)
    {
        int byteLength = data.Length;
        int length = (data.Length + 1) / 2;

        byte[] command = new byte[35 + byteLength];
        command[0] = 0x03;
        command[1] = 0x00;

        ushort len = (ushort)(33 + byteLength);
        ByteTransform.GetBytes(len).CopyTo(command, 2);

        command[4] = 0x02;
        command[5] = 0xF0;
        command[6] = 0x80;
        command[7] = 0x32;
        command[8] = 0x01;
        command[9] = 0x00;
        command[10] = 0x00;
        command[11] = 0x01;
        command[12] = 0x00;

        len = (ushort)(12 + byteLength);
        ByteTransform.GetBytes(len).CopyTo(command, 13);

        command[15] = 0x05;
        command[16] = 0x01;
        command[17] = 0x12;
        command[18] = 0x0A;
        command[19] = 0x10;
        command[20] = 0x02;

        ByteTransform.GetBytes((ushort)length).CopyTo(command, 22);

        if (typeCode == 0x84)
        {
            ByteTransform.GetBytes((ushort)dbBlock).CopyTo(command, 24);
        }

        command[26] = typeCode;

        ByteTransform.GetBytes((ushort)(startAddress * 8)).CopyTo(command, 27);

        command[29] = 0x00;
        command[30] = 0x04;

        ByteTransform.GetBytes((ushort)(byteLength * 8)).CopyTo(command, 31);

        Array.Copy(data, 0, command, 35, data.Length);

        return command;
    }

    private OperateResult<(byte type, int dbBlock, int startAddress)> AnalysisAddress(string address)
    {
        try
        {
            address = address.ToUpper();
            byte typeCode = 0x81;
            int dbBlock = 0;
            int startAddress = 0;

            if (address.StartsWith("I"))
            {
                typeCode = 0x81;
                startAddress = int.Parse(address.Substring(1));
            }
            else if (address.StartsWith("Q"))
            {
                typeCode = 0x82;
                startAddress = int.Parse(address.Substring(1));
            }
            else if (address.StartsWith("M"))
            {
                typeCode = 0x83;
                startAddress = int.Parse(address.Substring(1));
            }
            else if (address.StartsWith("DB"))
            {
                typeCode = 0x84;
                int index = address.IndexOf('.');
                if (index > 0)
                {
                    dbBlock = int.Parse(address.Substring(2, index - 2));
                    startAddress = int.Parse(address.Substring(index + 1));
                }
                else
                {
                    dbBlock = int.Parse(address.Substring(2));
                }
            }

            return OperateResult<(byte, int, int)>.Success((typeCode, dbBlock, startAddress));
        }
        catch (Exception ex)
        {
            return OperateResult<(byte, int, int)>.Fail(ex.Message);
        }
    }

    public override string ToString()
    {
        return $"SiemensS7Net[{IpAddress}:{Port}]";
    }
}
