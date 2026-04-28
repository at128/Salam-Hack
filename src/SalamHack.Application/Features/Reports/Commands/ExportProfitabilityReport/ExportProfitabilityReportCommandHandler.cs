using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Reports.Models;
using SalamHack.Application.Features.Reports.Queries.GetProfitabilityReport;
using SalamHack.Domain.Common.Results;
using MediatR;

namespace SalamHack.Application.Features.Reports.Commands.ExportProfitabilityReport;

public sealed class ExportProfitabilityReportCommandHandler(
    ISender sender,
    IReportExporter reportExporter)
    : IRequestHandler<ExportProfitabilityReportCommand, Result<ReportExportDto>>
{
    public async Task<Result<ReportExportDto>> Handle(ExportProfitabilityReportCommand cmd, CancellationToken ct)
    {
        var reportResult = await sender.Send(
            new GetProfitabilityReportQuery(cmd.UserId, cmd.FromUtc, cmd.ToUtc),
            ct);

        if (reportResult.IsError)
            return reportResult.Errors;

        var file = await reportExporter.ExportProfitabilityReportAsync(
            reportResult.Value,
            cmd.Format,
            ct);

        return new ReportExportDto(
            cmd.Format,
            file.FileName,
            file.ContentType,
            file.Content);
    }
}
