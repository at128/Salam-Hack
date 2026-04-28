using FluentValidation;
using SalamHack.Domain.Common.Constants;

namespace SalamHack.Application.Features.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress()
            .MaximumLength(ApplicationConstants.FieldLengths.EmailMaxLength);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .MaximumLength(ApplicationConstants.FieldLengths.PhoneNumberMaxLength);

        RuleFor(x => x.ClientType)
            .IsInEnum();

        RuleFor(x => x.CompanyName)
            .MaximumLength(200);

        RuleFor(x => x.Notes)
            .MaximumLength(2000);
    }
}
