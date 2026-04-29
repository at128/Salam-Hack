using SalamHack.Application.Common.Models;
using SalamHack.Application.Features.Expenses.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Expenses;
using MediatR;

namespace SalamHack.Application.Features.Expenses.Queries.GetExpenses;

public sealed record GetExpensesQuery(
    Guid UserId,
    string? Search,
    Guid? ProjectId,
    ExpenseCategory? Category,
    DateTimeOffset? FromDate,
    DateTimeOffset? ToDate,
    bool? IsRecurring,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PaginatedList<ExpenseListItemDto>>>;
