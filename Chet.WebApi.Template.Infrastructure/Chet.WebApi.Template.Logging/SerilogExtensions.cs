using Microsoft.Extensions.Hosting;
using Serilog;

namespace Chet.WebApi.Template.Logging
{
    /// <summary>
    /// Serilog扩展类，提供配置Serilog日志的扩展方法
    /// </summary>
    public static class SerilogExtensions
    {
        /// <summary>
        /// 配置Serilog日志，从应用程序配置中读取日志配置
        /// </summary>
        /// <param name="context">主机构建上下文，用于访问应用程序配置</param>
        /// <param name="configuration">Serilog日志配置对象</param>
        public static void ConfigureSerilog(this HostBuilderContext context, LoggerConfiguration configuration)
        {
            configuration.ReadFrom.Configuration(context.Configuration);
        }
    }
}
