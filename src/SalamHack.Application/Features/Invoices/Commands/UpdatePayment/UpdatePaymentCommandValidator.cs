using FluentValidation;

namespace SalamHack.Application.Features.Invoices.Commands.UpdatePayment;

public sealed class UpdatePaymentCommandValidator : AbstractValidator<UpdatePaymentCommand>
{
    public UpdatePaymentCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.PaymentId)
            .NotEmpty().WithMessage("معرف الدفعة مطلوب.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("يجب أن يكون مبلغ الدفعة أكبر من صفر.");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("تاريخ الدفعة مطلوب.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("العملة مطلوبة.")
            .MaximumLength(3).WithMessage("رمز العملة يجب ألا يتجاوز 3 أحرف.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("الملاحظات يجب ألا تتجاوز 500 حرف.")
            .When(x => x.Notes is not null);
    }
}
