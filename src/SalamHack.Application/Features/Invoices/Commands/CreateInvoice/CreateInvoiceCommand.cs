using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Invoices.Commands.CreateInvoice;

public sealed record CreateInvoiceCommand(
    Guid UserId,
    Guid ProjectId,
    string InvoiceNumber,
    decimal TotalAmount,
    decimal AdvanceAmount,
    DateTimeOffset IssueDate,
    DateTimeOffset DueDate,
    string Currency,
    string? Notes) : IRequest<Result<InvoiceDto>>;
