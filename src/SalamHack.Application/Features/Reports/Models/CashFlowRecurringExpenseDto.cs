using SalamHack.Domain.Expenses;

namespace SalamHack.Application.Features.Reports.Models;

public sealed record CashFlowRecurringExpenseDto(
    Guid ExpenseId,
    string Description,
    decimal Amount,
    RecurrenceInterval RecurrenceInterval,
    decimal MonthlyEquivalentAmount,
    DateTimeOffset ExpenseDate,
    DateTimeOffset? RecurrenceEndDate,
    string Currency);
