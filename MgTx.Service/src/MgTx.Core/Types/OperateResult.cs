// MgTx - 猛攻通讯
// Copyright (c) 2026 MgTx Project
// Licensed under MIT License
// https://github.com/mgtx/mgtx-service

namespace MgTx.Core.Types;

/// <summary>
/// 统一操作结果类型，用于封装所有操作的返回结果
/// </summary>
/// <remarks>
/// 提供统一的成功/失败状态、错误消息和错误代码
/// </remarks>
public record OperateResult(bool IsSuccess, string? Message = null, int ErrorCode = 0)
{
    /// <summary>
    /// 创建成功的操作结果
    /// </summary>
    /// <returns>成功的操作结果</returns>
    public static OperateResult Success() => new(true);
    
    /// <summary>
    /// 创建带消息的成功操作结果
    /// </summary>
    /// <param name="message">成功消息</param>
    /// <returns>成功的操作结果</returns>
    public static OperateResult Success(string message) => new(true, message);
    
    /// <summary>
    /// 创建失败的操作结果
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="errorCode">错误代码（默认为-1）</param>
    /// <returns>失败的操作结果</returns>
    public static OperateResult Fail(string message, int errorCode = -1) => new(false, message, errorCode);
    
    /// <summary>
    /// 创建失败的操作结果 from another OperateResult
    /// </summary>
    public static OperateResult Fail(OperateResult result) => new(false, result.Message, result.ErrorCode);
    
    /// <summary>
    /// 将结果转换为带内容的类型
    /// </summary>
    /// <typeparam name="T">内容类型</typeparam>
    /// <param name="content">可选的内容数据</param>
    /// <returns>带内容的操作结果</returns>
    public OperateResult<T> Convert<T>(T? content = default) => new(IsSuccess, content, Message, ErrorCode);
}

/// <summary>
/// 带内容的操作结果类型，用于封装有返回数据的操作结果
/// </summary>
/// <typeparam name="T">返回的数据类型</typeparam>
/// <remarks>
/// 继承自 OperateResult，增加了 Content 属性用于存储返回数据
/// </remarks>
public record OperateResult<T>(bool IsSuccess, T? Content, string? Message = null, int ErrorCode = 0)
{
    /// <summary>
    /// 创建带内容的成功操作结果
    /// </summary>
    /// <param name="content">返回的数据</param>
    /// <returns>带内容的成功操作结果</returns>
    public static OperateResult<T> Success(T content) => new(true, content);
    
    /// <summary>
    /// 创建带内容的失败操作结果
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="errorCode">错误代码（默认为-1）</param>
    /// <returns>带内容的失败操作结果</returns>
    public static OperateResult<T> Fail(string message, int errorCode = -1) => new(false, default, message, errorCode);
    
    /// <summary>
    /// 创建带内容的失败操作结果 from another OperateResult
    /// </summary>
    public static OperateResult<T> Fail(OperateResult result) => new(false, default, result.Message, result.ErrorCode);
    
    /// <summary>
    /// 将带内容的结果转换为不带内容的类型
    /// </summary>
    /// <returns>不带内容的操作结果</returns>
    public OperateResult Convert() => new(IsSuccess, Message, ErrorCode);
    
    /// <summary>
    /// Implicit conversion to OperateResult
    /// </summary>
    public static implicit operator OperateResult(OperateResult<T> result) => result.Convert();
}
