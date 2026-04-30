using SalamHack.Domain.Common.Results;

namespace SalamHack.Domain.Payments;

public static class PaymentErrors
{
    public static readonly Error InvalidInvoiceId = Error.Validation(
        "Payment.InvalidInvoiceId",
        "معرف الفاتورة مطلوب.");

    public static readonly Error AmountMustBePositive = Error.Validation(
        "Payment.AmountMustBePositive",
        "يجب أن يكون مبلغ الدفع أكبر من صفر.");

    public static readonly Error CurrencyRequired = Error.Validation(
        "Payment.CurrencyRequired",
        "العملة مطلوبة.");
}
