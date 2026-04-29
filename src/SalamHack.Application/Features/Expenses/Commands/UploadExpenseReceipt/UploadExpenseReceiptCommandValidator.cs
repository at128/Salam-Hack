using SalamHack.Application.Features.Expenses;
using FluentValidation;

namespace SalamHack.Application.Features.Expenses.Commands.UploadExpenseReceipt;

public sealed class UploadExpenseReceiptCommandValidator : AbstractValidator<UploadExpenseReceiptCommand>
{
    public UploadExpenseReceiptCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ExpenseId)
            .NotEmpty().WithMessage("Expense ID is required.");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("Receipt file name is required.")
            .MaximumLength(255);

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Receipt content type is required.")
            .Must(ExpenseReceiptRules.IsAllowedContentType)
            .WithMessage("Receipt must be a PDF, JPEG, PNG, or WebP file.");

        RuleFor(x => x.Content)
            .NotNull().WithMessage("Receipt content is required.");

        RuleFor(x => x.Length)
            .GreaterThan(0).WithMessage("Receipt content is required.")
            .LessThanOrEqualTo(ExpenseReceiptRules.MaxFileSizeBytes)
            .WithMessage("Receipt file size must be 10 MB or less.");
    }
}
