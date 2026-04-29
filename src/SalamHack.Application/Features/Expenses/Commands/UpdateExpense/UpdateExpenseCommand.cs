using SalamHack.Application.Features.Expenses.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Expenses;
using MediatR;

namespace SalamHack.Application.Features.Expenses.Commands.UpdateExpense;

public sealed record UpdateExpenseCommand(
    Guid UserId,
    Guid ExpenseId,
    Guid? ProjectId,
    ExpenseCategory Category,
    string Description,
    decimal Amount,
    bool IsRecurring,
    DateTimeOffset ExpenseDate,
    RecurrenceInterval? RecurrenceInterval,
    DateTimeOffset? RecurrenceEndDate,
    string Currency) : IRequest<Result<ExpenseDto>>;
