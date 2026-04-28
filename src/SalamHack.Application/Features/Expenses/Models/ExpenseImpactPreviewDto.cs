namespace SalamHack.Application.Features.Expenses.Models;

public sealed record ExpenseImpactPreviewDto(
    Guid ProjectId,
    string ProjectName,
    decimal CurrentProjectExpenses,
    decimal AddedExpenseAmount,
    decimal NewProjectExpenses,
    decimal CurrentProfit,
    decimal NewProfit,
    decimal CurrentMarginPercent,
    decimal NewMarginPercent,
    decimal ExpenseRatioPercent);
