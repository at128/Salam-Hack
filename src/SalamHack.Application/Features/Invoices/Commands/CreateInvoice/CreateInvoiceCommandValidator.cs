using FluentValidation;

namespace SalamHack.Application.Features.Invoices.Commands.CreateInvoice;

public sealed class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("معرف المشروع مطلوب.");

        RuleFor(x => x.InvoiceNumber)
            .NotEmpty().WithMessage("رقم الفاتورة مطلوب.")
            .MaximumLength(50);

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
