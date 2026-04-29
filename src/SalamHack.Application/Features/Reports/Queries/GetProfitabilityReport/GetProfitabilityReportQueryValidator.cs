using FluentValidation;

namespace SalamHack.Application.Features.Reports.Queries.GetProfitabilityReport;

public sealed class GetProfitabilityReportQueryValidator : AbstractValidator<GetProfitabilityReportQuery>
{
    public GetProfitabilityReportQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x)
            .Must(x => x.FromUtc is null || x.ToUtc is null || x.FromUtc <= x.ToUtc)
            .WithMessage("From date must be before or equal to To date.");
    }
}
