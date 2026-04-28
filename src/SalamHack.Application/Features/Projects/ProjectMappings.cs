using SalamHack.Application.Features.Projects.Models;
using SalamHack.Domain.Projects;
using SalamHack.Domain.Services;

namespace SalamHack.Application.Features.Projects;

internal static class ProjectMappings
{
    public static ProjectDto ToDto(this Project project, decimal additionalExpenses)
        => project.ToDto(
            project.Customer.CustomerName,
            project.Service.ServiceName,
            project.Service.Category,
            additionalExpenses);

    public static ProjectDto ToDto(
        this Project project,
        string customerName,
        string serviceName,
        ServiceCategory serviceCategory,
        decimal additionalExpenses)
    {
        var health = project.GetHealthSnapshot(additionalExpenses);

        return new ProjectDto(
            project.Id,
            project.CustomerId,
            customerName,
            project.ServiceId,
            serviceName,
            serviceCategory,
            project.ProjectName,
            project.EstimatedHours,
            project.ActualHours,
            project.ToolCost,
            project.Revision,
            project.IsUrgent,
            project.SuggestedPrice,
            project.MinPrice,
            project.AdvanceAmount,
            project.ProfitMargin,
            project.Status,
            project.StartDate,
            project.EndDate,
            health.IsError ? EmptyHealth(additionalExpenses) : health.Value.ToDto(),
            project.CreatedAtUtc,
            project.LastModifiedUtc);
    }

    public static ProjectListItemDto ToListItemDto(this Project project, decimal additionalExpenses)
    {
        var health = project.GetHealthSnapshot(additionalExpenses);

        return new ProjectListItemDto(
            project.Id,
            project.ProjectName,
            project.CustomerId,
            project.Customer.CustomerName,
            project.ServiceId,
            project.Service.ServiceName,
            project.SuggestedPrice,
            project.ProfitMargin,
            project.Status,
            project.StartDate,
            project.EndDate,
            health.IsError ? EmptyHealth(additionalExpenses) : health.Value.ToDto());
    }

    private static ProjectHealthDto ToDto(this ProjectHealthSnapshot snapshot)
        => new(
            snapshot.BaseCost,
            snapshot.AdditionalExpenses,
            snapshot.TotalCost,
            snapshot.Profit,
            snapshot.MarginPercent,
            snapshot.HourlyProfit,
            snapshot.HealthStatus,
            snapshot.IsHealthy);

    private static ProjectHealthDto EmptyHealth(decimal additionalExpenses)
        => new(0, additionalExpenses, additionalExpenses, 0, 0, 0, ProjectHealthStatus.Critical, false);
}
