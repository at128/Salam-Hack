using SalamHack.Application.Features.Expenses.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Expenses;
using MediatR;

namespace SalamHack.Application.Features.Expenses.Commands.CreateExpense;

public sealed record CreateExpenseCommand(
    Guid UserId,
    Guid? ProjectId,
    ExpenseCategory Category,
    string Description,
    decimal Amount,
    bool IsRecurring,
    DateTimeOffset ExpenseDate,
    RecurrenceInterval? RecurrenceInterval,
    DateTimeOffset? RecurrenceEndDate,
    string Currency) : IRequest<Result<ExpenseDto>>;
