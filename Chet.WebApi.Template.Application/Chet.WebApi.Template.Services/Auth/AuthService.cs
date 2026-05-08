using AutoMapper;
using Chet.WebApi.Template.Configuration;
using Chet.WebApi.Template.Contracts;
using Chet.WebApi.Template.Contracts.Auth;
using Chet.WebApi.Template.Contracts.Jwt;
using Chet.WebApi.Template.Contracts.Security;
using Chet.WebApi.Template.Contracts.User;
using Chet.WebApi.Template.Domain.User;
using Chet.WebApi.Template.DTOs.Auth;
using Chet.WebApi.Template.DTOs.User;
using Chet.WebApi.Template.Shared;
using Microsoft.Extensions.Logging;

namespace Chet.WebApi.Template.Services.Auth;

/// <summary>
/// 认证服务实现类，实现了 IAuthService 接口
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly AppSettings _appSettings;

    public AuthService(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IPasswordService passwordService,
        IMapper mapper,
        ILogger<AuthService> logger,
        AppSettings appSettings)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _passwordService = passwordService;
        _mapper = mapper;
        _logger = logger;
        _appSettings = appSettings;
    }

    public async Task<JwtTokenDto> LoginAsync(LoginDto loginDto)
    {
        _logger.LogInformation("User login attempt: {Email}", loginDto.Email);

        var user = await _unitOfWork.Users.GetByEmailAsync(loginDto.Email);
        
        if (user == null || !_passwordService.Verify(loginDto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Invalid login attempt: {Email}", loginDto.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        using var transaction = await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            // 使用 UTC 时间计算过期时间，确保跨时区一致性
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_appSettings.Jwt?.RefreshTokenExpirationDays ?? 7);

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("User login successful: {Email}", loginDto.Email);

            return new JwtTokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for: {Email}", loginDto.Email);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task RegisterAsync(RegisterDto registerDto)
    {
        _logger.LogInformation("User registration attempt: {Email}", registerDto.Email);

        var existingUser = await _unitOfWork.Users.GetByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("User registration failed: Email already exists: {Email}", registerDto.Email);
            throw new BadRequestException("Email already exists");
        }

        using var transaction = await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            var user = _mapper.Map<UserEntity>(registerDto);
            user.PasswordHash = _passwordService.Hash(registerDto.Password);

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("User registration successful: {Email}", registerDto.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for: {Email}", registerDto.Email);
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<JwtTokenDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        _logger.LogInformation("Refresh token attempt");

        var token = await _jwtService.RefreshTokenAsync(refreshTokenDto.AccessToken, refreshTokenDto.RefreshToken);
        return token;
    }
}
