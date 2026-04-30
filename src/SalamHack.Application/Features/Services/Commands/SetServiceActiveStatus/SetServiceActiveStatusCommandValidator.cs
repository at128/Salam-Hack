using FluentValidation;

namespace SalamHack.Application.Features.Services.Commands.SetServiceActiveStatus;

public sealed class SetServiceActiveStatusCommandValidator : AbstractValidator<SetServiceActiveStatusCommand>
{
    public SetServiceActiveStatusCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("معرف الخدمة مطلوب.");
    }
}
