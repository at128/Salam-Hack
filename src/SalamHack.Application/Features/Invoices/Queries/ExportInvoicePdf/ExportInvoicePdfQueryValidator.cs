using FluentValidation;

namespace SalamHack.Application.Features.Invoices.Queries.ExportInvoicePdf;

public sealed class ExportInvoicePdfQueryValidator : AbstractValidator<ExportInvoicePdfQuery>
{
    public ExportInvoicePdfQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("Invoice ID is required.");
    }
}
