using FluentValidation;

namespace SalamHack.Application.Features.Invoices.Queries.GetPaymentsSummary;

public sealed class GetPaymentsSummaryQueryValidator : AbstractValidator<GetPaymentsSummaryQuery>
{
    public GetPaymentsSummaryQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.OverdueInvoiceLimit)
            .InclusiveBetween(1, 50);
    }
}
