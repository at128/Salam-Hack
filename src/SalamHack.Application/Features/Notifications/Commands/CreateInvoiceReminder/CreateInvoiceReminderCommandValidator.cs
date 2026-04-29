using FluentValidation;

namespace SalamHack.Application.Features.Notifications.Commands.CreateInvoiceReminder;

public sealed class CreateInvoiceReminderCommandValidator : AbstractValidator<CreateInvoiceReminderCommand>
{
    public CreateInvoiceReminderCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("Invoice ID is required.");

        RuleFor(x => x.Message)
            .NotEmpty()
            .MaximumLength(1000);
    }
}
