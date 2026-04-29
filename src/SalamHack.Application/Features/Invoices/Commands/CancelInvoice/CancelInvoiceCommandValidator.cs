using FluentValidation;

namespace SalamHack.Application.Features.Invoices.Commands.CancelInvoice;

public sealed class CancelInvoiceCommandValidator : AbstractValidator<CancelInvoiceCommand>
{
    public CancelInvoiceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("Invoice ID is required.");
    }
}
