using SalamHack.Domain.Projects;

namespace SalamHack.Application.Features.Customers.Models;

public sealed record CustomerProjectSummaryDto(
    Guid ProjectId,
    string ProjectName,
    Guid ServiceId,
    string ServiceName,
    ProjectStatus Status,
    decimal SuggestedPrice,
    decimal ProfitMargin,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate);
