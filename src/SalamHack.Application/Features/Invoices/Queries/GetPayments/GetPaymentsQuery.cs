using SalamHack.Application.Common.Models;
using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Invoices.Queries.GetPayments;

public sealed record GetPaymentsQuery(
    Guid UserId,
    Guid? InvoiceId,
    DateTimeOffset? FromDate,
    DateTimeOffset? ToDate,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PaginatedList<PaymentDto>>>;
