using SalamHack.Domain.Common.Results;

namespace SalamHack.Domain.Payments;

public static class PaymentErrors
{
    public static readonly Error InvalidInvoiceId = Error.Validation(
        "Payment.InvalidInvoiceId",
        "Invoice id is required.");

    public static readonly Error AmountMustBePositive = Error.Validation(
        "Payment.AmountMustBePositive",
        "Payment amount must be greater than zero.");

    public static readonly Error CurrencyRequired = Error.Validation(
        "Payment.CurrencyRequired",
        "Currency is required.");
}
