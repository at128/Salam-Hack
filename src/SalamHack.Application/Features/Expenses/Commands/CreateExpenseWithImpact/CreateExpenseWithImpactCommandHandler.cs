using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Expenses.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Expenses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Expenses.Commands.CreateExpenseWithImpact;

public sealed class CreateExpenseWithImpactCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateExpenseWithImpactCommand, Result<ExpenseMutationResultDto>>
{
    public async Task<Result<ExpenseMutationResultDto>> Handle(CreateExpenseWithImpactCommand cmd, CancellationToken ct)
    {
        string? projectName = null;
        var impacts = new List<ExpenseChangeImpactDto>();

        if (cmd.ProjectId.HasValue)
        {
            var currentProjectExpenses = await ExpenseImpactCalculator.SumProjectExpensesAsync(
                context,
                cmd.UserId,
                cmd.ProjectId.Value,
                ct);

            var impact = await ExpenseImpactCalculator.BuildImpactAsync(
                context,
                cmd.UserId,
                cmd.ProjectId.Value,
                currentProjectExpenses,
                currentProjectExpenses + cmd.Amount,
                cmd.Amount,
                ct);

            if (impact.IsError)
                return impact.Errors;

            impacts.Add(impact.Value);
            projectName = impact.Value.ProjectName;
        }

        var expenseResult = Expense.Create(
            cmd.UserId,
            cmd.Description,
            cmd.Amount,
            cmd.Category,
            cmd.ExpenseDate,
            cmd.Currency,
            cmd.ProjectId,
            cmd.IsRecurring,
            cmd.RecurrenceInterval,
            cmd.RecurrenceEndDate);

        if (expenseResult.IsError)
            return expenseResult.Errors;

        var expense = expenseResult.Value;
        context.Expenses.Add(expense);
        await context.SaveChangesAsync(ct);

        return new ExpenseMutationResultDto(expense.ToDto(projectName), impacts);
    }
}
