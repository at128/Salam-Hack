using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Invoices;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Invoices.Queries.GetPaymentsSummary;

public sealed class GetPaymentsSummaryQueryHandler(
    IAppDbContext context,
    TimeProvider timeProvider)
    : IRequestHandler<GetPaymentsSummaryQuery, Result<PaymentsSummaryDto>>
{
    public async Task<Result<PaymentsSummaryDto>> Handle(GetPaymentsSummaryQuery query, CancellationToken ct)
    {
        var asOfUtc = query.AsOfUtc ?? timeProvider.GetUtcNow();
        var overdueInvoiceLimit = Math.Clamp(query.OverdueInvoiceLimit, 1, 50);

        var billableInvoices = context.Invoices
            .AsNoTracking()
            .Where(i => i.UserId == query.UserId &&
                        i.Status != InvoiceStatus.Draft &&
                        i.Status != InvoiceStatus.Cancelled);

        var totals = await billableInvoices
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Invoiced = g.Sum(i => (decimal?)i.TotalWithTax) ?? 0,
                Collected = g.Sum(i => (decimal?)i.PaidAmount) ?? 0,
                Paid = g.Count(i => i.Status == InvoiceStatus.Paid ||
                                    i.TotalWithTax <= i.PaidAmount),
                Pending = g.Count(i => i.Status != InvoiceStatus.Paid &&
                                       i.Status != InvoiceStatus.Overdue &&
                                       i.DueDate >= asOfUtc &&
                                       i.TotalWithTax > i.PaidAmount),
                Overdue = g.Count(i => (i.Status == InvoiceStatus.Overdue || i.DueDate < asOfUtc) &&
                                       i.Status != InvoiceStatus.Paid &&
                                       i.TotalWithTax > i.PaidAmount),
                OverdueAmount = g
                    .Where(i => (i.Status == InvoiceStatus.Overdue || i.DueDate < asOfUtc) &&
                                i.Status != InvoiceStatus.Paid &&
                                i.TotalWithTax > i.PaidAmount)
                    .Sum(i => (decimal?)(i.TotalWithTax - i.PaidAmount)) ?? 0
            })
            .FirstOrDefaultAsync(ct);

        var totalInvoiced = totals?.Invoiced ?? 0;
        var totalCollected = totals?.Collected ?? 0;
        var totalOutstanding = totalInvoiced - totalCollected;
        var totalOverdue = totals?.OverdueAmount ?? 0;
        var collectionRate = totalInvoiced > 0
            ? Math.Round(totalCollected / totalInvoiced * 100, 2)
            : 0;

        var overdueInvoices = await billableInvoices
            .Where(i => (i.Status == InvoiceStatus.Overdue || i.DueDate < asOfUtc) &&
                        i.Status != InvoiceStatus.Paid &&
                        i.TotalWithTax > i.PaidAmount)
            .OrderBy(i => i.DueDate)
            .Take(overdueInvoiceLimit)
            .Select(i => new
            {
                i.Id,
                i.InvoiceNumber,
                i.CustomerId,
                CustomerName = i.Project.Customer.CustomerName,
                Remaining = i.TotalWithTax - i.PaidAmount,
                i.DueDate,
                i.Currency
            })
            .ToListAsync(ct);

        var overdueDtos = overdueInvoices
            .Select(i => new PaymentsSummaryOverdueInvoiceDto(
                i.Id,
                i.InvoiceNumber,
                i.CustomerId,
                i.CustomerName,
                i.Remaining,
                i.DueDate,
                Math.Max(0, (int)(asOfUtc - i.DueDate).TotalDays),
                i.Currency))
            .ToList();

        return new PaymentsSummaryDto(
            totalInvoiced,
            totalCollected,
            totalOutstanding,
            totalOverdue,
            collectionRate,
            totals?.Paid ?? 0,
            totals?.Pending ?? 0,
            totals?.Overdue ?? 0,
            overdueDtos);
    }
}
