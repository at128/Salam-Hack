using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Invoices.Commands.MarkInvoiceOverdue;

public sealed record MarkInvoiceOverdueCommand(Guid UserId, Guid InvoiceId) : IRequest<Result<InvoiceDto>>;
