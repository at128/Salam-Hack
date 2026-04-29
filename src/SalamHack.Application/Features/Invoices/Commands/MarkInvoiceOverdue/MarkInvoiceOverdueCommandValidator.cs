using FluentValidation;

namespace SalamHack.Application.Features.Invoices.Commands.MarkInvoiceOverdue;

public sealed class MarkInvoiceOverdueCommandValidator : AbstractValidator<MarkInvoiceOverdueCommand>
{
    public MarkInvoiceOverdueCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("Invoice ID is required.");
    }
}
