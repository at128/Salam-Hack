using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Invoices.Commands.DeleteInvoice;

public sealed record DeleteInvoiceCommand(Guid UserId, Guid InvoiceId) : IRequest<Result<Deleted>>;
