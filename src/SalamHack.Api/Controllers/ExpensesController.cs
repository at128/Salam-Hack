using SalamHack.Application.Features.Expenses.Commands.CreateExpense;
using SalamHack.Application.Features.Expenses.Commands.CreateExpenseWithImpact;
using SalamHack.Application.Features.Expenses.Commands.DeleteExpense;
using SalamHack.Application.Features.Expenses.Commands.DeleteExpenseReceipt;
using SalamHack.Application.Features.Expenses.Commands.UpdateExpense;
using SalamHack.Application.Features.Expenses.Commands.UpdateExpenseWithImpact;
using SalamHack.Application.Features.Expenses.Commands.UploadExpenseReceipt;
using SalamHack.Application.Features.Expenses;
using SalamHack.Application.Features.Expenses.Queries.ClassifyExpense;
using SalamHack.Application.Features.Expenses.Queries.GetExpenseById;
using SalamHack.Application.Features.Expenses.Queries.GetExpenseCategoryBreakdown;
using SalamHack.Application.Features.Expenses.Queries.GetExpenseReceipt;
using SalamHack.Application.Features.Expenses.Queries.GetExpenses;
using SalamHack.Application.Features.Expenses.Queries.PreviewExpenseImpact;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Expenses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SalamHack.Api.Controllers;

[Authorize]
public sealed class ExpensesController(ISender sender) : ApiController
{
    [HttpGet]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> GetExpenses(
        [FromQuery] string? search,
        [FromQuery] Guid? projectId,
        [FromQuery] ExpenseCategory? category,
        [FromQuery] DateTimeOffset? fromDate,
        [FromQuery] DateTimeOffset? toDate,
        [FromQuery] bool? isRecurring,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(
            new GetExpensesQuery(userId, search, projectId, category, fromDate, toDate, isRecurring, pageNumber, pageSize),
            ct);

        return result.Match(expenses => OkResponse(expenses), Problem);
    }

    [HttpGet("category-breakdown")]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> GetCategoryBreakdown(
        [FromQuery] DateTimeOffset? fromUtc,
        [FromQuery] DateTimeOffset? toUtc,
        [FromQuery] Guid? projectId,
        CancellationToken ct = default)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(
            new GetExpenseCategoryBreakdownQuery(userId, fromUtc, toUtc, projectId),
            ct);

        return result.Match(breakdown => OkResponse(breakdown), Problem);
    }

    [HttpGet("impact-preview")]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> PreviewImpact(
        [FromQuery] Guid projectId,
        [FromQuery] decimal amount,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new PreviewExpenseImpactQuery(userId, projectId, amount), ct);

        return result.Match(preview => OkResponse(preview), Problem);
    }

    [HttpGet("{expenseId:guid}")]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> GetExpense(Guid expenseId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new GetExpenseByIdQuery(userId, expenseId), ct);

        return result.Match(expense => OkResponse(expense), Problem);
    }

    [HttpGet("{expenseId:guid}/receipt")]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> GetReceipt(Guid expenseId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new GetExpenseReceiptQuery(userId, expenseId), ct);

        return result.Match(receipt => OkResponse(receipt), Problem);
    }

    [HttpPost("classify")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> Classify(
        [FromBody] ClassifyExpenseRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(new ClassifyExpenseQuery(request.Description), ct);

        return result.Match(classification => OkResponse(classification), Problem);
    }

    [HttpPost]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> CreateExpense(
        [FromBody] ExpenseRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new CreateExpenseCommand(
            userId,
            request.ProjectId,
            request.Category,
            request.Description,
            request.Amount,
            request.IsRecurring,
            request.ExpenseDate,
            request.RecurrenceInterval,
            request.RecurrenceEndDate,
            request.Currency), ct);

        return result.Match(
            expense => CreatedResponse(nameof(GetExpense), new { expenseId = expense.Id }, expense, "Expense created successfully."),
            Problem);
    }

    [HttpPost("{expenseId:guid}/receipt")]
    [EnableRateLimiting("user-write")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(ExpenseReceiptRules.MaxFileSizeBytes + 1024 * 1024)]
    public async Task<IActionResult> UploadReceipt(
        Guid expenseId,
        [FromForm] UploadExpenseReceiptRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        if (request.File is null || request.File.Length == 0)
        {
            return Problem([
                Error.Validation(
                    "Expenses.ReceiptFileRequired",
                    "Receipt file is required.")
            ]);
        }

        await using var stream = request.File.OpenReadStream();

        var result = await sender.Send(new UploadExpenseReceiptCommand(
            userId,
            expenseId,
            request.File.FileName,
            request.File.ContentType,
            stream,
            request.File.Length), ct);

        return result.Match(receipt => OkResponse(receipt, "Receipt uploaded successfully."), Problem);
    }

    [HttpPost("with-impact")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> CreateExpenseWithImpact(
        [FromBody] ExpenseRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new CreateExpenseWithImpactCommand(
            userId,
            request.ProjectId,
            request.Category,
            request.Description,
            request.Amount,
            request.IsRecurring,
            request.ExpenseDate,
            request.RecurrenceInterval,
            request.RecurrenceEndDate,
            request.Currency), ct);

        return result.Match(
            mutation => CreatedResponse(nameof(GetExpense), new { expenseId = mutation.Expense.Id }, mutation, "Expense created successfully."),
            Problem);
    }

    [HttpPut("{expenseId:guid}")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> UpdateExpense(
        Guid expenseId,
        [FromBody] ExpenseRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new UpdateExpenseCommand(
            userId,
            expenseId,
            request.ProjectId,
            request.Category,
            request.Description,
            request.Amount,
            request.IsRecurring,
            request.ExpenseDate,
            request.RecurrenceInterval,
            request.RecurrenceEndDate,
            request.Currency), ct);

        return result.Match(expense => OkResponse(expense, "Expense updated successfully."), Problem);
    }

    [HttpPut("{expenseId:guid}/with-impact")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> UpdateExpenseWithImpact(
        Guid expenseId,
        [FromBody] ExpenseRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new UpdateExpenseWithImpactCommand(
            userId,
            expenseId,
            request.ProjectId,
            request.Category,
            request.Description,
            request.Amount,
            request.IsRecurring,
            request.ExpenseDate,
            request.RecurrenceInterval,
            request.RecurrenceEndDate,
            request.Currency), ct);

        return result.Match(mutation => OkResponse(mutation, "Expense updated successfully."), Problem);
    }

    [HttpDelete("{expenseId:guid}")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> DeleteExpense(Guid expenseId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new DeleteExpenseCommand(userId, expenseId), ct);

        return result.Match(_ => DeletedResponse("Expense deleted successfully."), Problem);
    }

    [HttpDelete("{expenseId:guid}/receipt")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> DeleteReceipt(Guid expenseId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new DeleteExpenseReceiptCommand(userId, expenseId), ct);

        return result.Match(_ => DeletedResponse("Receipt deleted successfully."), Problem);
    }
}

public sealed record ExpenseRequest(
    Guid? ProjectId,
    ExpenseCategory Category,
    string Description,
    decimal Amount,
    bool IsRecurring,
    DateTimeOffset ExpenseDate,
    RecurrenceInterval? RecurrenceInterval,
    DateTimeOffset? RecurrenceEndDate,
    string Currency);

public sealed record ClassifyExpenseRequest(string Description);

public sealed class UploadExpenseReceiptRequest
{
    public IFormFile? File { get; init; }
}
