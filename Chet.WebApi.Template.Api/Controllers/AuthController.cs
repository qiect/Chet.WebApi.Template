using Chet.WebApi.Template.Contracts;
using Chet.WebApi.Template.DTOs;
using Chet.WebApi.Template.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Chet.WebApi.Template.Api.Controllers;

/// <summary>
/// 认证控制器，处理用户注册、登录和令牌刷新请求
/// </summary>
[ApiController]
[Route("api/[controller]")]
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
    /// <param name="registerDto">注册信息DTO</param>
    /// <returns>注册结果</returns>
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
    /// <param name="loginDto">登录信息DTO</param>
    /// <returns>JWT令牌</returns>
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
    /// <param name="refreshTokenDto">刷新令牌信息DTO</param>
    /// <returns>新的JWT令牌</returns>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(RefreshTokenDto refreshTokenDto)
    {
        _logger.LogInformation("Token refresh attempt");
        var token = await _authService.RefreshTokenAsync(refreshTokenDto);
        return Ok(ApiResponse.Ok(token, "Token refreshed successfully"));
    }
}