using FluentValidation;

namespace SalamHack.Application.Features.Services.Queries.GetServices;

public sealed class GetServicesQueryValidator : AbstractValidator<GetServicesQuery>
{
    public GetServicesQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Search)
            .MaximumLength(200);

        RuleFor(x => x.Category)
            .IsInEnum()
            .When(x => x.Category.HasValue);

        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
