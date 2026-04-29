namespace SalamHack.Application.Features.Customers.Models;

public sealed record CustomerProfileDto(
    CustomerDto Customer,
    int ProjectCount,
    DateTimeOffset? FirstDealAt,
    DateTimeOffset? LastActivityAt,
    decimal TotalInvoiced,
    decimal TotalPaid,
    decimal TotalOverdue,
    IReadOnlyCollection<CustomerProjectSummaryDto> Projects,
    IReadOnlyCollection<CustomerInvoiceSummaryDto> Invoices);
