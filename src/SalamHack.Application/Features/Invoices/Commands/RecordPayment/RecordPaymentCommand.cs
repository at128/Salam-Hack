using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Payments;
using MediatR;

namespace SalamHack.Application.Features.Invoices.Commands.RecordPayment;

public sealed record RecordPaymentCommand(
    Guid UserId,
    Guid InvoiceId,
    decimal Amount,
    PaymentMethod Method,
    DateTimeOffset PaymentDate,
    string Currency,
    string? Notes) : IRequest<Result<InvoiceDto>>;
