using SalamHack.Application.Features.Invoices.Commands.CancelInvoice;
using SalamHack.Application.Features.Invoices.Commands.CreateInvoice;
using SalamHack.Application.Features.Invoices.Commands.CreateQuickInvoice;
using SalamHack.Application.Features.Invoices.Commands.DeleteInvoice;
using SalamHack.Application.Features.Invoices.Commands.MarkInvoiceOverdue;
using SalamHack.Application.Features.Invoices.Commands.RecordAdvancePayment;
using SalamHack.Application.Features.Invoices.Commands.RecordPayment;
using SalamHack.Application.Features.Invoices.Commands.SendInvoice;
using SalamHack.Application.Features.Invoices.Commands.UpdateInvoiceDetails;
using SalamHack.Application.Features.Invoices.Queries.ExportInvoicePdf;
using SalamHack.Application.Features.Invoices.Queries.GetInvoiceById;
using SalamHack.Application.Features.Invoices.Queries.GetInvoices;
using SalamHack.Application.Features.Invoices.Queries.GetPayments;
using SalamHack.Domain.Invoices;
using SalamHack.Domain.Payments;
using SalamHack.Domain.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SalamHack.Api.Controllers;

[Authorize]
public sealed class InvoicesController(ISender sender) : ApiController
{
    [HttpGet]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> GetInvoices(
        [FromQuery] string? search,
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? projectId,
        [FromQuery] InvoiceStatus? status,
        [FromQuery] DateTimeOffset? fromDate,
        [FromQuery] DateTimeOffset? toDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(
            new GetInvoicesQuery(userId, search, customerId, projectId, status, fromDate, toDate, pageNumber, pageSize),
            ct);

        return result.Match(invoices => OkResponse(invoices), Problem);
    }

    [HttpGet("{invoiceId:guid}")]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> GetInvoice(Guid invoiceId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new GetInvoiceByIdQuery(userId, invoiceId), ct);

        return result.Match(invoice => OkResponse(invoice), Problem);
    }

    [HttpGet("{invoiceId:guid}/pdf")]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> ExportInvoicePdf(Guid invoiceId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new ExportInvoicePdfQuery(userId, invoiceId), ct);

        return result.Match(file => OkResponse(file, "Invoice PDF exported successfully."), Problem);
    }

    [HttpGet("{invoiceId:guid}/payments")]
    [EnableRateLimiting("user-read")]
    public async Task<IActionResult> GetInvoicePayments(
        Guid invoiceId,
        [FromQuery] DateTimeOffset? fromDate,
        [FromQuery] DateTimeOffset? toDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(
            new GetPaymentsQuery(userId, invoiceId, fromDate, toDate, pageNumber, pageSize),
            ct);

        return result.Match(payments => OkResponse(payments), Problem);
    }

    [HttpPost]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> CreateInvoice(
        [FromBody] CreateInvoiceRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new CreateInvoiceCommand(
            userId,
            request.ProjectId,
            request.InvoiceNumber,
            request.TotalAmount,
            request.AdvanceAmount,
            request.IssueDate,
            request.DueDate,
            request.Currency,
            request.Notes), ct);

        return result.Match(
            invoice => CreatedResponse(nameof(GetInvoice), new { invoiceId = invoice.Id }, invoice, "Invoice created successfully."),
            Problem);
    }

    [HttpPost("quick-draft")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> CreateQuickDraftInvoice(
        [FromBody] CreateQuickInvoiceRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new CreateQuickInvoiceCommand(
            userId,
            request.CustomerId,
            request.ServiceName,
            request.TotalAmount,
            request.AdvanceAmount,
            request.InvoiceNumber,
            request.IssueDate,
            request.DueDate,
            request.Currency,
            request.Notes,
            request.ProjectName,
            request.EstimatedHours,
            request.ToolCost,
            request.Revision,
            request.IsUrgent,
            request.StartDate,
            request.EndDate,
            request.ServiceCategory), ct);

        return result.Match(
            invoice => CreatedResponse(nameof(GetInvoice), new { invoiceId = invoice.Id }, invoice, "Invoice draft created successfully."),
            Problem);
    }

    [HttpPut("{invoiceId:guid}")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> UpdateInvoice(
        Guid invoiceId,
        [FromBody] UpdateInvoiceRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new UpdateInvoiceDetailsCommand(
            userId,
            invoiceId,
            request.TotalAmount,
            request.AdvanceAmount,
            request.IssueDate,
            request.DueDate,
            request.Currency,
            request.Notes), ct);

        return result.Match(invoice => OkResponse(invoice, "Invoice updated successfully."), Problem);
    }

    [HttpPost("{invoiceId:guid}/payments")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> RecordPayment(
        Guid invoiceId,
        [FromBody] RecordPaymentRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new RecordPaymentCommand(
            userId,
            invoiceId,
            request.Amount,
            request.Method,
            request.PaymentDate,
            request.Currency,
            request.Notes), ct);

        return result.Match(invoice => OkResponse(invoice, "Payment recorded successfully."), Problem);
    }

    [HttpPost("{invoiceId:guid}/advance-payment")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> RecordAdvancePayment(
        Guid invoiceId,
        [FromBody] RecordAdvancePaymentRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new RecordAdvancePaymentCommand(
            userId,
            invoiceId,
            request.Method,
            request.PaymentDate,
            request.Currency,
            request.Notes), ct);

        return result.Match(invoice => OkResponse(invoice, "Advance payment recorded successfully."), Problem);
    }

    [HttpPost("{invoiceId:guid}/send")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> SendInvoice(Guid invoiceId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new SendInvoiceCommand(userId, invoiceId), ct);

        return result.Match(invoice => OkResponse(invoice, "Invoice sent successfully."), Problem);
    }

    [HttpPost("{invoiceId:guid}/cancel")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> CancelInvoice(Guid invoiceId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new CancelInvoiceCommand(userId, invoiceId), ct);

        return result.Match(invoice => OkResponse(invoice, "Invoice cancelled successfully."), Problem);
    }

    [HttpPost("{invoiceId:guid}/mark-overdue")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> MarkOverdue(Guid invoiceId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new MarkInvoiceOverdueCommand(userId, invoiceId), ct);

        return result.Match(invoice => OkResponse(invoice, "Invoice marked overdue successfully."), Problem);
    }

    [HttpDelete("{invoiceId:guid}")]
    [EnableRateLimiting("user-write")]
    public async Task<IActionResult> DeleteInvoice(Guid invoiceId, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var result = await sender.Send(new DeleteInvoiceCommand(userId, invoiceId), ct);

        return result.Match(_ => DeletedResponse("Invoice deleted successfully."), Problem);
    }
}

public sealed record CreateInvoiceRequest(
    Guid ProjectId,
    string InvoiceNumber,
    decimal TotalAmount,
    decimal AdvanceAmount,
    DateTimeOffset IssueDate,
    DateTimeOffset DueDate,
    string Currency,
    string? Notes);

public sealed record CreateQuickInvoiceRequest(
    Guid CustomerId,
    string ServiceName,
    decimal TotalAmount,
    decimal AdvanceAmount = 0,
    string? InvoiceNumber = null,
    DateTimeOffset? IssueDate = null,
    DateTimeOffset? DueDate = null,
    string Currency = "SAR",
    string? Notes = null,
    string? ProjectName = null,
    decimal? EstimatedHours = null,
    decimal ToolCost = 0,
    int Revision = 0,
    bool IsUrgent = false,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null,
    ServiceCategory ServiceCategory = ServiceCategory.Other);

public sealed record UpdateInvoiceRequest(
    decimal TotalAmount,
    decimal AdvanceAmount,
    DateTimeOffset IssueDate,
    DateTimeOffset DueDate,
    string Currency,
    string? Notes);

public sealed record RecordPaymentRequest(
    decimal Amount,
    PaymentMethod Method,
    DateTimeOffset PaymentDate,
    string Currency,
    string? Notes);

public sealed record RecordAdvancePaymentRequest(
    PaymentMethod Method,
    DateTimeOffset PaymentDate,
    string Currency,
    string? Notes);
