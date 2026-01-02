// <copyright file="Program.cs" company="Chet.WebApi.Template">
// Copyright (c) Chet.WebApi.Template. All rights reserved.
// </copyright>

using Chet.WebApi.Template.Data;
using Chet.WebApi.Template.Contracts;
using Chet.WebApi.Template.Services;
using Chet.WebApi.Template.Caching;
using Chet.WebApi.Template.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Chet.WebApi.Template.Shared;
using StackExchange.Redis;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// 应用程序入口点，配置服务和HTTP请求管道
/// </summary>
var builder = WebApplication.CreateBuilder(args);

// 加载应用程序配置
var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
builder.Services.AddSingleton(appSettings!);

// 添加控制器服务
builder.Services.AddControllers();

// 配置Swagger/OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    // 定义Swagger文档信息
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Chet.WebApi.Template", Version = "v1" });
    
    // 添加Bearer认证方案定义
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    // 添加Bearer认证要求
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// 添加数据库上下文服务
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 注册仓储服务
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfCoreRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();

// 注册业务逻辑服务
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// 根据Redis配置决定使用哪个缓存服务
if (appSettings?.Redis != null && appSettings.Redis.Enabled)
{
    builder.Services.AddScoped<ICacheService, RedisCacheService>();
}
else
{
    builder.Services.AddScoped<ICacheService, NoOpCacheService>();
}

// 配置AutoMapper，明确指定映射配置类所在的程序集
builder.Services.AddAutoMapper(typeof(Chet.WebApi.Template.Mapping.MappingProfile));


// 注册Redis连接服务，根据配置决定是否启用
if (appSettings?.Redis != null && appSettings.Redis.Enabled)
{
    var redisConnectionString = appSettings.Redis.ConnectionString ?? "localhost:6379";
    var configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
    configurationOptions.AbortOnConnectFail = false;
    builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(configurationOptions));
}

// 配置JWT身份认证或使用允许所有请求的认证方案
if (appSettings?.Jwt != null && appSettings.Jwt.Enabled)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = appSettings.Jwt.Issuer,
                ValidAudience = appSettings.Jwt.Audience,
                // 使用配置中的SecretKey，确保与生成令牌时使用相同的密钥
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Jwt.SecretKey ?? "DefaultJwtSecretKey"))
            };
        });
}
else
{
    // 当JWT禁用时，注册一个允许所有请求的认证方案
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "AllowAll";
    })
    .AddScheme<AuthenticationSchemeOptions, AllowAllAuthenticationHandler>("AllowAll", null);
}

// 构建Web应用程序
var app = builder.Build();

// 自动创建数据库
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

// 配置HTTP请求管道
if (app.Environment.IsDevelopment())
{
    // 开发环境启用Swagger
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 添加自定义异常处理中间件
app.UseExceptionHandler(options =>
{
    options.Run(async context =>
    {
        // 获取异常信息
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        if (exception == null)
        {
            return;
        }

        // 记录异常日志
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(exception, "An unexpected error occurred");

        // 设置默认错误状态码和消息
        var statusCode = StatusCodes.Status500InternalServerError;
        var message = "An unexpected error occurred";

        // 根据异常类型设置不同的状态码和消息
        if (exception is NotFoundException)
        {
            statusCode = StatusCodes.Status404NotFound;
            message = exception.Message;
        }
        else if (exception is BadRequestException)
        {
            statusCode = StatusCodes.Status400BadRequest;
            message = exception.Message;
        }
        else if (exception is UnauthorizedAccessException)
        {
            statusCode = StatusCodes.Status401Unauthorized;
            message = exception.Message;
        }

        // 构造统一格式的错误响应
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var errorResponse = Chet.WebApi.Template.Shared.ApiResponse.Error(message, statusCode);

        // 返回错误响应
        await context.Response.WriteAsJsonAsync(errorResponse);
    });
});

// 启用HTTPS重定向
app.UseHttpsRedirection();

// 根据JWT配置决定是否启用身份认证和授权中间件
if (appSettings?.Jwt != null && appSettings.Jwt.Enabled)
{
    // 添加身份认证中间件
    app.UseAuthentication();
    // 添加授权中间件
    app.UseAuthorization();
}

// 映射控制器路由
app.MapControllers();

// 根路径重定向到Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger"));

// 启动应用程序
app.Run();

/// <summary>
/// 允许所有请求的认证处理程序，当JWT禁用时使用
/// </summary>
public class AllowAllAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="optionsMonitor">选项监视器</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="encoder">URL编码器</param>
    /// <param name="clock">系统时钟</param>
    public AllowAllAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> optionsMonitor,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(optionsMonitor, logger, encoder, clock)
    { }

    /// <summary>
    /// 处理认证请求，允许所有请求通过
    /// </summary>
    /// <returns>认证结果</returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // 创建一个包含默认声明的身份
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "Anonymous"),
            new Claim(ClaimTypes.Role, "Guest")
        };

        // 创建身份和认证票据
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        // 返回成功的认证结果
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
