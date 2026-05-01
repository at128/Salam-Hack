using FluentValidation;
using SalamHack.Domain.Common.Constants;

namespace SalamHack.Application.Features.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة")
            .MaximumLength(ApplicationConstants.FieldLengths.EmailMaxLength);

        RuleFor(x => x.Otp)
            .NotEmpty().WithMessage("رمز التحقق مطلوب")
            .Length(6).WithMessage("رمز التحقق يجب أن يتكون من 6 أرقام")
            .Matches("^[0-9]{6}$").WithMessage("رمز التحقق يجب أن يتكون من أرقام فقط");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("كلمة المرور الجديدة مطلوبة")
            .MinimumLength(ApplicationConstants.FieldLengths.PasswordMinLength)
                .WithMessage($"يجب أن تكون كلمة المرور {ApplicationConstants.FieldLengths.PasswordMinLength} أحرف على الأقل")
            .Matches("[A-Z]").WithMessage("يجب أن تحتوي كلمة المرور على حرف كبير واحد على الأقل")
            .Matches("[a-z]").WithMessage("يجب أن تحتوي كلمة المرور على حرف صغير واحد على الأقل")
            .Matches("[0-9]").WithMessage("يجب أن تحتوي كلمة المرور على رقم واحد على الأقل")
            .Matches("[^a-zA-Z0-9]").WithMessage("يجب أن تحتوي كلمة المرور على رمز خاص واحد على الأقل");
    }
}
