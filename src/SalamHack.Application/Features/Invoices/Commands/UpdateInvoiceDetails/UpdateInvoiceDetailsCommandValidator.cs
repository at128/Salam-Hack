using FluentValidation;

namespace SalamHack.Application.Features.Invoices.Commands.UpdateInvoiceDetails;

public sealed class UpdateInvoiceDetailsCommandValidator : AbstractValidator<UpdateInvoiceDetailsCommand>
{
    public UpdateInvoiceDetailsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("Invoice ID is required.");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0);

        RuleFor(x => x.AdvanceAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(x => x.IssueDate)
            .WithMessage("Due date cannot be earlier than issue date.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(10);

        RuleFor(x => x.Notes)
            .MaximumLength(2000);
    }
}
