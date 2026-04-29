using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Invoices;
using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Application.Features.Pricing.Models;
using SalamHack.Application.Features.Projects;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Invoices;
using SalamHack.Domain.Projects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Pricing.Commands.CreateInvoiceFromPricingQuote;

public sealed class CreateInvoiceFromPricingQuoteCommandHandler(
    IAppDbContext context,
    IServiceHistoryAnalyzer serviceHistoryAnalyzer)
    : IRequestHandler<CreateInvoiceFromPricingQuoteCommand, Result<InvoiceFromPricingQuoteDto>>
{
    public async Task<Result<InvoiceFromPricingQuoteDto>> Handle(
        CreateInvoiceFromPricingQuoteCommand cmd,
        CancellationToken ct)
    {
        var customer = await context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == cmd.CustomerId && c.UserId == cmd.UserId, ct);

        if (customer is null)
            return ApplicationErrors.Customers.CustomerNotFound;

        var projectName = cmd.ProjectName.Trim();
        var nameExists = await context.Projects
            .AnyAsync(p => p.UserId == cmd.UserId && p.ProjectName == projectName, ct);

        if (nameExists)
            return ApplicationErrors.Projects.ProjectNameAlreadyExists;

        var invoiceNumber = cmd.InvoiceNumber.Trim();
        var numberExists = await context.Invoices
            .AnyAsync(i => i.UserId == cmd.UserId && i.InvoiceNumber == invoiceNumber, ct);

        if (numberExists)
            return ApplicationErrors.Invoices.InvoiceNumberAlreadyExists;

        var calculation = await PricingQuoteBuilder.CalculateAsync(
            context,
            serviceHistoryAnalyzer,
            cmd.UserId,
            cmd.ServiceId,
            cmd.EstimatedHours,
            cmd.Complexity,
            cmd.ToolCost,
            PricingQuoteBuilder.DefaultRecentProjectCount,
            cmd.Revision,
            cmd.IsUrgent,
            ct);

        if (calculation.IsError)
            return calculation.Errors;

        if (!CanUsePlan(calculation.Value.Quote, cmd.SelectedPlan))
            return ApplicationErrors.Pricing.PricingPlanCannotBeUsed;

        var selectedPrice = calculation.Value.Pricing.GetPrice(cmd.SelectedPlan);
        var projectResult = Project.Create(
            cmd.UserId,
            cmd.CustomerId,
            cmd.ServiceId,
            projectName,
            cmd.EstimatedHours,
            cmd.ToolCost,
            cmd.Revision,
            cmd.IsUrgent,
            selectedPrice,
            cmd.StartDate,
            cmd.EndDate);

        if (projectResult.IsError)
            return projectResult.Errors;

        var project = projectResult.Value;
        var advanceAmount = PricingQuoteBuilder.GetAdvanceAmount(calculation.Value.Pricing, cmd.SelectedPlan);
        var invoiceResult = Invoice.Create(
            cmd.UserId,
            project.Id,
            project.CustomerId,
            invoiceNumber,
            selectedPrice,
            advanceAmount,
            cmd.IssueDate,
            cmd.DueDate,
            cmd.Currency,
            cmd.Notes);

        if (invoiceResult.IsError)
            return invoiceResult.Errors;

        await using var transaction = await context.BeginTransactionAsync(ct);

        context.Projects.Add(project);
        context.Invoices.Add(invoiceResult.Value);
        await context.SaveChangesAsync(ct);

        var invoiceDto = await LoadInvoiceDtoAsync(invoiceResult.Value.Id, cmd.UserId, ct);
        if (invoiceDto is null)
            return ApplicationErrors.Invoices.InvoiceNotFound;

        await transaction.CommitAsync(ct);

        var projectDto = project.ToDto(
            customer.CustomerName,
            calculation.Value.Service.ServiceName,
            calculation.Value.Service.Category,
            additionalExpenses: 0);

        return new InvoiceFromPricingQuoteDto(
            projectDto,
            invoiceDto,
            calculation.Value.Quote,
            cmd.SelectedPlan,
            selectedPrice,
            advanceAmount);
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

    private static bool CanUsePlan(PricingQuoteDto quote, Domain.Pricing.PricingPlanType selectedPlan)
        => quote.Plans.Any(p => p.PlanType == selectedPlan && p.IsViable);
}
