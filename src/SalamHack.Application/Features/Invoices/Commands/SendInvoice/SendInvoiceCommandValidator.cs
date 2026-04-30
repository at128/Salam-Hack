using FluentValidation;

namespace SalamHack.Application.Features.Invoices.Commands.SendInvoice;

public sealed class SendInvoiceCommandValidator : AbstractValidator<SendInvoiceCommand>
{
    public SendInvoiceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("معرف الفاتورة مطلوب.");
    }
}
