using SalamHack.Domain.Projects;
using SalamHack.Domain.Services;

namespace SalamHack.Application.Features.Projects.Models;

public sealed record ProjectDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    Guid ServiceId,
    string ServiceName,
    ServiceCategory ServiceCategory,
    string ProjectName,
    decimal EstimatedHours,
    decimal ActualHours,
    decimal ToolCost,
    int Revision,
    bool IsUrgent,
    decimal SuggestedPrice,
    decimal MinPrice,
    decimal AdvanceAmount,
    decimal ProfitMargin,
    ProjectStatus Status,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    ProjectHealthDto Health,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset LastModifiedUtc);
