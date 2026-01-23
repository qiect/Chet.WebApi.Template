using Serilog;

namespace Chet.WebApi.Template.Api.Configurations;

/// <summary>
/// Serilog配置类
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// 配置Serilog
    /// </summary>
    /// <param name="builder">WebApplicationBuilder实例</param>
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration)
        );
    }
}
