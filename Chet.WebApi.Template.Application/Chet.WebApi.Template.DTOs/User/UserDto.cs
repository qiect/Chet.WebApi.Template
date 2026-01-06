namespace Chet.WebApi.Template.DTOs;

/// <summary>
/// 用户数据传输对象，用于返回用户信息给客户端
/// </summary>
public class UserDto
{
    /// <summary>
    /// 用户ID，唯一标识符
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 用户邮箱
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// 用户创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 用户信息更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
