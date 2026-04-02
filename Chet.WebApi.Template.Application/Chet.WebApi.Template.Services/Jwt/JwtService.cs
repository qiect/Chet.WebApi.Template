using Chet.WebApi.Template.Configuration;
using Chet.WebApi.Template.Contracts.Jwt;
using Chet.WebApi.Template.Contracts.User;
using Chet.WebApi.Template.Domain;
using Chet.WebApi.Template.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Chet.WebApi.Template.Services;

/// <summary>
/// JWT 服务实现类，实现了 IJwtService 接口
/// </summary>
public class JwtService : IJwtService
{
    private readonly AppSettings _appSettings;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<JwtService> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="appSettings">应用程序配置</param>
    /// <param name="userRepository">用户仓储</param>
    /// <param name="logger">日志记录器</param>
    public JwtService(AppSettings appSettings, IUserRepository userRepository, ILogger<JwtService> logger)
    {
        _appSettings = appSettings;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public string GenerateAccessToken(UserEnitity user)
    {
        _logger.LogInformation("Generating access token for user: {Email}", user.Email);

        // 定义 JWT 声明
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // 主题，用户ID
            new Claim(JwtRegisteredClaimNames.Email, user.Email), // 邮箱
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // JWT ID，唯一标识符
        };

        // 使用框架中设计好的AppSettings配置
        var jwtSettings = _appSettings.Jwt ?? new JwtSettings();
        var jwtKey = jwtSettings.SecretKey ?? jwtSettings.Key ?? "DefaultJwtSecretKey";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 创建 JWT 令牌
        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer, // 发行者
            audience: jwtSettings.Audience, // 受众
            claims: claims, // 声明
            expires: DateTime.Now.AddMinutes(jwtSettings.AccessTokenExpirationMinutes > 0 ? jwtSettings.AccessTokenExpirationMinutes : 30), // 过期时间
            signingCredentials: creds); // 签名凭据

        // 生成 JWT 字符串
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc />
    public string GenerateRefreshToken()
    {
        _logger.LogInformation("Generating refresh token");

        // 使用加密安全的随机数生成器生成 32 字节的随机数
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber); // 转换为 Base64 字符串
        }
    }

    /// <inheritdoc />
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        _logger.LogInformation("Getting principal from expired token");

        // 使用框架中设计好的AppSettings配置
        var jwtSettings = _appSettings.Jwt ?? new JwtSettings();
        var jwtKey = jwtSettings.SecretKey ?? jwtSettings.Key ?? "DefaultJwtSecretKey";

        // 配置令牌验证参数，不验证过期时间
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = false // 不验证令牌过期时间
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        // 验证令牌算法是否为 HmacSha256
        if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }

    /// <inheritdoc />
    public async Task<JwtTokenDto> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        _logger.LogInformation("Refreshing token");

        // 从过期令牌获取声明主体
        var principal = GetPrincipalFromExpiredToken(accessToken);
        // 获取用户ID
        var userId = Convert.ToInt32(principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value);
        // 获取用户信息
        var user = await _userRepository.GetByIdAsync(userId);

        // 验证刷新令牌
        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime < DateTime.Now)
        {
            throw new SecurityTokenException("Invalid refresh token");
        }

        // 生成新的访问令牌和刷新令牌
        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();

        // 更新用户的刷新令牌和过期时间
        user.RefreshToken = newRefreshToken;
        var jwtSettings = _appSettings.Jwt ?? new JwtSettings();
        user.RefreshTokenExpiryTime = DateTime.Now.AddDays(jwtSettings.RefreshTokenExpirationDays > 0 ? jwtSettings.RefreshTokenExpirationDays : 7);
        await _userRepository.UpdateAsync(user);

        // 返回新的令牌对
        return new JwtTokenDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }
}
