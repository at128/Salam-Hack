using SalamHack.Application.Features.Analyses.Models;
using SalamHack.Domain.Analyses;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Analyses.Queries.GetProjectAnalyses;

public sealed record GetProjectAnalysesQuery(
    Guid UserId,
    Guid ProjectId,
    AnalysisType? Type = null) : IRequest<Result<IReadOnlyCollection<AnalysisDto>>>;
