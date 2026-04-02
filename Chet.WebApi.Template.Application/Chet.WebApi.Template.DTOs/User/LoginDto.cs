using System.ComponentModel.DataAnnotations;

namespace Chet.WebApi.Template.DTOs.User;

/// <summary>
/// 用户登录数据传输对象，用于接收用户登录请求
/// </summary>
public class LoginDto
{
    /// <summary>
    /// 用户邮箱，用于登录认证
    /// </summary>
    [Required(ErrorMessage = "邮箱不能为空")]
    [EmailAddress(ErrorMessage = "请输入有效的邮箱地址")]
    public required string Email { get; set; }

    /// <summary>
    /// 用户密码，用于登录认证
    /// </summary>
    [Required(ErrorMessage = "密码不能为空")]
    public required string Password { get; set; }
}
