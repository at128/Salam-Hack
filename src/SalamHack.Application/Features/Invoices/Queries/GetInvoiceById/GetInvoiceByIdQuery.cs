using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Invoices.Queries.GetInvoiceById;

public sealed record GetInvoiceByIdQuery(Guid UserId, Guid InvoiceId) : IRequest<Result<InvoiceDto>>;
