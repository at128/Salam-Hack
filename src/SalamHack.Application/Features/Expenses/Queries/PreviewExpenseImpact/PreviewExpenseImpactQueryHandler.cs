using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Expenses.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Expenses.Queries.PreviewExpenseImpact;

public sealed class PreviewExpenseImpactQueryHandler(IAppDbContext context)
    : IRequestHandler<PreviewExpenseImpactQuery, Result<ExpenseImpactPreviewDto>>
{
    public async Task<Result<ExpenseImpactPreviewDto>> Handle(PreviewExpenseImpactQuery query, CancellationToken ct)
    {
        var project = await context.Projects
            .AsNoTracking()
            .Include(p => p.Expenses)
            .FirstOrDefaultAsync(p => p.Id == query.ProjectId && p.UserId == query.UserId, ct);

        if (project is null)
            return ApplicationErrors.Projects.ProjectNotFound;

        var currentExpenses = project.Expenses.Sum(e => e.Amount);
        var currentHealth = project.GetHealthSnapshot(currentExpenses);
        var newHealth = project.GetHealthSnapshot(currentExpenses + query.Amount);

        if (currentHealth.IsError)
            return currentHealth.Errors;

        if (newHealth.IsError)
            return newHealth.Errors;

        var newProjectExpenses = currentExpenses + query.Amount;
        var expenseRatio = project.SuggestedPrice > 0
            ? Math.Round(newProjectExpenses / project.SuggestedPrice * 100, 2)
            : 0;

        return new ExpenseImpactPreviewDto(
            project.Id,
            project.ProjectName,
            currentExpenses,
            query.Amount,
            newProjectExpenses,
            currentHealth.Value.Profit,
            newHealth.Value.Profit,
            currentHealth.Value.MarginPercent,
            newHealth.Value.MarginPercent,
            expenseRatio);
    }
}
