// <copyright file="Program.cs" company="Chet.WebApi.Template">
// Copyright (c) Chet.WebApi.Template. All rights reserved.
// </copyright>

using Chet.WebApi.Template.Api.Configurations;
using Chet.WebApi.Template.Api.Middleware;
using Chet.WebApi.Template.Configuration;
using Chet.WebApi.Template.Mapping.User;
using Serilog;

/// <summary>
/// ASP.NET Core Web API 应用程序入口点
/// <para>
/// 本文件是应用程序的启动配置中心，负责：
/// - 初始化日志系统（Serilog）
/// - 配置依赖注入服务
/// - 构建中间件管道
/// - 启动HTTP服务器监听
/// </para>
/// </summary>
/// <remarks>
/// <para>项目架构概览：</para>
/// <para>
/// 本项目采用分层架构（Layered Architecture）设计模式：
/// <list type="table">
///   <listheader>
///     <term>层</term>
///     <description>职责</description>
///   </listheader>
///   <item>
///     <term>Api（表示层）</term>
///     <description>控制器、中间件、配置、DTO</description>
///   </item>
///   <item>
///     <term>Application（应用层）</term>
///     <description>业务逻辑、服务、映射、DTO定义</description>
///   </item>
///   <item>
///     <term>Infrastructure（基础设施层）</term>
///     <description>数据访问、缓存、日志、外部服务集成</description>
///   </item>
///   <item>
///     <term>Core（核心层）</term>
///     <description>领域实体、接口、共享工具、契约</description>
///   </item>
/// </list>
/// 
/// <para>技术栈：</para>
/// <list type="bullet">
///   <item><description>.NET 10 + C# 12</description></item>
///   <item><description>Entity Framework Core 8/9 (SQLite)</description></item>
///   <item><description>Redis缓存（可选，支持NoOp降级）</description></item>
///   <item><description>JWT身份认证</description></item>
///   <item><description>AutoMapper对象映射</description></item>
///   <item><description>Serilog结构化日志</description></item>
///   <item><description>Swagger/OpenAPI文档</description></item>
///   <item><description>Docker容器化部署</description></item>
/// </list>
/// </para>
/// </remarks>

// ============================================
// 第一阶段：初始化和前置检查
// ============================================

Log.Information("Starting application...");

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureSerilog();


var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
builder.Services.AddSingleton(appSettings!);

// ============================================
// 第二阶段：服务注册（依赖注入配置）
// ============================================

builder.Services.AddControllers();

builder.Services.ConfigureSwagger();

builder.Services.ConfigureDatabase(builder.Configuration);

builder.Services.ConfigureRedis(appSettings);

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.ConfigureRepositories();
builder.Services.ConfigureServices();

builder.Services.ConfigureJwt(appSettings);

builder.Services.ConfigureApiVersioning();

builder.Services.ConfigureCors(builder.Configuration);

// ============================================
// 第三阶段：构建应用实例
// ============================================

var app = builder.Build();

// ============================================
// 第四阶段：数据库初始化
// ============================================

await app.InitializeDatabaseAsync();

// ============================================
// 第五阶段：中间件管道配置
// ============================================

app.ConfigureExceptionHandling();

app.UseCors("DefaultPolicy");

app.UseRateLimiting();

app.UseHttpsRedirection();

app.ConfigureSwaggerUI();

app.ConfigureAuthMiddleware(appSettings);

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"));

// ============================================
// 第六阶段：启动应用
// ============================================

Log.Information("Application started successfully. Listening on {Urls}", app.Urls);
app.Run();
