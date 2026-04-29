using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Invoices.Queries.GetPaymentsSummary;

public sealed record GetPaymentsSummaryQuery(
    Guid UserId,
    DateTimeOffset? AsOfUtc = null,
    int OverdueInvoiceLimit = 10) : IRequest<Result<PaymentsSummaryDto>>;
