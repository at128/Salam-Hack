namespace SalamHack.Application.Features.Expenses.Models;

public sealed record ExpenseChangeImpactDto(
    Guid ProjectId,
    string ProjectName,
    decimal PreviousProjectExpenses,
    decimal NewProjectExpenses,
    decimal ExpenseDeltaAmount,
    decimal PreviousProfit,
    decimal NewProfit,
    decimal PreviousMarginPercent,
    decimal NewMarginPercent,
    decimal ExpenseRatioPercent);
