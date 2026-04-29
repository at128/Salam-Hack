namespace SalamHack.Application.Features.Dashboard.Models;

public enum DashboardTransactionType
{
    Payment,
    Expense
}

public sealed record DashboardTransactionDto(
    DateTimeOffset Date,
    string Description,
    decimal Amount,
    DashboardTransactionType Type,
    Guid? InvoiceId,
    Guid? ExpenseId,
    Guid? CustomerId,
    string Currency);
