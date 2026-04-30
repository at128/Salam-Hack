using FluentValidation;

namespace SalamHack.Application.Features.Pricing.Commands.CreateProjectFromPricingQuote;

public sealed class CreateProjectFromPricingQuoteCommandValidator : AbstractValidator<CreateProjectFromPricingQuoteCommand>
{
    public CreateProjectFromPricingQuoteCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("معرف العميل مطلوب.");

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("معرف الخدمة مطلوب.");

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
