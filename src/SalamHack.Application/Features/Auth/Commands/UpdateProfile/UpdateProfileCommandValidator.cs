using FluentValidation;
using SalamHack.Domain.Common.Constants;

namespace SalamHack.Application.Features.Auth.Commands.UpdateProfile;

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("الاسم الأول مطلوب")
            .MaximumLength(ApplicationConstants.FieldLengths.FirstNameMaxLength);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("اسم العائلة مطلوب")
            .MaximumLength(ApplicationConstants.FieldLengths.LastNameMaxLength);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(ApplicationConstants.FieldLengths.PhoneNumberMaxLength)
            .When(x => x.PhoneNumber is not null);
    }
}
