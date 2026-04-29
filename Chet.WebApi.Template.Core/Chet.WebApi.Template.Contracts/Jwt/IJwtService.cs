using Chet.WebApi.Template.Domain.User;
using Chet.WebApi.Template.DTOs.Auth;
using System.Security.Claims;

namespace Chet.WebApi.Template.Contracts.Jwt
{
    /// <summary>
    /// JWT服务接口，定义了JWT令牌相关的操作
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// 生成访问令牌
        /// </summary>
        /// <param name="user">用户实体</param>
        /// <returns>访问令牌字符串</returns>
        string GenerateAccessToken(UserEntity user);

        /// <summary>
        /// 生成刷新令牌
        /// </summary>
        /// <returns>刷新令牌字符串</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// 从过期的访问令牌中获取声明主体
        /// </summary>
        /// <param name="token">过期的访问令牌</param>
        /// <returns>声明主体</returns>
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);

        /// <summary>
        /// 使用刷新令牌获取新的访问令牌
        /// </summary>
        /// <param name="accessToken">当前访问令牌</param>
        /// <param name="refreshToken">刷新令牌</param>
        /// <returns>新的JWT令牌DTO</returns>
        Task<JwtTokenDto> RefreshTokenAsync(string accessToken, string refreshToken);
    }
}
