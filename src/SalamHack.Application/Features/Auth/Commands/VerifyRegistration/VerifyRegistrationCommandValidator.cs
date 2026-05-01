using FluentValidation;
using SalamHack.Domain.Common.Constants;

namespace SalamHack.Application.Features.Auth.Commands.VerifyRegistration;

public sealed class VerifyRegistrationCommandValidator : AbstractValidator<VerifyRegistrationCommand>
{
    public VerifyRegistrationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة")
            .MaximumLength(ApplicationConstants.FieldLengths.EmailMaxLength);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة")
            .MinimumLength(ApplicationConstants.FieldLengths.PasswordMinLength)
                .WithMessage($"يجب أن تكون كلمة المرور {ApplicationConstants.FieldLengths.PasswordMinLength} أحرف على الأقل")
            .Matches("[A-Z]").WithMessage("يجب أن تحتوي كلمة المرور على حرف كبير واحد على الأقل")
            .Matches("[a-z]").WithMessage("يجب أن تحتوي كلمة المرور على حرف صغير واحد على الأقل")
            .Matches("[0-9]").WithMessage("يجب أن تحتوي كلمة المرور على رقم واحد على الأقل")
            .Matches("[^a-zA-Z0-9]").WithMessage("يجب أن تحتوي كلمة المرور على رمز خاص واحد على الأقل");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("الاسم الأول مطلوب")
            .MaximumLength(ApplicationConstants.FieldLengths.FirstNameMaxLength);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("اسم العائلة مطلوب")
            .MaximumLength(ApplicationConstants.FieldLengths.LastNameMaxLength);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(ApplicationConstants.FieldLengths.PhoneNumberMaxLength)
            .When(x => x.PhoneNumber is not null);

        RuleFor(x => x.Otp)
            .NotEmpty().WithMessage("رمز التحقق مطلوب")
            .Length(6).WithMessage("رمز التحقق يجب أن يتكون من 6 أرقام")
            .Matches("^[0-9]{6}$").WithMessage("رمز التحقق يجب أن يتكون من أرقام فقط");
    }
}
