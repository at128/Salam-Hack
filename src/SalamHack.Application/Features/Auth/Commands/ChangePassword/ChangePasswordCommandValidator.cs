using FluentValidation;
using SalamHack.Domain.Common.Constants;

namespace SalamHack.Application.Features.Auth.Commands.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("كلمة المرور الحالية مطلوبة");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("كلمة المرور الجديدة مطلوبة")
            .MinimumLength(ApplicationConstants.FieldLengths.PasswordMinLength)
                .WithMessage($"يجب أن تكون كلمة المرور {ApplicationConstants.FieldLengths.PasswordMinLength} أحرف على الأقل")
            .Matches("[A-Z]").WithMessage("يجب أن تحتوي كلمة المرور على حرف كبير واحد على الأقل")
            .Matches("[a-z]").WithMessage("يجب أن تحتوي كلمة المرور على حرف صغير واحد على الأقل")
            .Matches("[0-9]").WithMessage("يجب أن تحتوي كلمة المرور على رقم واحد على الأقل")
            .Matches("[^a-zA-Z0-9]").WithMessage("يجب أن تحتوي كلمة المرور على رمز خاص واحد على الأقل")
            .NotEqual(x => x.CurrentPassword)
                .WithMessage("يجب أن تكون كلمة المرور الجديدة مختلفة عن الحالية");
    }
}
