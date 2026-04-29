using SalamHack.Application.Features.Analyses.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Analyses.Queries.GetProjectAnalysisDashboard;

public sealed record GetProjectAnalysisDashboardQuery(
    Guid UserId,
    Guid? ProjectId = null) : IRequest<Result<ProjectAnalysisDashboardDto>>;
