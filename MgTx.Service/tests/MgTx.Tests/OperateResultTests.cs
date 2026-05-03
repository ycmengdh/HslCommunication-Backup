using MgTx.Core.Types;
using FluentAssertions;

namespace MgTx.Tests;

public class OperateResultTests
{
    [Fact]
    public void Success_Should_CreateSuccessResult()
    {
        var result = OperateResult.Success();
        
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().BeNull();
        result.ErrorCode.Should().Be(0);
    }

    [Fact]
    public void Success_WithMessage_Should_CreateSuccessResultWithMessage()
    {
        var result = OperateResult.Success("操作成功");
        
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("操作成功");
    }

    [Fact]
    public void Fail_Should_CreateFailureResult()
    {
        var result = OperateResult.Fail("操作失败");
        
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("操作失败");
        result.ErrorCode.Should().Be(-1);
    }

    [Fact]
    public void Fail_WithErrorCode_Should_CreateFailureResultWithCode()
    {
        var result = OperateResult.Fail("操作失败", 1001);
        
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("操作失败");
        result.ErrorCode.Should().Be(1001);
    }

    [Fact]
    public void Success_T_Should_CreateSuccessResultWithContent()
    {
        var result = OperateResult<string>.Success("test data");
        
        result.IsSuccess.Should().BeTrue();
        result.Content.Should().Be("test data");
    }

    [Fact]
    public void Fail_T_Should_CreateFailureResult()
    {
        var result = OperateResult<string>.Fail("操作失败");
        
        result.IsSuccess.Should().BeFalse();
        result.Content.Should().BeNull();
    }

    [Fact]
    public void Convert_Should_ConvertGenericToNonGeneric()
    {
        var generic = OperateResult<string>.Success("test");
        
        var result = generic.Convert();
        
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().BeNull();
    }

    [Fact]
    public void Convert_WithContent_Should_ConvertNonGenericToGeneric()
    {
        var nonGeneric = OperateResult.Success();
        
        var result = nonGeneric.Convert("test data");
        
        result.IsSuccess.Should().BeTrue();
        result.Content.Should().Be("test data");
    }
}
