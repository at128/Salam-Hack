using SalamHack.Domain.Projects;

namespace SalamHack.Application.Features.Analyses.Models;

public sealed record ProjectAnalysisDto(
    Guid ProjectId,
    string ProjectName,
    Guid CustomerId,
    string CustomerName,
    Guid ServiceId,
    string ServiceName,
    ProjectHealthStatus HealthStatus,
    string WhatHappened,
    string WhatItMeans,
    string WhatToDo,
    ProjectAnalysisNumbersDto Numbers,
    IReadOnlyCollection<ProjectWhatIfScenarioDto> Scenarios);
