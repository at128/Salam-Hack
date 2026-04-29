using FluentValidation;

namespace SalamHack.Application.Features.Reports.Commands.ExportProfitabilityReport;

public sealed class ExportProfitabilityReportCommandValidator : AbstractValidator<ExportProfitabilityReportCommand>
{
    public ExportProfitabilityReportCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Format)
            .IsInEnum();

        RuleFor(x => x)
            .Must(x => x.FromUtc is null || x.ToUtc is null || x.FromUtc <= x.ToUtc)
            .WithMessage("From date must be before or equal to To date.");
    }
}
