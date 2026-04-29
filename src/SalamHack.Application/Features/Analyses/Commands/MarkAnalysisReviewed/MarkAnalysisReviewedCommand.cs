using SalamHack.Application.Features.Analyses.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Analyses.Commands.MarkAnalysisReviewed;

public sealed record MarkAnalysisReviewedCommand(
    Guid UserId,
    Guid AnalysisId) : IRequest<Result<AnalysisDto>>;
