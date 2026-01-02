using System.ComponentModel.DataAnnotations;

namespace Chet.WebApi.Template.DTOs
{
    /// <summary>
    /// 用户注册数据传输对象，用于接收用户注册请求
    /// </summary>
    public class RegisterDto
    {
        /// <summary>
        /// 用户名，用于标识用户
        /// </summary>
        [Required(ErrorMessage = "用户名不能为空")]
        [MaxLength(100, ErrorMessage = "用户名长度不能超过100个字符")]
        public required string Name { get; set; }

        /// <summary>
        /// 用户邮箱，用于登录和通知
        /// </summary>
        [Required(ErrorMessage = "邮箱不能为空")]
        [EmailAddress(ErrorMessage = "请输入有效的邮箱地址")]
        [MaxLength(255, ErrorMessage = "邮箱长度不能超过255个字符")]
        public required string Email { get; set; }

        /// <summary>
        /// 用户密码，用于登录认证
        /// </summary>
        [Required(ErrorMessage = "密码不能为空")]
        [MinLength(6, ErrorMessage = "密码长度不能少于6个字符")]
        public required string Password { get; set; }
    }
}
