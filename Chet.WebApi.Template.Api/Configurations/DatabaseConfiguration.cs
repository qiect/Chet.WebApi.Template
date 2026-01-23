using Chet.WebApi.Template.Data;
using Microsoft.EntityFrameworkCore;

namespace Chet.WebApi.Template.Api.Configurations;

/// <summary>
/// 数据库配置类
/// </summary>
public static class DatabaseConfiguration
{
    /// <summary>
    /// 配置数据库
    /// </summary>
    /// <param name="services">IServiceCollection实例</param>
    /// <param name="configuration">IConfiguration实例</param>
    public static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        // 添加数据库上下文服务
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
    }

    /// <summary>
    /// 初始化数据库
    /// </summary>
    /// <param name="app">WebApplication实例</param>
    public static void InitializeDatabase(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.EnsureCreated();
        }
    }
}
