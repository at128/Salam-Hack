using SalamHack.Domain.Payments;

namespace SalamHack.Application.Features.Invoices.Models;

public sealed record PaymentDto(
    Guid Id,
    Guid InvoiceId,
    decimal Amount,
    PaymentMethod Method,
    DateTimeOffset PaymentDate,
    string? Notes,
    string Currency,
    DateTimeOffset CreatedAtUtc);
