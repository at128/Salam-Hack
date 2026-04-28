using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Expenses.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Expenses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Expenses.Commands.CreateExpense;

public sealed class CreateExpenseCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateExpenseCommand, Result<ExpenseDto>>
{
    public async Task<Result<ExpenseDto>> Handle(CreateExpenseCommand cmd, CancellationToken ct)
    {
        string? projectName = null;

        if (cmd.ProjectId.HasValue)
        {
            var project = await context.Projects
                .AsNoTracking()
                .Where(p => p.Id == cmd.ProjectId.Value && p.UserId == cmd.UserId)
                .Select(p => new { p.ProjectName })
                .FirstOrDefaultAsync(ct);

            if (project is null)
                return ApplicationErrors.Projects.ProjectNotFound;

            projectName = project.ProjectName;
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

        return expense.ToDto(projectName);
    }
}
