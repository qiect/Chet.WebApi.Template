namespace Chet.WebApi.Template.DTOs;

/// <summary>
/// JWT令牌数据传输对象，用于在客户端和服务器之间传输令牌信息
/// </summary>
public class JwtTokenDto
{
    /// <summary>
    /// JWT访问令牌，用于API请求的身份认证
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// 刷新令牌，用于在访问令牌过期时获取新的访问令牌
    /// </summary>
    public string? RefreshToken { get; set; }
}
