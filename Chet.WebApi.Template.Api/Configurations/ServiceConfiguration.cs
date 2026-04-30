using Chet.WebApi.Template.Contracts.Auth;
using Chet.WebApi.Template.Contracts.Jwt;
using Chet.WebApi.Template.Contracts.Security;
using Chet.WebApi.Template.Contracts.User;
using Chet.WebApi.Template.Services.Auth;
using Chet.WebApi.Template.Services.Jwt;
using Chet.WebApi.Template.Services.Security;
using Chet.WebApi.Template.Services.User;

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
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<Chet.WebApi.Template.Contracts.IUnitOfWork, Data.UnitOfWork>();
    }
}
