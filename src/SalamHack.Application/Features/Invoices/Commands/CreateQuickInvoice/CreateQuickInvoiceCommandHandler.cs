using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Constants;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Invoices;
using SalamHack.Domain.Projects;
using SalamHack.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Invoices.Commands.CreateQuickInvoice;

public sealed class CreateQuickInvoiceCommandHandler(
    IAppDbContext context,
    TimeProvider timeProvider)
    : IRequestHandler<CreateQuickInvoiceCommand, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(CreateQuickInvoiceCommand cmd, CancellationToken ct)
    {
        var customer = await context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == cmd.CustomerId && c.UserId == cmd.UserId, ct);

        if (customer is null)
            return ApplicationErrors.Customers.CustomerNotFound;

        var serviceName = cmd.ServiceName.Trim();
        var service = await context.Services
            .FirstOrDefaultAsync(s => s.UserId == cmd.UserId && s.ServiceName == serviceName, ct);

        if (service is not null && !service.IsActive)
            return ApplicationErrors.Services.InactiveServiceCannotBeUsed;

        var now = timeProvider.GetUtcNow();
        var issueDate = cmd.IssueDate ?? now;
        var dueDate = cmd.DueDate ?? issueDate.AddDays(30);
        var startDate = cmd.StartDate ?? issueDate;
        var endDate = cmd.EndDate ?? dueDate;
        var invoiceNumberResult = await ResolveInvoiceNumberAsync(cmd.UserId, cmd.InvoiceNumber, now, ct);
        if (invoiceNumberResult.IsError)
            return invoiceNumberResult.Errors;

        var invoiceNumber = invoiceNumberResult.Value;
        var projectName = await ResolveProjectNameAsync(cmd.UserId, cmd.ProjectName, serviceName, invoiceNumber, ct);
        var estimatedHours = cmd.EstimatedHours ?? EstimateHours(cmd.TotalAmount);
        var createdService = service is null;

        if (createdService)
        {
            var serviceResult = Service.Create(
                cmd.UserId,
                serviceName,
                cmd.ServiceCategory,
                ApplicationConstants.BusinessRules.DefaultRevenueRatePerHour,
                defaultRevisions: 0);

            if (serviceResult.IsError)
                return serviceResult.Errors;

            service = serviceResult.Value;
        }

        var serviceId = service!.Id;
        var projectResult = Project.Create(
            cmd.UserId,
            cmd.CustomerId,
            serviceId,
            projectName,
            estimatedHours,
            cmd.ToolCost,
            cmd.Revision,
            cmd.IsUrgent,
            cmd.TotalAmount,
            startDate,
            endDate);

        if (projectResult.IsError)
            return projectResult.Errors;

        var project = projectResult.Value;
        var invoiceResult = Invoice.Create(
            cmd.UserId,
            project.Id,
            project.CustomerId,
            invoiceNumber,
            cmd.TotalAmount,
            cmd.AdvanceAmount,
            issueDate,
            dueDate,
            cmd.Currency,
            cmd.Notes);

        if (invoiceResult.IsError)
            return invoiceResult.Errors;

        await using var transaction = await context.BeginTransactionAsync(ct);

        if (createdService)
            context.Services.Add(service);

        context.Projects.Add(project);
        context.Invoices.Add(invoiceResult.Value);
        await context.SaveChangesAsync(ct);

        var invoiceDto = await LoadInvoiceDtoAsync(invoiceResult.Value.Id, cmd.UserId, ct);
        if (invoiceDto is null)
            return ApplicationErrors.Invoices.InvoiceNotFound;

        await transaction.CommitAsync(ct);

        return invoiceDto;
    }

    private async Task<Result<string>> ResolveInvoiceNumberAsync(
        Guid userId,
        string? requestedInvoiceNumber,
        DateTimeOffset now,
        CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(requestedInvoiceNumber))
        {
            var invoiceNumber = requestedInvoiceNumber.Trim();
            var exists = await context.Invoices
                .AnyAsync(i => i.UserId == userId && i.InvoiceNumber == invoiceNumber, ct);

            if (exists)
                return ApplicationErrors.Invoices.InvoiceNumberAlreadyExists;

            return invoiceNumber;
        }

        for (var attempt = 0; attempt < 100; attempt++)
        {
            var candidate = attempt == 0
                ? $"INV-{now:yyyyMMddHHmmss}"
                : $"INV-{now:yyyyMMddHHmmss}-{attempt:D2}";

            var exists = await context.Invoices
                .AnyAsync(i => i.UserId == userId && i.InvoiceNumber == candidate, ct);

            if (!exists)
                return candidate;
        }

        return $"INV-{Guid.CreateVersion7():N}";
    }

    private async Task<string> ResolveProjectNameAsync(
        Guid userId,
        string? requestedProjectName,
        string serviceName,
        string invoiceNumber,
        CancellationToken ct)
    {
        var baseName = string.IsNullOrWhiteSpace(requestedProjectName)
            ? $"{serviceName} - {invoiceNumber}"
            : requestedProjectName.Trim();
        baseName = TrimToMaxLength(baseName, 180);

        for (var attempt = 0; attempt < 100; attempt++)
        {
            var suffix = attempt == 0 ? string.Empty : $" ({attempt + 1})";
            var candidate = TrimToMaxLength(baseName, 200 - suffix.Length) + suffix;
            var exists = await context.Projects
                .AnyAsync(p => p.UserId == userId && p.ProjectName == candidate, ct);

            if (!exists)
                return candidate;
        }

        return TrimToMaxLength($"{baseName} - {Guid.CreateVersion7():N}", 200);
    }

    private async Task<InvoiceDto?> LoadInvoiceDtoAsync(Guid invoiceId, Guid userId, CancellationToken ct)
    {
        var invoice = await context.Invoices
            .AsNoTracking()
            .Include(i => i.Project)
                .ThenInclude(p => p.Customer)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.UserId == userId, ct);

        return invoice?.ToDto();
    }

    private static decimal EstimateHours(decimal totalAmount)
        => Math.Max(1, Math.Round(totalAmount / ApplicationConstants.BusinessRules.DefaultRevenueRatePerHour, 2));

    private static string TrimToMaxLength(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength].Trim();
}
