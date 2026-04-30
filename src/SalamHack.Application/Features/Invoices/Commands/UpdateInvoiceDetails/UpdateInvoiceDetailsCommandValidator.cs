using FluentValidation;

namespace SalamHack.Application.Features.Invoices.Commands.UpdateInvoiceDetails;

public sealed class UpdateInvoiceDetailsCommandValidator : AbstractValidator<UpdateInvoiceDetailsCommand>
{
    public UpdateInvoiceDetailsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("معرف الفاتورة مطلوب.");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0);

        RuleFor(x => x.AdvanceAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(x => x.IssueDate)
            .WithMessage("لا يمكن أن يكون تاريخ الاستحقاق قبل تاريخ الإصدار.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("العملة مطلوبة.")
            .MaximumLength(10);

        RuleFor(x => x.Notes)
            .MaximumLength(2000);
    }
}
