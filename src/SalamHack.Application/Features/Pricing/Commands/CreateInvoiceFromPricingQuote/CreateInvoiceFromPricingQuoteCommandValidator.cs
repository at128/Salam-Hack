using FluentValidation;

namespace SalamHack.Application.Features.Pricing.Commands.CreateInvoiceFromPricingQuote;

public sealed class CreateInvoiceFromPricingQuoteCommandValidator : AbstractValidator<CreateInvoiceFromPricingQuoteCommand>
{
    public CreateInvoiceFromPricingQuoteCommandValidator()
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

        RuleFor(x => x.InvoiceNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(x => x.IssueDate);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .MaximumLength(10);
    }
}
