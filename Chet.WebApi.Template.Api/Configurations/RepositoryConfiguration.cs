using Chet.WebApi.Template.Contracts;
using Chet.WebApi.Template.Contracts.User;
using Chet.WebApi.Template.Data;
using Chet.WebApi.Template.Data.User;

namespace Chet.WebApi.Template.Api.Configurations;

/// <summary>
/// 仓储服务配置类
/// </summary>
public static class RepositoryConfiguration
{
    /// <summary>
    /// 配置仓储服务
    /// </summary>
    /// <param name="services">IServiceCollection实例</param>
    public static void ConfigureRepositories(this IServiceCollection services)
    {
        // 注册仓储服务
        services.AddScoped(typeof(IRepository<>), typeof(EfCoreRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
    }
}
