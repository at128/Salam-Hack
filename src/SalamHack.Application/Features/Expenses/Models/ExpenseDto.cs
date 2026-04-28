using SalamHack.Domain.Expenses;

namespace SalamHack.Application.Features.Expenses.Models;

public sealed record ExpenseDto(
    Guid Id,
    Guid? ProjectId,
    string? ProjectName,
    ExpenseCategory Category,
    string Description,
    decimal Amount,
    bool IsRecurring,
    DateTimeOffset ExpenseDate,
    RecurrenceInterval? RecurrenceInterval,
    DateTimeOffset? RecurrenceEndDate,
    string Currency,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset LastModifiedUtc);
