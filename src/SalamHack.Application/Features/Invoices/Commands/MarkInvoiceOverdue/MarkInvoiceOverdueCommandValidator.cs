using FluentValidation;

namespace SalamHack.Application.Features.Invoices.Commands.MarkInvoiceOverdue;

public sealed class MarkInvoiceOverdueCommandValidator : AbstractValidator<MarkInvoiceOverdueCommand>
{
    public MarkInvoiceOverdueCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("معرف الفاتورة مطلوب.");
    }
}
