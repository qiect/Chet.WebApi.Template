namespace Chet.WebApi.Template.Domain.User;

/// <summary>
/// 用户实体类，继承自 BaseEntity
/// </summary>
public class UserEnitity : BaseEntity
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 用户邮箱，用于登录和通知
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// 密码哈希值，使用 BCrypt 算法生成
    /// </summary>
    public string PasswordHash { get; set; }

    /// <summary>
    /// 刷新令牌，用于获取新的访问令牌
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// 刷新令牌过期时间
    /// </summary>
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
