using FluentValidation;

namespace SalamHack.Application.Features.Notifications.Commands.CreateInvoiceReminder;

public sealed class CreateInvoiceReminderCommandValidator : AbstractValidator<CreateInvoiceReminderCommand>
{
    public CreateInvoiceReminderCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("معرف الفاتورة مطلوب.");

        RuleFor(x => x.Message)
            .NotEmpty()
            .MaximumLength(1000);
    }
}
