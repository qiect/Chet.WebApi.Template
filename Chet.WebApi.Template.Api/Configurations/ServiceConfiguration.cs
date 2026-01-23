using Chet.WebApi.Template.Contracts;
using Chet.WebApi.Template.Services;

namespace Chet.WebApi.Template.Api.Configurations;

/// <summary>
/// 业务逻辑服务配置类
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    /// 配置业务逻辑服务
    /// </summary>
    /// <param name="services">IServiceCollection实例</param>
    public static void ConfigureServices(this IServiceCollection services)
    {
        // 注册业务逻辑服务
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJwtService, JwtService>();
    }
}
