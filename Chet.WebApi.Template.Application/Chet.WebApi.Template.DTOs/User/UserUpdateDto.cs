namespace Chet.WebApi.Template.DTOs.User;

/// <summary>
/// 用户更新数据传输对象，用于接收更新用户信息的请求
/// </summary>
public class UserUpdateDto
{
    /// <summary>
    /// 用户名，用于标识用户
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 用户邮箱，用于登录和通知
    /// </summary>
    public required string Email { get; set; }
}
