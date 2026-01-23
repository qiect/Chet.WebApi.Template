using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using Chet.WebApi.Template.Shared;

namespace Chet.WebApi.Template.Api.Configurations;

/// <summary>
/// 异常处理配置类
/// </summary>
public static class ExceptionHandlingConfiguration
{
    /// <summary>
    /// 配置异常处理中间件
    /// </summary>
    /// <param name="app">WebApplication实例</param>
    public static void ConfigureExceptionHandling(this WebApplication app)
    {
        // 添加自定义异常处理中间件
        app.UseExceptionHandler(options =>
        {
            options.Run(async context =>
            {
                // 获取异常信息
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                if (exception == null)
                {
                    return;
                }

                // 记录异常日志
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(exception, "An unexpected error occurred");

                // 设置默认错误状态码和消息
                var statusCode = HttpStatusCode.InternalServerError;
                var message = "An unexpected error occurred";

                // 根据异常类型设置不同的状态码和消息
                if (exception.GetType().Name == "NotFoundException")
                {
                    statusCode = HttpStatusCode.NotFound;
                    message = exception.Message;
                }
                else if (exception.GetType().Name == "BadRequestException")
                {
                    statusCode = HttpStatusCode.BadRequest;
                    message = exception.Message;
                }
                else if (exception is UnauthorizedAccessException)
                {
                    statusCode = HttpStatusCode.Unauthorized;
                    message = exception.Message;
                }

                // 构造统一格式的错误响应
                context.Response.StatusCode = (int)statusCode;
                context.Response.ContentType = "application/json";

                var errorResponse = ApiResponse.Error(message, (int)statusCode);

                // 返回错误响应
                await context.Response.WriteAsJsonAsync(errorResponse);
            });
        });
    }
}
