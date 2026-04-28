using FluentValidation;

namespace SalamHack.Application.Features.Services.Commands.CreateService;

public sealed class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

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
