using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Invoices.Commands.CancelInvoice;

public sealed record CancelInvoiceCommand(Guid UserId, Guid InvoiceId) : IRequest<Result<InvoiceDto>>;
