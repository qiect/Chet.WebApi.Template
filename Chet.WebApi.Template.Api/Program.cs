// <copyright file="Program.cs" company="Chet.WebApi.Template">
// Copyright (c) Chet.WebApi.Template. All rights reserved.
// </copyright>

using Chet.WebApi.Template.Api.Configurations;
using Chet.WebApi.Template.Configuration;
using Chet.WebApi.Template.Mapping;
using Serilog;


Log.Information("Starting application...");
Log.Information("Creating WebApplicationBuilder...");
var builder = WebApplication.CreateBuilder(args);
Log.Information("WebApplicationBuilder created successfully.");

// 配置Serilog
builder.ConfigureSerilog();

// 加载应用程序配置
var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
builder.Services.AddSingleton(appSettings!);

// 添加控制器服务
builder.Services.AddControllers();

// 配置Swagger
builder.Services.ConfigureSwagger();

// 配置数据库
builder.Services.ConfigureDatabase(builder.Configuration);

// 配置Redis缓存
builder.Services.ConfigureRedis(appSettings);

// 配置AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// 配置仓储服务
builder.Services.ConfigureRepositories();

// 配置业务逻辑服务
builder.Services.ConfigureServices();

// 配置JWT认证
builder.Services.ConfigureJwt(appSettings);

// 构建Web应用程序
Log.Information("Building web application...");
var app = builder.Build();
Log.Information("Web application built successfully.");

// 自动创建数据库
Log.Information("Creating database if not exists...");
app.InitializeDatabase();
Log.Information("Database creation completed.");

// 配置HTTP请求管道

// 1. 异常处理中间件（应在最前面）
Log.Information("Adding exception handling middleware...");
app.ConfigureExceptionHandling();
Log.Information("Exception handling middleware added.");

// 2. HTTPS重定向
app.UseHttpsRedirection();

// 3. Swagger UI（仅在开发环境）
app.ConfigureSwaggerUI();

// 4. 认证和授权中间件
Log.Information("Configuring authentication and authorization...");
app.ConfigureAuthMiddleware(appSettings);
Log.Information("Authentication and authorization configured.");

// 5. 控制器路由
app.MapControllers();

// 6. 根路径重定向到Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger"));

// 启动应用程序
Log.Information("Starting web application...");
app.Run();
