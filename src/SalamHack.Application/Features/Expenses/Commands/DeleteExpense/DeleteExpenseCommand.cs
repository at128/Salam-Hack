using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Expenses.Commands.DeleteExpense;

public sealed record DeleteExpenseCommand(Guid UserId, Guid ExpenseId) : IRequest<Result<Deleted>>;
