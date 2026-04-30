using FluentValidation;

namespace SalamHack.Application.Features.Invoices.Queries.ExportInvoicePdf;

public sealed class ExportInvoicePdfQueryValidator : AbstractValidator<ExportInvoicePdfQuery>
{
    public ExportInvoicePdfQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("معرف الفاتورة مطلوب.");
    }
}
