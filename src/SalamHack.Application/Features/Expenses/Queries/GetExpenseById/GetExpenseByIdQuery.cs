using SalamHack.Application.Features.Expenses.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Expenses.Queries.GetExpenseById;

public sealed record GetExpenseByIdQuery(Guid UserId, Guid ExpenseId) : IRequest<Result<ExpenseDto>>;
