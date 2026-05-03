using System.Text;

namespace MgTx.Core.Transfer;

public enum DataFormat
{
    ABCD = 0,
    BADC = 1,
    CDAB = 2,
    DCBA = 3
}

public class RegularByteTransform : IByteTransform
{
    public DataFormat DataFormat { get; set; } = DataFormat.ABCD;

    public byte[] TransByte(byte[] buffer)
    {
        return (byte[])buffer.Clone();
    }

    public bool TransBool(byte[] buffer, int index)
    {
        return buffer[index] != 0;
    }

    public short TransInt16(byte[] buffer, int index = 0)
    {
        byte[] tmp = new byte[2];
        tmp[1] = buffer[index];
        tmp[0] = buffer[index + 1];
        return BitConverter.ToInt16(tmp, 0);
    }

    public ushort TransUInt16(byte[] buffer, int index = 0)
    {
        byte[] tmp = new byte[2];
        tmp[1] = buffer[index];
        tmp[0] = buffer[index + 1];
        return BitConverter.ToUInt16(tmp, 0);
    }

    public int TransInt32(byte[] buffer, int index = 0)
    {
        byte[] tmp = new byte[4];
        tmp[3] = buffer[index];
        tmp[2] = buffer[index + 1];
        tmp[1] = buffer[index + 2];
        tmp[0] = buffer[index + 3];
        return BitConverter.ToInt32(tmp, 0);
    }

    public uint TransUInt32(byte[] buffer, int index = 0)
    {
        byte[] tmp = new byte[4];
        tmp[3] = buffer[index];
        tmp[2] = buffer[index + 1];
        tmp[1] = buffer[index + 2];
        tmp[0] = buffer[index + 3];
        return BitConverter.ToUInt32(tmp, 0);
    }

    public long TransInt64(byte[] buffer, int index = 0)
    {
        byte[] tmp = new byte[8];
        for (int i = 0; i < 8; i++)
        {
            tmp[7 - i] = buffer[index + i];
        }
        return BitConverter.ToInt64(tmp, 0);
    }

    public ulong TransUInt64(byte[] buffer, int index = 0)
    {
        byte[] tmp = new byte[8];
        for (int i = 0; i < 8; i++)
        {
            tmp[7 - i] = buffer[index + i];
        }
        return BitConverter.ToUInt64(tmp, 0);
    }

    public float TransSingle(byte[] buffer, int index = 0)
    {
        byte[] tmp = new byte[4];
        tmp[3] = buffer[index];
        tmp[2] = buffer[index + 1];
        tmp[1] = buffer[index + 2];
        tmp[0] = buffer[index + 3];
        return BitConverter.ToSingle(tmp, 0);
    }

    public double TransDouble(byte[] buffer, int index = 0)
    {
        byte[] tmp = new byte[8];
        for (int i = 0; i < 8; i++)
        {
            tmp[7 - i] = buffer[index + i];
        }
        return BitConverter.ToDouble(tmp, 0);
    }

    public string TransString(byte[] buffer, int index = 0, int length = -1)
    {
        if (length == -1)
        {
            length = buffer.Length - index;
        }
        return Encoding.UTF8.GetString(buffer, index, length);
    }

    public byte[] TransByte(byte[] buffer, int index = 0, int length = -1)
    {
        if (length == -1)
        {
            length = buffer.Length - index;
        }
        byte[] result = new byte[length];
        Array.Copy(buffer, index, result, 0, length);
        return result;
    }

    public short[] TransInt16(byte[] buffer, int index = 0, int length = -1)
    {
        if (length == -1)
        {
            length = (buffer.Length - index) / 2;
        }
        short[] result = new short[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = TransInt16(buffer, index + i * 2);
        }
        return result;
    }

    public short[] TransInt16Array(byte[] buffer, int index = 0, int length = -1)
    {
        return TransInt16(buffer, index, length);
    }

    public ushort[] TransUInt16(byte[] buffer, int index = 0, int length = -1)
    {
        if (length == -1)
        {
            length = (buffer.Length - index) / 2;
        }
        ushort[] result = new ushort[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = TransUInt16(buffer, index + i * 2);
        }
        return result;
    }

    public ushort[] TransUInt16Array(byte[] buffer, int index = 0, int length = -1)
    {
        return TransUInt16(buffer, index, length);
    }

    public int[] TransInt32(byte[] buffer, int index = 0, int length = -1)
    {
        if (length == -1)
        {
            length = (buffer.Length - index) / 4;
        }
        int[] result = new int[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = TransInt32(buffer, index + i * 4);
        }
        return result;
    }

    public int[] TransInt32Array(byte[] buffer, int index = 0, int length = -1)
    {
        return TransInt32(buffer, index, length);
    }

    public uint[] TransUInt32(byte[] buffer, int index = 0, int length = -1)
    {
        if (length == -1)
        {
            length = (buffer.Length - index) / 4;
        }
        uint[] result = new uint[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = TransUInt32(buffer, index + i * 4);
        }
        return result;
    }

    public uint[] TransUInt32Array(byte[] buffer, int index = 0, int length = -1)
    {
        return TransUInt32(buffer, index, length);
    }

    public long[] TransInt64(byte[] buffer, int index = 0, int length = -1)
    {
        if (length == -1)
        {
            length = (buffer.Length - index) / 8;
        }
        long[] result = new long[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = TransInt64(buffer, index + i * 8);
        }
        return result;
    }

    public long[] TransInt64Array(byte[] buffer, int index = 0, int length = -1)
    {
        return TransInt64(buffer, index, length);
    }

    public ulong[] TransUInt64(byte[] buffer, int index = 0, int length = -1)
    {
        if (length == -1)
        {
            length = (buffer.Length - index) / 8;
        }
        ulong[] result = new ulong[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = TransUInt64(buffer, index + i * 8);
        }
        return result;
    }

    public ulong[] TransUInt64Array(byte[] buffer, int index = 0, int length = -1)
    {
        return TransUInt64(buffer, index, length);
    }

    public float[] TransSingle(byte[] buffer, int index = 0, int length = -1)
    {
        if (length == -1)
        {
            length = (buffer.Length - index) / 4;
        }
        float[] result = new float[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = TransSingle(buffer, index + i * 4);
        }
        return result;
    }

    public float[] TransSingleArray(byte[] buffer, int index = 0, int length = -1)
    {
        return TransSingle(buffer, index, length);
    }

    public double[] TransDouble(byte[] buffer, int index = 0, int length = -1)
    {
        if (length == -1)
        {
            length = (buffer.Length - index) / 8;
        }
        double[] result = new double[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = TransDouble(buffer, index + i * 8);
        }
        return result;
    }

    public double[] TransDoubleArray(byte[] buffer, int index = 0, int length = -1)
    {
        return TransDouble(buffer, index, length);
    }

    public bool[] TransBoolArray(byte[] buffer, int index = 0, int length = -1)
    {
        if (length == -1)
        {
            length = buffer.Length - index;
        }
        bool[] result = new bool[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = TransBool(buffer, index + i);
        }
        return result;
    }

    public byte[] GetBytes(bool value)
    {
        return new byte[] { value ? (byte)1 : (byte)0 };
    }

    public byte[] GetBytes(short value)
    {
        byte[] buffer = BitConverter.GetBytes(value);
        Array.Reverse(buffer);
        return buffer;
    }

    public byte[] GetBytes(ushort value)
    {
        byte[] buffer = BitConverter.GetBytes(value);
        Array.Reverse(buffer);
        return buffer;
    }

    public byte[] GetBytes(int value)
    {
        byte[] buffer = BitConverter.GetBytes(value);
        Array.Reverse(buffer);
        return buffer;
    }

    public byte[] GetBytes(uint value)
    {
        byte[] buffer = BitConverter.GetBytes(value);
        Array.Reverse(buffer);
        return buffer;
    }

    public byte[] GetBytes(long value)
    {
        byte[] buffer = BitConverter.GetBytes(value);
        Array.Reverse(buffer);
        return buffer;
    }

    public byte[] GetBytes(ulong value)
    {
        byte[] buffer = BitConverter.GetBytes(value);
        Array.Reverse(buffer);
        return buffer;
    }

    public byte[] GetBytes(float value)
    {
        byte[] buffer = BitConverter.GetBytes(value);
        Array.Reverse(buffer);
        return buffer;
    }

    public byte[] GetBytes(double value)
    {
        byte[] buffer = BitConverter.GetBytes(value);
        Array.Reverse(buffer);
        return buffer;
    }

    public byte[] GetBytes(string value)
    {
        return Encoding.UTF8.GetBytes(value);
    }

    public byte[] GetBytes(bool[] value)
    {
        byte[] result = new byte[value.Length];
        for (int i = 0; i < value.Length; i++)
        {
            result[i] = value[i] ? (byte)1 : (byte)0;
        }
        return result;
    }

    public byte[] GetBytes(short[] value)
    {
        byte[] result = new byte[value.Length * 2];
        for (int i = 0; i < value.Length; i++)
        {
            byte[] tmp = GetBytes(value[i]);
            Array.Copy(tmp, 0, result, i * 2, 2);
        }
        return result;
    }

    public byte[] GetBytes(ushort[] value)
    {
        byte[] result = new byte[value.Length * 2];
        for (int i = 0; i < value.Length; i++)
        {
            byte[] tmp = GetBytes(value[i]);
            Array.Copy(tmp, 0, result, i * 2, 2);
        }
        return result;
    }

    public byte[] GetBytes(int[] value)
    {
        byte[] result = new byte[value.Length * 4];
        for (int i = 0; i < value.Length; i++)
        {
            byte[] tmp = GetBytes(value[i]);
            Array.Copy(tmp, 0, result, i * 4, 4);
        }
        return result;
    }

    public byte[] GetBytes(uint[] value)
    {
        byte[] result = new byte[value.Length * 4];
        for (int i = 0; i < value.Length; i++)
        {
            byte[] tmp = GetBytes(value[i]);
            Array.Copy(tmp, 0, result, i * 4, 4);
        }
        return result;
    }

    public byte[] GetBytes(long[] value)
    {
        byte[] result = new byte[value.Length * 8];
        for (int i = 0; i < value.Length; i++)
        {
            byte[] tmp = GetBytes(value[i]);
            Array.Copy(tmp, 0, result, i * 8, 8);
        }
        return result;
    }

    public byte[] GetBytes(ulong[] value)
    {
        byte[] result = new byte[value.Length * 8];
        for (int i = 0; i < value.Length; i++)
        {
            byte[] tmp = GetBytes(value[i]);
            Array.Copy(tmp, 0, result, i * 8, 8);
        }
        return result;
    }

    public byte[] GetBytes(float[] value)
    {
        byte[] result = new byte[value.Length * 4];
        for (int i = 0; i < value.Length; i++)
        {
            byte[] tmp = GetBytes(value[i]);
            Array.Copy(tmp, 0, result, i * 4, 4);
        }
        return result;
    }

    public byte[] GetBytes(double[] value)
    {
        byte[] result = new byte[value.Length * 8];
        for (int i = 0; i < value.Length; i++)
        {
            byte[] tmp = GetBytes(value[i]);
            Array.Copy(tmp, 0, result, i * 8, 8);
        }
        return result;
    }
}
