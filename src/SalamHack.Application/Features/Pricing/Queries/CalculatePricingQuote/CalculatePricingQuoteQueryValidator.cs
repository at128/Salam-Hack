using FluentValidation;

namespace SalamHack.Application.Features.Pricing.Queries.CalculatePricingQuote;

public sealed class CalculatePricingQuoteQueryValidator : AbstractValidator<CalculatePricingQuoteQuery>
{
    public CalculatePricingQuoteQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("معرف الخدمة مطلوب.");

        RuleFor(x => x.EstimatedHours)
            .GreaterThan(0);

        RuleFor(x => x.Complexity)
            .IsInEnum();

        RuleFor(x => x.ToolCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.RecentProjectCount)
            .InclusiveBetween(0, 20);

        RuleFor(x => x.RequestedRevisions)
            .GreaterThanOrEqualTo(0)
            .When(x => x.RequestedRevisions.HasValue);
    }
}
