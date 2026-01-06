namespace Chet.WebApi.Template.Shared;

/// <summary>
/// 通用API响应包装器
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// 响应状态码
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// 响应状态，true表示成功，false表示失败
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 响应消息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 响应数据
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// 响应时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 构造函数
    /// </summary>
    public ApiResponse()
    { }

    /// <summary>
    /// 成功响应静态工厂方法
    /// </summary>
    /// <param name="data">响应数据</param>
    /// <param name="message">响应消息</param>
    /// <param name="statusCode">响应状态码，默认200</param>
    /// <returns>ApiResponse实例</returns>
    public static ApiResponse Ok(object? data = null, string? message = null, int statusCode = 200)
    {
        return new ApiResponse
        {
            StatusCode = statusCode,
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// 错误响应静态工厂方法
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="statusCode">错误状态码，默认500</param>
    /// <returns>ApiResponse实例</returns>
    public static ApiResponse Error(string? message = null, int statusCode = 500)
    {
        return new ApiResponse
        {
            StatusCode = statusCode,
            Success = false,
            Message = message
        };
    }

    /// <summary>
    /// 无数据成功响应静态工厂方法
    /// </summary>
    /// <param name="message">响应消息</param>
    /// <param name="statusCode">响应状态码，默认204</param>
    /// <returns>ApiResponse实例</returns>
    public static ApiResponse NoContent(string? message = null, int statusCode = 204)
    {
        return new ApiResponse
        {
            StatusCode = statusCode,
            Success = true,
            Message = message
        };
    }
}

/// <summary>
/// 泛型API响应包装器
/// </summary>
/// <typeparam name="T">响应数据类型</typeparam>
public class ApiResponse<T> : ApiResponse
{
    /// <summary>
    /// 泛型响应数据
    /// </summary>
    public new T? Data { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    public ApiResponse()
    { }

    /// <summary>
    /// 成功响应静态工厂方法
    /// </summary>
    /// <param name="data">响应数据</param>
    /// <param name="message">响应消息</param>
    /// <param name="statusCode">响应状态码，默认200</param>
    /// <returns>ApiResponse&lt;T&gt;实例</returns>
    public static ApiResponse<T> CreateSuccess(T? data = default, string? message = null, int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            StatusCode = statusCode,
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// 错误响应静态工厂方法
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="statusCode">错误状态码，默认500</param>
    /// <returns>ApiResponse&lt;T&gt;实例</returns>
    public static ApiResponse<T> CreateError(string? message = null, int statusCode = 500)
    {
        return new ApiResponse<T>
        {
            StatusCode = statusCode,
            Success = false,
            Message = message
        };
    }
}

