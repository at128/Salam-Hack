using SalamHack.Domain.Expenses;

namespace SalamHack.Application.Features.Expenses.Models;

public sealed record ExpenseListItemDto(
    Guid Id,
    Guid? ProjectId,
    string? ProjectName,
    ExpenseCategory Category,
    string Description,
    decimal Amount,
    bool IsRecurring,
    DateTimeOffset ExpenseDate,
    string Currency);
