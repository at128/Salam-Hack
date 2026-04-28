using FluentValidation;

namespace SalamHack.Application.Features.Pricing.Queries.CalculatePricingQuote;

public sealed class CalculatePricingQuoteQueryValidator : AbstractValidator<CalculatePricingQuoteQuery>
{
    public CalculatePricingQuoteQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("Service ID is required.");

        RuleFor(x => x.EstimatedHours)
            .GreaterThan(0);

        RuleFor(x => x.Complexity)
            .IsInEnum();

        RuleFor(x => x.RecentProjectCount)
            .InclusiveBetween(0, 20);
    }
}
