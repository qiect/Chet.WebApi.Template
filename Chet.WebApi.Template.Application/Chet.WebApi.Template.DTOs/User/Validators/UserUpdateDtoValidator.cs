using FluentValidation;

namespace Chet.WebApi.Template.DTOs.User.Validators;

public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
{
    public UserUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("用户名不能为空")
            .MaximumLength(100).WithMessage("用户名长度不能超过100个字符");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("邮箱不能为空")
            .EmailAddress().WithMessage("请输入有效的邮箱地址")
            .MaximumLength(255).WithMessage("邮箱长度不能超过255个字符");
    }
}
