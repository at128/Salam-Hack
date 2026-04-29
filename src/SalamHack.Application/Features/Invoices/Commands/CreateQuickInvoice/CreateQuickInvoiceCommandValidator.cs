using FluentValidation;

namespace SalamHack.Application.Features.Invoices.Commands.CreateQuickInvoice;

public sealed class CreateQuickInvoiceCommandValidator : AbstractValidator<CreateQuickInvoiceCommand>
{
    public CreateQuickInvoiceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.ServiceName)
            .NotEmpty().WithMessage("Service name is required.")
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
            .NotEmpty().WithMessage("Currency is required.")
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
            .WithMessage("Due date cannot be earlier than issue date.");

        RuleFor(x => x)
            .Must(x => x.StartDate is null || x.EndDate is null || x.StartDate <= x.EndDate)
            .WithMessage("Project end date cannot be earlier than start date.");
    }
}
