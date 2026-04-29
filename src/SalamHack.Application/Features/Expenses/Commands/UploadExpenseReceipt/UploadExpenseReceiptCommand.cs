using SalamHack.Application.Features.Expenses.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Expenses.Commands.UploadExpenseReceipt;

public sealed record UploadExpenseReceiptCommand(
    Guid UserId,
    Guid ExpenseId,
    string FileName,
    string ContentType,
    byte[] Content) : IRequest<Result<ExpenseReceiptDto>>;
