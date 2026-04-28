using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Expenses.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Expenses.Commands.UpdateExpenseWithImpact;

public sealed class UpdateExpenseWithImpactCommandHandler(IAppDbContext context)
    : IRequestHandler<UpdateExpenseWithImpactCommand, Result<ExpenseMutationResultDto>>
{
    public async Task<Result<ExpenseMutationResultDto>> Handle(UpdateExpenseWithImpactCommand cmd, CancellationToken ct)
    {
        var expense = await context.Expenses
            .FirstOrDefaultAsync(e => e.Id == cmd.ExpenseId && e.UserId == cmd.UserId, ct);

        if (expense is null)
            return ApplicationErrors.Expenses.ExpenseNotFound;

        var oldProjectId = expense.ProjectId;
        var oldAmount = expense.Amount;
        string? projectName = null;
        var impacts = await BuildImpactsAsync(cmd, oldProjectId, oldAmount, ct);

        if (impacts.IsError)
            return impacts.Errors;

        if (cmd.ProjectId.HasValue)
            projectName = impacts.Value.FirstOrDefault(i => i.ProjectId == cmd.ProjectId.Value)?.ProjectName;

        var updateResult = expense.Update(
            cmd.ProjectId,
            cmd.Category,
            cmd.Description,
            cmd.Amount,
            cmd.IsRecurring,
            cmd.ExpenseDate,
            cmd.RecurrenceInterval,
            cmd.RecurrenceEndDate,
            cmd.Currency);

        if (updateResult.IsError)
            return updateResult.Errors;

        await context.SaveChangesAsync(ct);

        return new ExpenseMutationResultDto(expense.ToDto(projectName), impacts.Value);
    }

    private async Task<Result<IReadOnlyCollection<ExpenseChangeImpactDto>>> BuildImpactsAsync(
        UpdateExpenseWithImpactCommand cmd,
        Guid? oldProjectId,
        decimal oldAmount,
        CancellationToken ct)
    {
        var impacts = new List<ExpenseChangeImpactDto>();

        if (oldProjectId.HasValue && oldProjectId == cmd.ProjectId)
        {
            var previousExpenses = await ExpenseImpactCalculator.SumProjectExpensesAsync(
                context,
                cmd.UserId,
                oldProjectId.Value,
                ct);
            var newExpenses = previousExpenses - oldAmount + cmd.Amount;

            var impact = await ExpenseImpactCalculator.BuildImpactAsync(
                context,
                cmd.UserId,
                oldProjectId.Value,
                previousExpenses,
                newExpenses,
                cmd.Amount - oldAmount,
                ct);

            if (impact.IsError)
                return impact.Errors;

            impacts.Add(impact.Value);
            return impacts;
        }

        if (oldProjectId.HasValue)
        {
            var previousExpenses = await ExpenseImpactCalculator.SumProjectExpensesAsync(
                context,
                cmd.UserId,
                oldProjectId.Value,
                ct);

            var impact = await ExpenseImpactCalculator.BuildImpactAsync(
                context,
                cmd.UserId,
                oldProjectId.Value,
                previousExpenses,
                previousExpenses - oldAmount,
                -oldAmount,
                ct);

            if (impact.IsError)
                return impact.Errors;

            impacts.Add(impact.Value);
        }

        if (cmd.ProjectId.HasValue)
        {
            var previousExpenses = await ExpenseImpactCalculator.SumProjectExpensesAsync(
                context,
                cmd.UserId,
                cmd.ProjectId.Value,
                ct);

            var impact = await ExpenseImpactCalculator.BuildImpactAsync(
                context,
                cmd.UserId,
                cmd.ProjectId.Value,
                previousExpenses,
                previousExpenses + cmd.Amount,
                cmd.Amount,
                ct);

            if (impact.IsError)
                return impact.Errors;

            impacts.Add(impact.Value);
        }

        return impacts;
    }
}
