using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Payments;
using MediatR;

namespace SalamHack.Application.Features.Invoices.Commands.RecordAdvancePayment;

public sealed record RecordAdvancePaymentCommand(
    Guid UserId,
    Guid InvoiceId,
    PaymentMethod Method,
    DateTimeOffset PaymentDate,
    string Currency,
    string? Notes) : IRequest<Result<InvoiceDto>>;
