using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;

namespace Chet.WebApi.Template.Api.Configurations;

/// <summary>
/// Swagger配置类
/// </summary>
public static class SwaggerConfiguration
{
    /// <summary>
    /// 配置Swagger
    /// </summary>
    /// <param name="services">IServiceCollection实例</param>
    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            // 定义Swagger文档信息
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Chet.WebApi.Template",
                Version = "v1",
                Description = "基于.NET 10的WebAPI模板框架，提供用户认证和管理功能"
            });

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

            // 包含XML注释文件
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // 启用Swagger注释
            c.EnableAnnotations();
        });
    }

    /// <summary>
    /// 配置Swagger UI
    /// </summary>
    /// <param name="app">WebApplication实例</param>
    public static void ConfigureSwaggerUI(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // 开发环境启用Swagger
            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }
}
