using FluentValidation;
using SalamHack.Domain.Common.Constants;

namespace SalamHack.Application.Features.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("اسم العميل مطلوب.")
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
            .EmailAddress()
            .MaximumLength(ApplicationConstants.FieldLengths.EmailMaxLength);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("رقم الهاتف مطلوب.")
            .MaximumLength(ApplicationConstants.FieldLengths.PhoneNumberMaxLength);

        RuleFor(x => x.ClientType)
            .IsInEnum();

        RuleFor(x => x.CompanyName)
            .MaximumLength(200);

        RuleFor(x => x.Notes)
            .MaximumLength(2000);
    }
}
