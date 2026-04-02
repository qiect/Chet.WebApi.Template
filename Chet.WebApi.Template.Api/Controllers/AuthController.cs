using Chet.WebApi.Template.Contracts.Auth;
using Chet.WebApi.Template.DTOs;
using Chet.WebApi.Template.Shared;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Chet.WebApi.Template.Api.Controllers;

/// <summary>
/// 认证控制器，处理用户注册、登录和令牌刷新请求
/// </summary>
[ApiController]
[Route("api/[controller]")]
[SwaggerTag("提供用户认证相关的API接口，包括注册、登录和令牌刷新")]
public class AuthController : ControllerBase
{
    /// <summary>
    /// 认证服务，用于处理认证相关的业务逻辑
    /// </summary>
    private readonly IAuthService _authService;

    /// <summary>
    /// 日志记录器，用于记录控制器操作日志
    /// </summary>
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="authService">认证服务</param>
    /// <param name="logger">日志记录器</param>
    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// 用户注册接口
    /// </summary>
    /// <param name="registerDto">注册信息DTO，包含用户邮箱、密码和名称</param>
    /// <returns>注册成功返回201状态码，失败返回400状态码</returns>
    /// <remarks>
    /// 示例请求：
    /// 
    ///     POST /api/Auth/register
    ///     {
    ///         "email": "user@example.com",
    ///         "password": "SecurePassword123!",
    ///         "name": "John Doe"
    ///     }
    /// </remarks>
    /// <response code="201">注册成功</response>
    /// <response code="400">注册失败，邮箱已存在或输入无效</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        _logger.LogInformation("User registration attempt with email: {Email}", registerDto.Email);
        await _authService.RegisterAsync(registerDto);
        return Created("", ApiResponse.Ok(null, "User registered successfully", StatusCodes.Status201Created));
    }

    /// <summary>
    /// 用户登录接口
    /// </summary>
    /// <param name="loginDto">登录信息DTO，包含用户邮箱和密码</param>
    /// <returns>登录成功返回JWT令牌，失败返回401状态码</returns>
    /// <remarks>
    /// 示例请求：
    /// 
    ///     POST /api/Auth/login
    ///     {
    ///         "email": "user@example.com",
    ///         "password": "SecurePassword123!"
    ///     }
    /// </remarks>
    /// <response code="200">登录成功，返回JWT令牌</response>
    /// <response code="401">登录失败，邮箱或密码错误</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        _logger.LogInformation("User login attempt with email: {Email}", loginDto.Email);
        var token = await _authService.LoginAsync(loginDto);
        return Ok(ApiResponse.Ok(token, "Login successful"));
    }

    /// <summary>
    /// 刷新令牌接口
    /// </summary>
    /// <param name="refreshTokenDto">刷新令牌信息DTO，包含访问令牌和刷新令牌</param>
    /// <returns>刷新成功返回新的JWT令牌，失败返回401状态码</returns>
    /// <remarks>
    /// 示例请求：
    /// 
    ///     POST /api/Auth/refresh-token
    ///     {
    ///         "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    ///         "refreshToken": "dGVzdHJlZnNlcnZpY2U="
    ///     }
    /// </remarks>
    /// <response code="200">令牌刷新成功，返回新的JWT令牌</response>
    /// <response code="401">令牌刷新失败，刷新令牌无效或已过期</response>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(RefreshTokenDto refreshTokenDto)
    {
        _logger.LogInformation("Token refresh attempt");
        var token = await _authService.RefreshTokenAsync(refreshTokenDto);
        return Ok(ApiResponse.Ok(token, "Token refreshed successfully"));
    }
}