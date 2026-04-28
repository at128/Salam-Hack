using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Reports.Models;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Reports.Commands.ExportProfitabilityReport;

public sealed record ExportProfitabilityReportCommand(
    Guid UserId,
    ReportExportFormat Format,
    DateTimeOffset? FromUtc = null,
    DateTimeOffset? ToUtc = null) : IRequest<Result<ReportExportDto>>;
