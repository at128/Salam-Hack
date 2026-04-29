using SalamHack.Application.Features.Expenses.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Expenses.Queries.GetExpenseCategoryBreakdown;

public sealed record GetExpenseCategoryBreakdownQuery(
    Guid UserId,
    DateTimeOffset? FromUtc = null,
    DateTimeOffset? ToUtc = null,
    Guid? ProjectId = null) : IRequest<Result<ExpenseCategoryBreakdownDto>>;
