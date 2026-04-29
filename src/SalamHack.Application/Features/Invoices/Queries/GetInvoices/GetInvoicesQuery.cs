using SalamHack.Application.Common.Models;
using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Invoices;
using MediatR;

namespace SalamHack.Application.Features.Invoices.Queries.GetInvoices;

public sealed record GetInvoicesQuery(
    Guid UserId,
    string? Search,
    Guid? CustomerId,
    Guid? ProjectId,
    InvoiceStatus? Status,
    DateTimeOffset? FromDate,
    DateTimeOffset? ToDate,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PaginatedList<InvoiceListItemDto>>>;
