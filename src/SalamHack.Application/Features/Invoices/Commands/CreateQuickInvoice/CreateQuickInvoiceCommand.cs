using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Services;
using MediatR;

namespace SalamHack.Application.Features.Invoices.Commands.CreateQuickInvoice;

public sealed record CreateQuickInvoiceCommand(
    Guid UserId,
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
    ServiceCategory ServiceCategory = ServiceCategory.Other) : IRequest<Result<InvoiceDto>>;
