using FluentValidation;

namespace SalamHack.Application.Features.Invoices.Commands.RecordAdvancePayment;

public sealed class RecordAdvancePaymentCommandValidator : AbstractValidator<RecordAdvancePaymentCommand>
{
    public RecordAdvancePaymentCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("Invoice ID is required.");

        RuleFor(x => x.Method)
            .IsInEnum();

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(10);

        RuleFor(x => x.Notes)
            .MaximumLength(2000);
    }
}
