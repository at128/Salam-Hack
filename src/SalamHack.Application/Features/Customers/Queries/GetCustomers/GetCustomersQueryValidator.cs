using FluentValidation;

namespace SalamHack.Application.Features.Customers.Queries.GetCustomers;

public sealed class GetCustomersQueryValidator : AbstractValidator<GetCustomersQuery>
{
    public GetCustomersQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.Search)
            .MaximumLength(200);

        RuleFor(x => x.ClientType)
            .IsInEnum()
            .When(x => x.ClientType.HasValue);

        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
