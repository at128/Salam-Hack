using FluentValidation;

namespace SalamHack.Application.Features.Reports.Queries.GetProfitabilityReport;

public sealed class GetProfitabilityReportQueryValidator : AbstractValidator<GetProfitabilityReportQuery>
{
    public GetProfitabilityReportQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x)
            .Must(x => x.FromUtc is null || x.ToUtc is null || x.FromUtc <= x.ToUtc)
            .WithMessage("يجب أن يكون تاريخ البداية قبل أو مساوياً لتاريخ النهاية.");
    }
}
