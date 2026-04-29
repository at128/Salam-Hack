using FluentValidation;

namespace SalamHack.Application.Features.Services.Commands.UpdateService;

public sealed class UpdateServiceCommandValidator : AbstractValidator<UpdateServiceCommand>
{
    public UpdateServiceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("Service ID is required.");

        RuleFor(x => x.ServiceName)
            .NotEmpty().WithMessage("Service name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Category)
            .IsInEnum();

        RuleFor(x => x.DefaultHourlyRate)
            .GreaterThan(0);

        RuleFor(x => x.DefaultRevisions)
            .GreaterThanOrEqualTo(0);
    }
}
