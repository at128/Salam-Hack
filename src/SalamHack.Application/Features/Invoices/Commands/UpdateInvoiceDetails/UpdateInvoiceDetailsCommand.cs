using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Invoices.Commands.UpdateInvoiceDetails;

public sealed record UpdateInvoiceDetailsCommand(
    Guid UserId,
    Guid InvoiceId,
    decimal TotalAmount,
    decimal AdvanceAmount,
    DateTimeOffset IssueDate,
    DateTimeOffset DueDate,
    string Currency,
    string? Notes) : IRequest<Result<InvoiceDto>>;
