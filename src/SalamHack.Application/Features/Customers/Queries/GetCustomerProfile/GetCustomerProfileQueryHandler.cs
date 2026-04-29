using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Customers.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Invoices;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Customers.Queries.GetCustomerProfile;

public sealed class GetCustomerProfileQueryHandler(
    IAppDbContext context,
    TimeProvider timeProvider)
    : IRequestHandler<GetCustomerProfileQuery, Result<CustomerProfileDto>>
{
    public async Task<Result<CustomerProfileDto>> Handle(GetCustomerProfileQuery query, CancellationToken ct)
    {
        var customer = await context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.CustomerId && c.UserId == query.UserId, ct);

        if (customer is null)
            return ApplicationErrors.Customers.CustomerNotFound;

        var asOfUtc = timeProvider.GetUtcNow();

        var projects = await context.Projects
            .AsNoTracking()
            .Where(p => p.UserId == query.UserId && p.CustomerId == query.CustomerId)
            .OrderByDescending(p => p.StartDate)
            .Select(p => new CustomerProjectSummaryDto(
                p.Id,
                p.ProjectName,
                p.ServiceId,
                p.Service.ServiceName,
                p.Status,
                p.SuggestedPrice,
                p.ProfitMargin,
                p.StartDate,
                p.EndDate))
            .ToListAsync(ct);

        var invoices = await context.Invoices
            .AsNoTracking()
            .Where(i => i.CustomerId == query.CustomerId && i.UserId == query.UserId)
            .OrderByDescending(i => i.IssueDate)
            .Select(i => new CustomerInvoiceSummaryDto(
                i.Id,
                i.InvoiceNumber,
                i.ProjectId,
                i.Project.ProjectName,
                i.TotalWithTax,
                i.PaidAmount,
                i.TotalWithTax - i.PaidAmount,
                i.Status != InvoiceStatus.Draft &&
                i.Status != InvoiceStatus.Cancelled &&
                i.TotalWithTax <= i.PaidAmount
                    ? InvoiceStatus.Paid
                    : i.Status != InvoiceStatus.Draft &&
                      i.Status != InvoiceStatus.Cancelled &&
                      i.Status != InvoiceStatus.Paid &&
                      i.TotalWithTax > i.PaidAmount &&
                      i.DueDate < asOfUtc
                    ? InvoiceStatus.Overdue
                    : i.Status,
                i.IssueDate,
                i.DueDate,
                i.Currency))
            .ToListAsync(ct);

        var billableInvoices = invoices
            .Where(i => i.Status != InvoiceStatus.Draft && i.Status != InvoiceStatus.Cancelled)
            .ToList();

        var totalOverdue = billableInvoices
            .Where(i => i.Status != InvoiceStatus.Paid &&
                        i.RemainingAmount > 0 &&
                        (i.Status == InvoiceStatus.Overdue || i.DueDate < asOfUtc))
            .Sum(i => i.RemainingAmount);

        var activityDates = projects.Select(p => p.StartDate)
            .Concat(projects.Select(p => p.EndDate))
            .Concat(invoices.Select(i => i.IssueDate))
            .ToList();

        return new CustomerProfileDto(
            customer.ToDto(),
            projects.Count,
            activityDates.Count > 0 ? activityDates.Min() : null,
            activityDates.Count > 0 ? activityDates.Max() : null,
            billableInvoices.Sum(i => i.TotalWithTax),
            billableInvoices.Sum(i => i.PaidAmount),
            totalOverdue,
            projects,
            invoices);
    }
}
