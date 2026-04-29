using FluentValidation;

namespace SalamHack.Application.Features.Customers.Queries.GetCustomerProfile;

public sealed class GetCustomerProfileQueryValidator : AbstractValidator<GetCustomerProfileQuery>
{
    public GetCustomerProfileQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");
    }
}
