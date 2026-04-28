using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Expenses.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Expenses.Commands.UpdateExpense;

public sealed class UpdateExpenseCommandHandler(IAppDbContext context)
    : IRequestHandler<UpdateExpenseCommand, Result<ExpenseDto>>
{
    public async Task<Result<ExpenseDto>> Handle(UpdateExpenseCommand cmd, CancellationToken ct)
    {
        var expense = await context.Expenses
            .FirstOrDefaultAsync(e => e.Id == cmd.ExpenseId && e.UserId == cmd.UserId, ct);

        if (expense is null)
            return ApplicationErrors.Expenses.ExpenseNotFound;

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

        return expense.ToDto(projectName);
    }
}
