using SalamHack.Application.Features.Expenses;
using FluentValidation;

namespace SalamHack.Application.Features.Expenses.Commands.UploadExpenseReceipt;

public sealed class UploadExpenseReceiptCommandValidator : AbstractValidator<UploadExpenseReceiptCommand>
{
    public UploadExpenseReceiptCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.ExpenseId)
            .NotEmpty().WithMessage("معرف المصروف مطلوب.");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("اسم ملف الإيصال مطلوب.")
            .MaximumLength(255);

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("نوع محتوى الإيصال مطلوب.")
            .Must(ExpenseReceiptRules.IsAllowedContentType)
            .WithMessage("يجب أن يكون الإيصال ملف PDF أو JPEG أو PNG أو WebP.");

        RuleFor(x => x.Content)
            .NotNull().WithMessage("محتوى الإيصال مطلوب.");

        RuleFor(x => x.Length)
            .GreaterThan(0).WithMessage("محتوى الإيصال مطلوب.")
            .LessThanOrEqualTo(ExpenseReceiptRules.MaxFileSizeBytes)
            .WithMessage("يجب ألا يتجاوز حجم ملف الإيصال 10 ميغابايت.");
    }
}
