using MgTx.Core.Transfer;
using MgTx.Core.Types;
using FluentAssertions;
using Xunit;

namespace MgTx.Tests;

public class ByteTransformTests
{
    private readonly RegularByteTransform _transform;

    public ByteTransformTests()
    {
        _transform = new RegularByteTransform { DataFormat = DataFormat.DCBA };
    }

    [Fact]
    public void TransInt16_Should_ConvertCorrectly()
    {
        byte[] buffer = new byte[] { 0x00, 0x01 };
        
        var result = _transform.TransInt16(buffer, 0);
        
        result.Should().Be(256);
    }

    [Fact]
    public void TransUInt16_Should_ConvertCorrectly()
    {
        byte[] buffer = new byte[] { 0x00, 0x01 };
        
        var result = _transform.TransUInt16(buffer, 0);
        
        result.Should().Be(256);
    }

    [Fact]
    public void TransInt32_Should_ConvertCorrectly()
    {
        byte[] buffer = new byte[] { 0x01, 0x00, 0x00, 0x00 };
        
        var result = _transform.TransInt32(buffer, 0);
        
        result.Should().Be(1);
    }

    [Fact]
    public void TransSingle_Should_ConvertCorrectly()
    {
        byte[] buffer = new byte[] { 0x66, 0xE6, 0xF6, 0x42 };
        
        var result = _transform.TransSingle(buffer, 0);
        
        result.Should().BeApproximately(123.45f, 0.1f);
    }

    [Fact]
    public void GetBytes_Int16_Should_ConvertCorrectly()
    {
        var result = _transform.GetBytes((short)256);
        
        result.Should().HaveCount(2);
        result[0].Should().Be(0x00);
        result[1].Should().Be(0x01);
    }

    [Fact]
    public void GetBytes_Single_Should_ConvertCorrectly()
    {
        var result = _transform.GetBytes(123.45f);
        
        result.Should().HaveCount(4);
    }

    [Fact]
    public void TransInt16_WithDCBA_Should_SwapBytes()
    {
        _transform.DataFormat = DataFormat.DCBA;
        byte[] buffer = new byte[] { 0x01, 0x00 };
        
        var result = _transform.TransInt16(buffer, 0);
        
        result.Should().Be(1);
    }

    [Fact]
    public void TransInt16_WithABCD_Should_KeepBytes()
    {
        _transform.DataFormat = DataFormat.ABCD;
        byte[] buffer = new byte[] { 0x00, 0x01 };
        
        var result = _transform.TransInt16(buffer, 0);
        
        result.Should().Be(1);
    }
}
