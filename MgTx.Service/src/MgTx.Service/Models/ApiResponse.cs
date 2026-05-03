namespace MgTx.Service.Models;

public record ApiResponse<T>(
    bool Success,
    T? Data,
    ApiError? Error,
    DateTime Timestamp = default
)
{
    public static ApiResponse<T> Ok(T data) => new(true, data, null, DateTime.UtcNow);
    
    public static ApiResponse<T> Fail(string message, int code = -1) => new(
        false, 
        default, 
        new ApiError(code, message), 
        DateTime.UtcNow
    );
}

public record ApiError(int Code, string Message, object? Details = null);

public record ApiResponse(
    bool Success,
    object? Data,
    ApiError? Error,
    DateTime Timestamp = default
)
{
    public static ApiResponse Ok(object? data = null) => new(true, data, null, DateTime.UtcNow);
    
    public static ApiResponse Fail(string message, int code = -1) => new(
        false, 
        null, 
        new ApiError(code, message), 
        DateTime.UtcNow
    );
}
