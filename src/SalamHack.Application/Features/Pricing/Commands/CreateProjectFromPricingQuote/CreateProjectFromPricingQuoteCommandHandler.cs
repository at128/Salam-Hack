using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Pricing.Models;
using SalamHack.Application.Features.Projects;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Projects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Pricing.Commands.CreateProjectFromPricingQuote;

public sealed class CreateProjectFromPricingQuoteCommandHandler(
    IAppDbContext context,
    IServiceHistoryAnalyzer serviceHistoryAnalyzer)
    : IRequestHandler<CreateProjectFromPricingQuoteCommand, Result<ProjectFromPricingQuoteDto>>
{
    public async Task<Result<ProjectFromPricingQuoteDto>> Handle(
        CreateProjectFromPricingQuoteCommand cmd,
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

        var calculation = await PricingQuoteBuilder.CalculateAsync(
            context,
            serviceHistoryAnalyzer,
            cmd.UserId,
            cmd.ServiceId,
            cmd.EstimatedHours,
            cmd.Complexity,
            PricingQuoteBuilder.DefaultRecentProjectCount,
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
        context.Projects.Add(project);
        await context.SaveChangesAsync(ct);

        var projectDto = project.ToDto(
            customer.CustomerName,
            calculation.Value.Service.ServiceName,
            calculation.Value.Service.Category,
            additionalExpenses: 0);

        return new ProjectFromPricingQuoteDto(
            projectDto,
            calculation.Value.Quote,
            cmd.SelectedPlan,
            selectedPrice,
            PricingQuoteBuilder.GetAdvanceAmount(calculation.Value.Pricing, cmd.SelectedPlan));
    }

    private static bool CanUsePlan(PricingQuoteDto quote, Domain.Pricing.PricingPlanType selectedPlan)
        => quote.Plans.Any(p => p.PlanType == selectedPlan && p.IsViable);
}
