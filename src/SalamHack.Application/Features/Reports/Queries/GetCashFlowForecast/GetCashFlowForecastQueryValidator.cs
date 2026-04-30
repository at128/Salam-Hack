using FluentValidation;

namespace SalamHack.Application.Features.Reports.Queries.GetCashFlowForecast;

public sealed class GetCashFlowForecastQueryValidator : AbstractValidator<GetCashFlowForecastQuery>
{
    public GetCashFlowForecastQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");
    }
}
