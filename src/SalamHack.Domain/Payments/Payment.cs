using SalamHack.Domain.Common;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Invoices;

namespace SalamHack.Domain.Payments;

public class Payment : AuditableEntity
{
    private Payment()
    {
    }

    private Payment(
        Guid id,
        Guid invoiceId,
        decimal amount,
        PaymentMethod method,
        DateTimeOffset paymentDate,
        string currency,
        string? notes = null)
        : base(id)
    {
        InvoiceId = invoiceId;
        Amount = amount;
        Method = method;
        PaymentDate = paymentDate;
        Notes = notes;
        Currency = currency;
    }

    public Guid InvoiceId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentMethod Method { get; private set; }
    public DateTimeOffset PaymentDate { get; private set; }
    public string? Notes { get; private set; }
    public string Currency { get; private set; } = null!;

    public Invoice Invoice { get; private set; } = null!;

    public static Result<Payment> Create(
        Guid invoiceId,
        decimal amount,
        PaymentMethod method,
        DateTimeOffset paymentDate,
        string currency,
        string? notes = null)
    {
        var validation = Validate(invoiceId, amount, currency);
        if (validation.IsError)
            return validation.Errors;

        return new Payment(
            Guid.CreateVersion7(),
            invoiceId,
            amount,
            method,
            paymentDate,
            currency.Trim(),
            NormalizeOptional(notes));
    }

    internal Result<Success> Update(
        decimal amount,
        PaymentMethod method,
        DateTimeOffset paymentDate,
        string? notes,
        string currency)
    {
        var validation = Validate(InvoiceId, amount, currency);
        if (validation.IsError)
            return validation;

        Amount = amount;
        Method = method;
        PaymentDate = paymentDate;
        Notes = NormalizeOptional(notes);
        Currency = currency.Trim();

        return Result.Success;
    }

    private static Result<Success> Validate(Guid invoiceId, decimal amount, string currency)
    {
        if (invoiceId == Guid.Empty)
            return PaymentErrors.InvalidInvoiceId;

        if (amount <= 0)
            return PaymentErrors.AmountMustBePositive;

        if (string.IsNullOrWhiteSpace(currency))
            return PaymentErrors.CurrencyRequired;

        return Result.Success;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
