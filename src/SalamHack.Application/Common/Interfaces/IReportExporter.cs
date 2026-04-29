using SalamHack.Application.Features.Reports.Models;

namespace SalamHack.Application.Common.Interfaces;

public interface IReportExporter
{
    Task<ReportExportFile> ExportProfitabilityReportAsync(
        ProfitabilityReportDto report,
        ReportExportFormat format,
        CancellationToken cancellationToken = default);
}

public enum ReportExportFormat
{
    Pdf,
    Csv,
    Xlsx
}

public sealed record ReportExportFile(
    string FileName,
    string ContentType,
    byte[] Content);
