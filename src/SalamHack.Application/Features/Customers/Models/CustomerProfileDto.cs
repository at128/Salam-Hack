namespace SalamHack.Application.Features.Customers.Models;

public sealed record CustomerProfileDto(
    CustomerDto Customer,
    int ProjectCount,
    decimal TotalInvoiced,
    decimal TotalPaid,
    decimal TotalOverdue,
    IReadOnlyCollection<CustomerProjectSummaryDto> Projects,
    IReadOnlyCollection<CustomerInvoiceSummaryDto> Invoices);
