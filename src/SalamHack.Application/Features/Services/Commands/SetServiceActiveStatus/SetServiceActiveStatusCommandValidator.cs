using FluentValidation;

namespace SalamHack.Application.Features.Services.Commands.SetServiceActiveStatus;

public sealed class SetServiceActiveStatusCommandValidator : AbstractValidator<SetServiceActiveStatusCommand>
{
    public SetServiceActiveStatusCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("Service ID is required.");
    }
}
