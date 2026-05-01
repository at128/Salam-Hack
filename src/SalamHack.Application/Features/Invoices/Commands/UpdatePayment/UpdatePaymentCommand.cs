using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Payments;
using MediatR;

namespace SalamHack.Application.Features.Invoices.Commands.UpdatePayment;

public sealed record UpdatePaymentCommand(
    Guid UserId,
    Guid PaymentId,
    decimal Amount,
    PaymentMethod Method,
    DateTimeOffset PaymentDate,
    string Currency,
    string? Notes) : IRequest<Result<InvoiceDto>>;
