using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Expenses.Models;
using SalamHack.Domain.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Expenses;

internal static class ExpenseImpactCalculator
{
    public static async Task<Result<ExpenseChangeImpactDto>> BuildImpactAsync(
        IAppDbContext context,
        Guid userId,
        Guid projectId,
        decimal previousProjectExpenses,
        decimal newProjectExpenses,
        decimal expenseDeltaAmount,
        CancellationToken ct)
    {
        var project = await context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId, ct);

        if (project is null)
            return ApplicationErrors.Projects.ProjectNotFound;

        var previousHealth = project.GetHealthSnapshot(previousProjectExpenses);
        if (previousHealth.IsError)
            return previousHealth.Errors;

        var newHealth = project.GetHealthSnapshot(newProjectExpenses);
        if (newHealth.IsError)
            return newHealth.Errors;

        var expenseRatio = project.SuggestedPrice > 0
            ? Math.Round(newProjectExpenses / project.SuggestedPrice * 100, 2)
            : 0;

        return new ExpenseChangeImpactDto(
            project.Id,
            project.ProjectName,
            previousProjectExpenses,
            newProjectExpenses,
            expenseDeltaAmount,
            previousHealth.Value.Profit,
            newHealth.Value.Profit,
            previousHealth.Value.MarginPercent,
            newHealth.Value.MarginPercent,
            expenseRatio);
    }

    public static async Task<decimal> SumProjectExpensesAsync(
        IAppDbContext context,
        Guid userId,
        Guid projectId,
        CancellationToken ct)
        => await context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.ProjectId == projectId)
            .SumAsync(e => (decimal?)e.Amount, ct) ?? 0;
}
