using SalamHack.Application.Features.Expenses.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Expenses.Queries.PreviewExpenseImpact;

public sealed record PreviewExpenseImpactQuery(
    Guid UserId,
    Guid ProjectId,
    decimal Amount) : IRequest<Result<ExpenseImpactPreviewDto>>;
