using FluentValidation;

namespace SalamHack.Application.Features.Pricing.Commands.CreateProjectFromPricingQuote;

public sealed class CreateProjectFromPricingQuoteCommandValidator : AbstractValidator<CreateProjectFromPricingQuoteCommand>
{
    public CreateProjectFromPricingQuoteCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("Service ID is required.");

        RuleFor(x => x.ProjectName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.EstimatedHours)
            .GreaterThan(0);

        RuleFor(x => x.Complexity)
            .IsInEnum();

        RuleFor(x => x.SelectedPlan)
            .IsInEnum();

        RuleFor(x => x.ToolCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Revision)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate);
    }
}
