using Microsoft.AspNetCore.Mvc;

namespace Chet.WebApi.Template.Api.Controllers;

/// <summary>
/// 健康检查控制器
/// <para>
/// 提供应用健康状态检查端点，用于监控和运维场景。
/// 支持Kubernetes/Docker的存活探针（Liveness）和就绪探针（Readiness）。
/// </para>
/// </summary>
/// <remarks>
/// <para>端点说明：</para>
/// <list type="bullet">
///   <item><description><c>GET /api/v1/health</c> - 存活探针，检查应用进程是否运行</description></item>
///   <item><description><c>GET /api/v1/health/ready</c> - 就绪探针，检查应用是否准备好接收流量</description></item>
/// </list>
/// 
/// <para>使用场景：</para>
/// <list type="number">
///   <item><description>Kubernetes/Docker容器健康检查</description></item>
///   <item><description>负载均衡器后端服务探测</description></item>
///   <item><description>监控系统（Prometheus、Zabbix等）</description></item>
/// </list>
/// </remarks>
[ApiController]
[Route("api/v{version:apiVersion}/health")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// 日志记录器实例
    /// </summary>
    private readonly ILogger<HealthController> _logger;

    /// <summary>
    /// 初始化健康检查控制器的新实例
    /// </summary>
    /// <param name="logger">日志记录器，用于记录健康检查过程中的信息</param>
    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 存活探针（Liveness Probe）
    /// <para>
    /// 检查应用进程是否正常运行。此端点仅验证应用是否在运行，
    /// 不检查外部依赖（数据库、Redis等）的状态。
    /// </para>
    /// </summary>
    /// <returns>
    /// 始终返回200 OK，包含应用状态、时间戳和版本信息
    /// </returns>
    /// <remarks>
    /// <para>典型用途：</para>
    /// <list type="bullet">
    ///   <item><description>Docker HEALTHCHECK指令</description></item>
    ///   <item><description>Kubernetes livenessProbe配置</description></item>
    ///   <item><description>负载均衡器心跳检测</description></item>
    /// </list>
    /// 
    /// <para>响应示例：</para>
    /// <code>
    /// {
    ///     "status": "Healthy",
    ///     "timestamp": "2026-04-29T10:30:00Z",
    ///     "version": "v1"
    /// }
    /// </code>
    /// </remarks>
    /// <response code="200">应用运行正常</response>
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public IActionResult Liveness()
    {
        return Ok(new HealthResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "v1"
        });
    }

    /// <summary>
    /// 就绪探针（Readiness Probe）
    /// <para>
    /// 检查应用是否已完全启动并准备好接收请求。
    /// 此端点会验证所有关键依赖项（数据库、Redis缓存）的连接状态。
    /// 只有当所有依赖都正常时，才返回健康状态。
    /// </para>
    /// </summary>
    /// <returns>
    /// <list type="bullet">
    ///   <item><description>200 OK - 所有依赖正常，应用就绪</description></item>
    ///   <item><description>503 Service Unavailable - 存在依赖故障，应用未就绪</description></item>
    /// </list>
    /// </returns>
    /// <param name="serviceProvider">
    /// 依赖注入服务提供者，用于获取数据库上下文和缓存服务的实例
    /// </param>
    /// <remarks>
    /// <para>检查项目：</para>
    /// <list type="table">
    ///   <listheader>
    ///     <term>依赖项</term>
    ///     <description>检查内容</description>
    ///   </listheader>
    ///   <item>
    ///     <term>Database</term>
    ///     <description>验证数据库连接是否可用</description>
    ///   </item>
    ///   <item>
    ///     <term>Redis</term>
    ///     <description>验证Redis连接是否可用（如已启用）</description>
    ///   </item>
    /// </list>
    /// 
    /// <para>响应示例（健康）：</para>
    /// <code>
    /// {
    ///     "status": "Healthy",
    ///     "timestamp": "2026-04-29T10:30:00Z",
    ///     "checks": {
    ///         "database": true,
    ///         "redis": true
    ///     }
    /// }
    /// </code>
    /// 
    /// <para>响应示例（不健康）：</para>
    /// <code>
    /// {
    ///     "status": "Unhealthy",
    ///     "timestamp": "2026-04-29T10:30:00Z",
    ///     "checks": {
    ///         "database": false,
    ///         "redis": true
    ///     }
    /// }
    /// </code>
    /// </remarks>
    /// <response code="200">所有依赖正常，应用就绪</response>
    /// <response code="503">存在依赖故障，应用未就绪</response>
    [HttpGet("ready")]
    [ProducesResponseType(typeof(HealthReadyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthReadyResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Readiness([FromServices] IServiceProvider serviceProvider)
    {
        var checks = new Dictionary<string, bool>();

        try
        {
            await CheckDatabaseAsync(serviceProvider, checks);
            await CheckRedisAsync(serviceProvider, checks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
        }

        var isHealthy = checks.Values.All(v => v);
        var response = new HealthReadyResponse
        {
            Status = isHealthy ? "Healthy" : "Unhealthy",
            Timestamp = DateTime.UtcNow,
            Checks = checks
        };

        return isHealthy ? Ok(response) : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }

    /// <summary>
    /// 检查数据库连接是否可用
    /// </summary>
    /// <param name="serviceProvider">依赖注入服务提供者</param>
    /// <param name="checks">检查结果字典，用于存储各组件的健康状态</param>
    /// <returns>表示异步操作的任务</returns>
    /// <remarks>
    /// 通过调用 DbContext.Database.CanConnectAsync() 来验证数据库连接。
    /// 如果无法连接或超时，会将 database 标记为不可用。
    /// </remarks>
    private static async Task CheckDatabaseAsync(IServiceProvider serviceProvider, Dictionary<string, bool> checks)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetService<Microsoft.EntityFrameworkCore.DbContext>();
            if (context != null)
            {
                await context.Database.CanConnectAsync();
                checks["database"] = true;
            }
            else
            {
                checks["database"] = false;
            }
        }
        catch
        {
            checks["database"] = false;
        }
    }

    /// <summary>
    /// 检查Redis缓存连接是否可用
    /// </summary>
    /// <param name="serviceProvider">依赖注入服务提供者</param>
    /// <param name="checks">检查结果字典，用于存储各组件的健康状态</param>
    /// <returns>表示异步操作的任务</returns>
    /// <remarks>
    /// 通过调用 ICacheService.PingAsync() 来验证Redis连接。
    /// 如果使用的是 NoOpCacheService（未启用Redis），则始终返回健康状态。
    /// </remarks>
    private static async Task CheckRedisAsync(IServiceProvider serviceProvider, Dictionary<string, bool> checks)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var cacheService = scope.ServiceProvider.GetService<Chet.WebApi.Template.Contracts.Cache.ICacheService>();
            if (cacheService != null)
            {
                await cacheService.PingAsync();
                checks["redis"] = true;
            }
            else
            {
                checks["redis"] = true;
            }
        }
        catch
        {
            checks["redis"] = false;
        }
    }
}

/// <summary>
/// 存活探针响应模型
/// <para>
/// 用于表示应用的基本运行状态信息，
/// 包含状态、时间戳和API版本号。
/// </para>
/// </summary>
public class HealthResponse
{
    /// <summary>
    /// 获取或设置健康状态
    /// <para>可能的值：Healthy（健康）、Unhealthy（不健康）</para>
    /// </summary>
    public string Status { get; set; } = "Healthy";

    /// <summary>
    /// 获取或设置检查时间戳（UTC时间）
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 获取或设置API版本号
    /// <para>例如：v1、v2</para>
    /// </summary>
    public string Version { get; set; } = "v1";
}

/// <summary>
/// 就绪探针响应模型
/// <para>
/// 用于表示应用的完整就绪状态信息，
/// 包含状态、时间戳以及各个依赖组件的详细检查结果。
/// </para>
/// </summary>
public class HealthReadyResponse
{
    /// <summary>
    /// 获取或设置整体健康状态
    /// <para>只有当所有Checks中的值都为true时，状态才为Healthy</para>
    /// </summary>
    public string Status { get; set; } = "Healthy";

    /// <summary>
    /// 获取或设置检查时间戳（UTC时间）
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 获取或设置各依赖组件的检查结果
    /// <para>
    /// 键：组件名称（如 database、redis）
    /// 值：该组件是否可用（true=可用，false=不可用）
    /// </para>
    /// </summary>
    public Dictionary<string, bool> Checks { get; set; } = new();
}
