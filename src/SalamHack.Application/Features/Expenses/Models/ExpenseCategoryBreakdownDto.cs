using SalamHack.Domain.Expenses;

namespace SalamHack.Application.Features.Expenses.Models;

public sealed record ExpenseCategoryBreakdownDto(
    DateTimeOffset FromUtc,
    DateTimeOffset ToUtc,
    decimal TotalAmount,
    int ExpenseCount,
    IReadOnlyCollection<ExpenseCategoryBreakdownItemDto> Categories);

public sealed record ExpenseCategoryBreakdownItemDto(
    ExpenseCategory Category,
    decimal TotalAmount,
    int ExpenseCount,
    decimal SharePercent);
