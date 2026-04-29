using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Expenses.Commands.DeleteExpenseReceipt;

public sealed record DeleteExpenseReceiptCommand(
    Guid UserId,
    Guid ExpenseId) : IRequest<Result<Deleted>>;
