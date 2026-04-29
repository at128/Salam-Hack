using SalamHack.Application.Features.Analyses.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Analyses.Commands.GenerateProjectAnalysis;

public sealed record GenerateProjectAnalysisCommand(
    Guid UserId,
    Guid ProjectId) : IRequest<Result<AnalysisDto>>;
