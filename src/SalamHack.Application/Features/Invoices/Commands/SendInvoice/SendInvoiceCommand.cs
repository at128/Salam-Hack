using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Invoices.Commands.SendInvoice;

public sealed record SendInvoiceCommand(Guid UserId, Guid InvoiceId) : IRequest<Result<InvoiceDto>>;
