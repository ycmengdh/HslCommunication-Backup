namespace MgTx.Core.Transfer;

public interface IByteTransform
{
    DataFormat DataFormat { get; set; }
    byte[] TransByte(byte[] buffer);
    bool TransBool(byte[] buffer, int index);
    short TransInt16(byte[] buffer, int index = 0);
    ushort TransUInt16(byte[] buffer, int index = 0);
    int TransInt32(byte[] buffer, int index = 0);
    uint TransUInt32(byte[] buffer, int index = 0);
    long TransInt64(byte[] buffer, int index = 0);
    ulong TransUInt64(byte[] buffer, int index = 0);
    float TransSingle(byte[] buffer, int index = 0);
    double TransDouble(byte[] buffer, int index = 0);
    string TransString(byte[] buffer, int index = 0, int length = -1);
    byte[] TransByte(byte[] buffer, int index = 0, int length = -1);
    short[] TransInt16Array(byte[] buffer, int index = 0, int length = -1);
    ushort[] TransUInt16Array(byte[] buffer, int index = 0, int length = -1);
    int[] TransInt32Array(byte[] buffer, int index = 0, int length = -1);
    uint[] TransUInt32Array(byte[] buffer, int index = 0, int length = -1);
    long[] TransInt64Array(byte[] buffer, int index = 0, int length = -1);
    ulong[] TransUInt64Array(byte[] buffer, int index = 0, int length = -1);
    float[] TransSingleArray(byte[] buffer, int index = 0, int length = -1);
    double[] TransDoubleArray(byte[] buffer, int index = 0, int length = -1);
    bool[] TransBoolArray(byte[] buffer, int index = 0, int length = -1);
    byte[] GetBytes(bool value);
    byte[] GetBytes(short value);
    byte[] GetBytes(ushort value);
    byte[] GetBytes(int value);
    byte[] GetBytes(uint value);
    byte[] GetBytes(long value);
    byte[] GetBytes(ulong value);
    byte[] GetBytes(float value);
    byte[] GetBytes(double value);
    byte[] GetBytes(string value);
    byte[] GetBytes(bool[] value);
    byte[] GetBytes(short[] value);
    byte[] GetBytes(ushort[] value);
    byte[] GetBytes(int[] value);
    byte[] GetBytes(uint[] value);
    byte[] GetBytes(long[] value);
    byte[] GetBytes(ulong[] value);
    byte[] GetBytes(float[] value);
    byte[] GetBytes(double[] value);
}
