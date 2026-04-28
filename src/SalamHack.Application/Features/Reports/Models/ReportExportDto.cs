using SalamHack.Application.Common.Interfaces;

namespace SalamHack.Application.Features.Reports.Models;

public sealed record ReportExportDto(
    ReportExportFormat Format,
    string FileName,
    string ContentType,
    byte[] Content);
