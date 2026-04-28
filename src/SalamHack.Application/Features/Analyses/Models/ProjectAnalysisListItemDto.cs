using SalamHack.Domain.Projects;

namespace SalamHack.Application.Features.Analyses.Models;

public sealed record ProjectAnalysisListItemDto(
    Guid ProjectId,
    string ProjectName,
    Guid CustomerId,
    string CustomerName,
    decimal MarginPercent,
    ProjectHealthStatus HealthStatus,
    decimal Profit);
