
using System;
using System.Text;

namespace TestEndian;

public static class Program
{
    public static void Main()
    {
        Console.WriteLine($"BitConverter.IsLittleEndian: {BitConverter.IsLittleEndian}");
        
        byte[] buffer = new byte[] { 0x00, 0x01 };
        
        // Original TransInt16:
        byte[] tmp = new byte[2];
        tmp[1] = buffer[0];
        tmp[0] = buffer[1];
        Console.WriteLine($"tmp: {BitConverter.ToString(tmp)}");
        Console.WriteLine($"BitConverter.ToInt16(tmp,0): {BitConverter.ToInt16(tmp,0)}");

        // Original GetBytes:
        byte[] bytes = BitConverter.GetBytes((short)256);
        Console.WriteLine($"Before reverse: {BitConverter.ToString(bytes)}");
        Array.Reverse(bytes);
        Console.WriteLine($"After reverse: {BitConverter.ToString(bytes)}");
    }
}
