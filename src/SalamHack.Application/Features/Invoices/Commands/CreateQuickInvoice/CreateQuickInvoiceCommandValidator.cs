using FluentValidation;

namespace SalamHack.Application.Features.Invoices.Commands.CreateQuickInvoice;

public sealed class CreateQuickInvoiceCommandValidator : AbstractValidator<CreateQuickInvoiceCommand>
{
    public CreateQuickInvoiceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("معرف العميل مطلوب.");

        RuleFor(x => x.ServiceName)
            .NotEmpty().WithMessage("اسم الخدمة مطلوب.")
            .MaximumLength(200);

        RuleFor(x => x.ProjectName)
            .MaximumLength(200);

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0);

        RuleFor(x => x.AdvanceAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.InvoiceNumber)
            .MaximumLength(50);

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("العملة مطلوبة.")
            .MaximumLength(10);

        RuleFor(x => x.Notes)
            .MaximumLength(2000);

        RuleFor(x => x.EstimatedHours)
            .GreaterThan(0)
            .When(x => x.EstimatedHours.HasValue);

        RuleFor(x => x.ToolCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Revision)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ServiceCategory)
            .IsInEnum();

        RuleFor(x => x)
            .Must(x => x.IssueDate is null || x.DueDate is null || x.IssueDate <= x.DueDate)
            .WithMessage("لا يمكن أن يكون تاريخ الاستحقاق قبل تاريخ الإصدار.");

        RuleFor(x => x)
            .Must(x => x.StartDate is null || x.EndDate is null || x.StartDate <= x.EndDate)
            .WithMessage("لا يمكن أن يكون تاريخ انتهاء المشروع قبل تاريخ البدء.");
    }
}
