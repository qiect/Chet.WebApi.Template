using Chet.WebApi.Template.Contracts.Auth;
using Chet.WebApi.Template.DTOs.Auth;
using Chet.WebApi.Template.DTOs.User;
using Chet.WebApi.Template.Shared;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Chet.WebApi.Template.Api.Controllers;

/// <summary>
/// 认证控制器（Authentication Controller）
/// <para>
/// 处理用户身份认证相关的HTTP请求，包括用户注册、登录和令牌刷新操作。
/// 使用JWT（JSON Web Token）作为无状态的身份验证机制。
/// </para>
/// </summary>
/// <remarks>
/// <para>端点列表：</para>
/// <list type="table">
///   <listheader>
///     <term>方法</term>
///     <description>端点</description>
///     <description>说明</description>
///   </listheader>
///   <item>
///     <term>POST</term>
///     <description>/api/v1/auth/register</description>
///     <description>注册新用户账户</description>
///   </item>
///   <item>
///     <term>POST</term>
///     <description>/api/v1/auth/login</description>
///     <description>用户登录获取JWT令牌</description>
///   </item>
///   <item>
///     <term>POST</term>
///     <description>/api/v1/auth/refresh-token</description>
///     <description>使用刷新令牌获取新的访问令牌</description>
///   </item>
/// </list>
/// 
/// <para>安全特性：</para>
/// <list type="bullet">
///   <item><description>登录接口受限流保护：每IP每分钟最多5次请求</description></item>
///   <item><description>注册接口受限流保护：每IP每分钟最多10次请求</description></item>
///   <item><description>密码使用BCrypt加密存储，不可逆</description></item>
///   <item><description>JWT令牌包含过期时间，支持滑动续期</description></item>
/// </list>
/// 
/// <para>认证流程：</para>
/// <orderedlist>
///   <item><description>客户端发送登录凭据到 /login 端点</description></item>
///   <item><description>服务端验证凭据后签发JWT Access Token + Refresh Token</description></item>
///   <item><description>后续请求在Authorization头中携带Bearer Token</description></item>
///   <item><description>Access Token过期后使用Refresh Token换取新Token</description></item>
/// </orderedlist>
/// </remarks>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[SwaggerTag("提供用户认证相关的API接口，包括注册、登录和令牌刷新")]
public class AuthController : ControllerBase
{
    /// <summary>
    /// 认证服务实例
    /// <para>负责处理业务逻辑：密码验证、令牌生成、用户创建等</para>
    /// </summary>
    private readonly IAuthService _authService;

    /// <summary>
    /// 日志记录器实例
    /// <para>用于记录认证事件、安全警告和错误信息</para>
    /// </summary>
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// 初始化认证控制器的新实例
    /// </summary>
    /// <param name="authService">认证服务接口，提供注册、登录、令牌刷新等业务功能</param>
    /// <param name="logger">日志记录器，用于记录操作日志和安全审计信息</param>
    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// 用户注册接口
    /// <para>
    /// 创建新的用户账户。系统会对输入数据进行验证，
    /// 包括邮箱格式、密码强度等。密码会使用BCrypt算法进行哈希处理后存储。
    /// </para>
    /// </summary>
    /// <param name="registerDto">注册信息数据传输对象，包含姓名、邮箱、密码</param>
    /// <returns>
    /// <list type="bullet">
    ///   <item><description>201 Created - 注册成功，返回成功消息</description></item>
    ///   <item><description>400 Bad Request - 输入数据验证失败或邮箱已存在</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>请求示例：</para>
    /// <code>
    /// POST /api/v1/auth/register
    /// Content-Type: application/json
    /// 
    /// {
    ///   "name": "张三",
    ///   "email": "zhangsan@example.com",
    ///   "password": "MySecure@123"
    /// }
    /// </code>
    /// 
    /// <para>响应示例（201）：</para>
    /// <code>
    /// {
    ///   "success": true,
    ///   "message": "User registered successfully",
    ///   "data": null,
    ///   "statusCode": 201
    /// }
    /// </code>
    /// </remarks>
    /// <response code="201">注册成功</response>
    /// <response code="400">请求参数无效或邮箱已存在</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        await _authService.RegisterAsync(registerDto);
        return Created("", ApiResponse.Ok(null, "User registered successfully", StatusCodes.Status201Created));
    }

    /// <summary>
    /// 用户登录接口
    /// <para>
    /// 验证用户凭据并签发JWT令牌对。
    /// 成功登录后会返回Access Token（短期有效）和Refresh Token（长期有效）。
    /// Access Token用于API调用认证，Refresh Token用于续期。
    /// </para>
    /// </summary>
    /// <param name="loginDto">登录信息数据传输对象，包含邮箱和密码</param>
    /// <returns>
    /// <list type="bullet">
    ///   <item><description>200 OK - 登录成功，返回JWT令牌对</description></item>
    ///   <item><description>401 Unauthorized - 邮箱或密码不正确</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>请求示例：</para>
    /// <code>
    /// POST /api/v1/auth/login
    /// Content-Type: application/json
    /// 
    /// {
    ///   "email": "zhangsan@example.com",
    ///   "password": "MySecure@123"
    /// }
    /// </code>
    /// 
    /// <para>响应示例（200）：</para>
    /// <code>
    /// {
    ///   "success": true,
    ///   "message": "Login successful",
    ///   "data": {
    ///     "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    ///     "refreshToken": "rt_xxxxx...",
    ///     "expiresIn": 3600
    ///   },
    ///   "statusCode": 200
    /// }
    /// </code>
    /// 
    /// <para>限流规则：</para>
    /// <para>每个IP地址每分钟最多5次登录尝试，超限返回429状态码。</para>
    /// </remarks>
    /// <response code="200">登录成功，返回JWT令牌</response>
    /// <response code="401">认证失败，邮箱或密码错误</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var token = await _authService.LoginAsync(loginDto);
        return Ok(ApiResponse.Ok(token, "Login successful"));
    }

    /// <summary>
    /// 刷新令牌接口
    /// <para>
    /// 使用有效的Refresh Token来获取新的Access Token。
    /// 当Access Token过期时，客户端应调用此接口进行无感续期，
    /// 避免用户需要重新登录。
    /// </para>
    /// </summary>
    /// <param name="refreshTokenDto">刷新令牌数据传输对象，包含有效的Refresh Token</param>
    /// <returns>
    /// <list type="bullet">
    ///   <item><description>200 OK - 刷新成功，返回新的令牌对</description></item>
    ///   <item><description>401 Unauthorized - Refresh Token无效或已过期</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>请求示例：</para>
    /// <code>
    /// POST /api/v1/auth/refresh-token
    /// Content-Type: application/json
    /// 
    /// {
    ///   "refreshToken": "rt_xxxxx..."
    /// }
    /// </code>
    /// 
    /// <para>响应示例（200）：</para>
    /// <code>
    /// {
    ///   "success": true,
    ///   "message": "Token refreshed successfully",
    ///   "data": {
    ///     "accessToken": "eyJhbGciOiJIUzI1NiIs...(new)",
    ///     "refreshToken": "rt_yyyyy...(new)",
    ///     "expiresIn": 3600
    ///   },
    ///   "statusCode": 200
    /// }
    /// </code>
    /// 
    /// <para>安全建议：</para>
    /// <list type="bullet">
    ///   <item><description>Refresh Token应安全存储（HttpOnly Cookie或安全存储）</description></item>
    ///   <item><description>每次刷新后旧Refresh Token应失效（单次使用）</description></item>
    ///   <item><description>检测到异常刷新行为时应撤销所有Token</description></item>
    /// </list>
    /// </remarks>
    /// <response code="200">令牌刷新成功</response>
    /// <response code="401">Refresh Token无效或已过期</response>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        var token = await _authService.RefreshTokenAsync(refreshTokenDto);
        return Ok(ApiResponse.Ok(token, "Token refreshed successfully"));
    }
}
