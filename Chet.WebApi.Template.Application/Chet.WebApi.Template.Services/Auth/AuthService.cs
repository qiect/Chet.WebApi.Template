using AutoMapper;
using Chet.WebApi.Template.Configuration;
using Chet.WebApi.Template.Contracts.Auth;
using Chet.WebApi.Template.Contracts.Jwt;
using Chet.WebApi.Template.Contracts.User;
using Chet.WebApi.Template.Domain.User;
using Chet.WebApi.Template.DTOs.Auth;
using Chet.WebApi.Template.DTOs.User;
using Chet.WebApi.Template.Shared;
using Microsoft.Extensions.Logging;
using static BCrypt.Net.BCrypt;

namespace Chet.WebApi.Template.Services.Auth;

/// <summary>
/// 认证服务实现类，实现了 IAuthService 接口
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly AppSettings _appSettings;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    /// <param name="jwtService">JWT服务</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="appSettings">应用程序配置</param>
    public AuthService(
        IUserRepository userRepository,
        IJwtService jwtService,
        IMapper mapper,
        ILogger<AuthService> logger,
        AppSettings appSettings)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _mapper = mapper;
        _logger = logger;
        _appSettings = appSettings;
    }

    /// <inheritdoc />
    public async Task<JwtTokenDto> LoginAsync(LoginDto loginDto)
    {
        _logger.LogInformation("User login attempt: {Email}", loginDto.Email);

        // 根据邮箱获取用户
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        // 验证用户是否存在且密码正确
        if (user == null || !Verify(loginDto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Invalid login attempt: {Email}", loginDto.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // 生成访问令牌和刷新令牌
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // 更新用户的刷新令牌和过期时间
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.Now.AddDays(_appSettings.Jwt?.RefreshTokenExpirationDays ?? 7);
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("User login successful: {Email}", loginDto.Email);

        // 返回令牌
        return new JwtTokenDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    /// <inheritdoc />
    public async Task RegisterAsync(RegisterDto registerDto)
    {
        _logger.LogInformation("User registration attempt: {Email}", registerDto.Email);

        // 检查邮箱是否已存在
        var existingUser = await _userRepository.GetByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("User registration failed: Email already exists: {Email}", registerDto.Email);
            throw new BadRequestException("Email already exists");
        }

        // 将注册DTO映射为用户实体
        var user = _mapper.Map<UserEntity>(registerDto);
        // 对密码进行哈希处理
        user.PasswordHash = HashPassword(registerDto.Password);

        // 添加用户到数据库
        await _userRepository.AddAsync(user);

        _logger.LogInformation("User registration successful: {Email}", registerDto.Email);
    }

    /// <inheritdoc />
    public async Task<JwtTokenDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        _logger.LogInformation("Refresh token attempt");

        // 使用JWT服务刷新令牌
        var token = await _jwtService.RefreshTokenAsync(refreshTokenDto.AccessToken, refreshTokenDto.RefreshToken);
        return token;
    }
}
