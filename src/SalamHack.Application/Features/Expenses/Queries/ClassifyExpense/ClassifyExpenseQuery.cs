using SalamHack.Application.Features.Expenses.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Expenses.Queries.ClassifyExpense;

public sealed record ClassifyExpenseQuery(string Description) : IRequest<Result<ExpenseClassificationDto>>;
