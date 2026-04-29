using SalamHack.Application.Features.Expenses.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Expenses.Queries.GetExpenseReceipt;

public sealed record GetExpenseReceiptQuery(
    Guid UserId,
    Guid ExpenseId) : IRequest<Result<ExpenseReceiptFileDto>>;
