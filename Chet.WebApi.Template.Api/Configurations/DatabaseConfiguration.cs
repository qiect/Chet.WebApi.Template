using Chet.WebApi.Template.Data;
using Microsoft.EntityFrameworkCore;

namespace Chet.WebApi.Template.Api.Configurations;

/// <summary>
/// 数据库配置扩展类
/// <para>
/// 提供数据库上下文的注册、初始化和迁移管理功能。
/// 支持Entity Framework Core的代码优先（Code-First）迁移策略，
/// 在应用启动时自动应用待处理的数据库迁移。
/// </para>
/// </summary>
/// <remarks>
/// <para>主要功能：</para>
/// <list type="number">
///   <item><description>注册DbContext到依赖注入容器</description></item>
///   <item><description>配置SQLite数据库连接</description></item>
///   <item><description>自动执行数据库迁移</description></item>
///   <item><description>种子数据初始化</description></item>
/// </list>
/// 
/// <para>迁移策略说明：</para>
/// <para>
/// 本项目使用Entity Framework Core的迁移功能来管理数据库Schema变更。
/// 与EnsureCreated()不同，MigrateAsync()支持增量式Schema更新，
/// 不会在检测到Schema变更时丢失现有数据。
/// </para>
/// 
/// <para>使用示例：</para>
/// <code>
/// // 1. 在Program.cs中注册服务
/// builder.Services.ConfigureDatabase(builder.Configuration);
/// 
/// // 2. 在应用构建后初始化数据库
/// var app = builder.Build();
/// await app.InitializeDatabaseAsync();
/// </code>
/// </remarks>
public static class DatabaseConfiguration
{
    /// <summary>
    /// 配置并注册数据库上下文服务
    /// </summary>
    /// <param name="services">依赖注入服务集合</param>
    /// <param name="configuration">应用程序配置，用于读取连接字符串</param>
    /// <remarks>
    /// <para>配置详情：</para>
    /// <list type="table">
    ///   <listheader>
    ///     <term>配置项</term>
    ///     <description>说明</description>
    ///   </listheader>
    ///   <item>
    ///     <term>数据库类型</term>
    ///     <description>SQLite（轻量级、文件型数据库，适合开发和测试环境）</description>
    ///   </item>
    ///   <item>
    ///     <term>连接字符串</term>
    ///     <description>从appsettings.json的ConnectionStrings:DefaultConnection读取</description>
    ///   </item>
    ///   <item>
    ///     <term>迁移程序集</term>
    ///     <description>指定包含迁移文件的程序集（AppDbContext所在程序集）</description>
    ///   </item>
    ///   <item>
    ///     <term>命令超时</term>
    ///     <description>30秒（防止长时间运行的查询阻塞应用）</description>
    ///   </item>
    /// </list>
    /// 
    /// <para>生命周期：</para>
    /// <para>注册为Scoped作用域，每个HTTP请求创建一个实例。</para>
    /// 
    /// <para>示例配置（appsettings.json）：</para>
    /// <code>
    /// {
    ///   "ConnectionStrings": {
    ///     "DefaultConnection": "Data Source=Chet.WebApi.Template.db"
    ///   }
    /// }
    /// </code>
    /// </remarks>
    public static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    sqlOptions.CommandTimeout(30);
                }));
    }

    /// <summary>
    /// 初始化数据库并应用挂起的迁移
    /// <para>
    /// 在应用启动时调用此方法，确保数据库Schema与当前模型一致。
    /// 如果存在未应用的迁移，将自动执行迁移操作。
    /// 迁移完成后，会检查是否需要初始化种子数据。
    /// </para>
    /// </summary>
    /// <param name="app">WebApplication实例，用于获取服务提供者</param>
    /// <returns>表示异步初始化操作的任务</returns>
    /// <exception cref="Exception">
    /// 当迁移执行失败时抛出异常。异常会被记录到日志中，
    /// 并重新抛出以阻止应用继续启动。
    /// </exception>
    /// <remarks>
    /// <para>执行流程：</para>
    /// <list type="number">
    ///   <item><description>创建新的服务作用域</description></item>
    ///   <item><description>从容器中获取DbContext和Logger实例</description></item>
    ///   <item><description>检查是否存在待处理的迁移</description></item>
    ///   <item><description>如果有迁移：执行MigrateAsync()</description></item>
    ///   <item><description>如果没有迁移：跳过，记录日志</description></item>
    ///   <item><description>调用SeedDataAsync()初始化种子数据</description></item>
    /// </list>
    /// 
    /// <para>何时会抛出异常？</para>
    /// <list type="bullet">
    ///   <item><description>数据库连接失败（如连接字符串错误、权限不足）</description></item>
    ///   <item><description>SQL语法错误（迁移脚本有问题）</description></item>
    ///   <item><description>并发冲突（多个实例同时尝试迁移）</description></item>
    /// </list>
    /// </remarks>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            logger.LogInformation("Initializing database...");

            if (dbContext.Database.GetPendingMigrations().Any())
            {
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully");
            }
            else if (!dbContext.Database.CanConnect())
            {
                await dbContext.Database.EnsureCreatedAsync();
                logger.LogInformation("Database created successfully");
            }
            else
            {
                logger.LogInformation("Database is up to date, no migrations needed");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations");
            throw;
        }

        await SeedDataAsync(dbContext, logger);
    }

    /// <summary>
    /// 初始化种子数据
    /// <para>
    /// 检查数据库是否为空（无用户记录），如果是则插入初始数据。
    /// 种子数据通常包括：
    /// - 默认管理员账户
    /// - 系统配置项
    /// - 基础参考数据
    /// </para>
    /// </summary>
    /// <param name="dbContext">数据库上下文实例</param>
    /// <param name="logger">日志记录器实例</param>
    /// <returns>表示异步操作的任务</returns>
    /// <remarks>
    /// <para>幂等性保证：</para>
    /// <para>
    /// 此方法通过检查Users表是否为空来判断是否需要播种，
    /// 确保多次调用不会产生重复数据。
    /// 这种模式称为"幂等播种"（Idempotent Seeding）。
    /// </para>
    /// 
    /// <para>扩展建议：</para>
    /// <para>
    /// 可以根据业务需求在此方法中添加更多种子数据：
    /// <list type="bullet">
    ///   <item><description>创建默认角色和权限</description></item>
    ///   <item><description>插入系统配置参数</description></item>
    ///   <item><description>添加基础字典数据（如国家、地区列表）</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    private static async Task SeedDataAsync(AppDbContext dbContext, ILogger logger)
    {
        if (!await dbContext.Users.AnyAsync())
        {
            logger.LogInformation("Seeding initial data...");
            
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Initial data seeded successfully");
        }
    }
}
