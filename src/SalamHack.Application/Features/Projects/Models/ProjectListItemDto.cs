using SalamHack.Domain.Projects;

namespace SalamHack.Application.Features.Projects.Models;

public sealed record ProjectListItemDto(
    Guid Id,
    string ProjectName,
    Guid CustomerId,
    string CustomerName,
    Guid ServiceId,
    string ServiceName,
    decimal SuggestedPrice,
    decimal ProfitMargin,
    ProjectStatus Status,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    ProjectHealthDto Health);
