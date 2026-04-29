using System.Globalization;
using System.IO.Compression;
using System.Security;
using System.Text;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Reports.Models;

namespace SalamHack.Infrastructure.Reports;

public sealed class ReportExporter(TimeProvider timeProvider) : IReportExporter
{
    private static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    public Task<ReportExportFile> ExportProfitabilityReportAsync(
        ProfitabilityReportDto report,
        ReportExportFormat format,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(report);

        var exportedAt = timeProvider.GetUtcNow();
        var fileStamp = exportedAt.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

        var file = format switch
        {
            ReportExportFormat.Csv => ExportCsv(report, fileStamp),
            ReportExportFormat.Xlsx => ExportXlsx(report, fileStamp),
            ReportExportFormat.Pdf => ExportPdf(report, fileStamp),
            _ => ExportCsv(report, fileStamp)
        };

        return Task.FromResult(file);
    }

    private static ReportExportFile ExportCsv(ProfitabilityReportDto report, string fileStamp)
    {
        var builder = new StringBuilder();

        foreach (var row in BuildRows(report))
        {
            builder.AppendLine(string.Join(",", row.Select(cell => EscapeCsv(cell.Value))));
        }

        return new ReportExportFile(
            $"profitability-report-{fileStamp}.csv",
            "text/csv; charset=utf-8",
            Utf8WithBom.GetBytes(builder.ToString()));
    }

    private static ReportExportFile ExportXlsx(ProfitabilityReportDto report, string fileStamp)
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddZipEntry(archive, "[Content_Types].xml", XlsxContentTypes());
            AddZipEntry(archive, "_rels/.rels", XlsxRootRelationships());
            AddZipEntry(archive, "xl/workbook.xml", XlsxWorkbook());
            AddZipEntry(archive, "xl/_rels/workbook.xml.rels", XlsxWorkbookRelationships());
            AddZipEntry(archive, "xl/styles.xml", XlsxStyles());
            AddZipEntry(archive, "xl/worksheets/sheet1.xml", XlsxWorksheet(BuildRows(report)));
        }

        return new ReportExportFile(
            $"profitability-report-{fileStamp}.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            stream.ToArray());
    }

    private static ReportExportFile ExportPdf(ProfitabilityReportDto report, string fileStamp)
    {
        var lines = new List<string>
        {
            "Profitability Report",
            $"Period: {FormatDate(report.Period.FromUtc)} to {FormatDate(report.Period.ToUtc)}",
            $"Revenue: {FormatMoney(report.Summary.TotalRevenue)}",
            $"Expenses: {FormatMoney(report.Summary.TotalExpenses)}",
            $"Profit: {FormatMoney(report.Summary.TotalProfit)}",
            $"Margin: {FormatPercent(report.Summary.OverallMarginPercent)}",
            string.Empty,
            "Top performers"
        };

        lines.AddRange(report.TopPerformers.Take(8).Select(item =>
            $"{item.Name}: profit {FormatMoney(item.Profit)}, margin {FormatPercent(item.MarginPercent)}"));

        lines.Add(string.Empty);
        lines.Add("Lowest performers");
        lines.AddRange(report.LowestPerformers.Take(8).Select(item =>
            $"{item.Name}: profit {FormatMoney(item.Profit)}, margin {FormatPercent(item.MarginPercent)}"));

        if (!string.IsNullOrWhiteSpace(report.Insight))
        {
            lines.Add(string.Empty);
            lines.Add("Insight");
            lines.Add(SanitizePdfText(report.Insight));
        }

        return new ReportExportFile(
            $"profitability-report-{fileStamp}.pdf",
            "application/pdf",
            BuildSimplePdf(lines));
    }

    private static IReadOnlyList<IReadOnlyList<CellValue>> BuildRows(ProfitabilityReportDto report)
    {
        var rows = new List<IReadOnlyList<CellValue>>
        {
            TextRow("Profitability Report"),
            TextRow("Period", $"{FormatDate(report.Period.FromUtc)} - {FormatDate(report.Period.ToUtc)}"),
            Array.Empty<CellValue>(),
            TextRow("Summary"),
            MixedRow("Total revenue", report.Summary.TotalRevenue),
            MixedRow("Total expenses", report.Summary.TotalExpenses),
            MixedRow("Total profit", report.Summary.TotalProfit),
            MixedRow("Overall margin percent", report.Summary.OverallMarginPercent),
            Array.Empty<CellValue>(),
            TextRow("Monthly trend"),
            TextRow("Year", "Month", "Revenue", "Expenses", "Profit")
        };

        rows.AddRange(report.MonthlyTrend.Select(point => Row(
            point.Year.ToString(CultureInfo.InvariantCulture),
            point.Month.ToString(CultureInfo.InvariantCulture),
            point.Revenue,
            point.Expenses,
            point.Profit)));

        AddBreakdown(rows, "By service", report.ByService);
        AddBreakdown(rows, "By customer", report.ByCustomer);
        AddBreakdown(rows, "By project", report.ByProject);
        AddBreakdown(rows, "Top performers", report.TopPerformers);
        AddBreakdown(rows, "Lowest performers", report.LowestPerformers);

        if (!string.IsNullOrWhiteSpace(report.Insight))
        {
            rows.Add([]);
            rows.Add(TextRow("Insight"));
            rows.Add(TextRow(report.Insight));
        }

        return rows;
    }

    private static void AddBreakdown(
        List<IReadOnlyList<CellValue>> rows,
        string title,
        IReadOnlyCollection<ProfitabilityBreakdownItemDto> items)
    {
        rows.Add([]);
        rows.Add(TextRow(title));
        rows.Add(TextRow("Name", "Type", "Revenue", "Cost", "Profit", "Margin percent"));

        rows.AddRange(items.Select(item => Row(
            item.Name,
            item.Type.ToString(),
            item.Revenue,
            item.Cost,
            item.Profit,
            item.MarginPercent)));
    }

    private static IReadOnlyList<CellValue> TextRow(params string[] values)
        => values.Select(value => new CellValue(value, IsNumber: false)).ToList();

    private static IReadOnlyList<CellValue> MixedRow(string label, decimal value)
        => [new(label, IsNumber: false), new(FormatDecimal(value), IsNumber: true)];

    private static IReadOnlyList<CellValue> Row(params object[] values)
        => values
            .Select(value => value switch
            {
                decimal decimalValue => new CellValue(FormatDecimal(decimalValue), IsNumber: true),
                int intValue => new CellValue(intValue.ToString(CultureInfo.InvariantCulture), IsNumber: true),
                _ => new CellValue(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty, IsNumber: false)
            })
            .ToList();

    private static string EscapeCsv(string value)
        => value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r')
            ? $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\""
            : value;

    private static string XlsxWorksheet(IReadOnlyList<IReadOnlyList<CellValue>> rows)
    {
        var builder = new StringBuilder();
        builder.Append("""
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
  <sheetData>
""");

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var rowNumber = rowIndex + 1;
            builder.Append(CultureInfo.InvariantCulture, $"    <row r=\"{rowNumber}\">");

            var row = rows[rowIndex];
            for (var columnIndex = 0; columnIndex < row.Count; columnIndex++)
            {
                var reference = $"{ColumnName(columnIndex + 1)}{rowNumber}";
                var cell = row[columnIndex];

                if (cell.IsNumber)
                {
                    builder.Append(CultureInfo.InvariantCulture, $"<c r=\"{reference}\"><v>{cell.Value}</v></c>");
                }
                else
                {
                    builder.Append(CultureInfo.InvariantCulture, $"<c r=\"{reference}\" t=\"inlineStr\"><is><t>{XmlEscape(cell.Value)}</t></is></c>");
                }
            }

            builder.AppendLine("</row>");
        }

        builder.Append("""
  </sheetData>
</worksheet>
""");

        return builder.ToString();
    }

    private static string XlsxContentTypes()
        => """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml" ContentType="application/xml"/>
  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
  <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
</Types>
""";

    private static string XlsxRootRelationships()
        => """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
</Relationships>
""";

    private static string XlsxWorkbook()
        => """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
  <sheets>
    <sheet name="Profitability" sheetId="1" r:id="rId1"/>
  </sheets>
</workbook>
""";

    private static string XlsxWorkbookRelationships()
        => """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
</Relationships>
""";

    private static string XlsxStyles()
        => """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
  <fonts count="1"><font><sz val="11"/><name val="Calibri"/></font></fonts>
  <fills count="1"><fill><patternFill patternType="none"/></fill></fills>
  <borders count="1"><border><left/><right/><top/><bottom/><diagonal/></border></borders>
  <cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>
  <cellXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/></cellXfs>
</styleSheet>
""";

    private static void AddZipEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Fastest);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream, Utf8NoBom);
        writer.Write(content);
    }

    private static byte[] BuildSimplePdf(IReadOnlyList<string> lines)
    {
        var contentBuilder = new StringBuilder();
        contentBuilder.AppendLine("BT");
        contentBuilder.AppendLine("/F1 16 Tf");
        contentBuilder.AppendLine("50 790 Td");
        contentBuilder.AppendLine($"({EscapePdfString(lines.FirstOrDefault() ?? "Report")}) Tj");
        contentBuilder.AppendLine("/F1 10 Tf");

        foreach (var line in lines.Skip(1).Take(42))
        {
            contentBuilder.AppendLine("0 -16 Td");
            contentBuilder.AppendLine($"({EscapePdfString(line)}) Tj");
        }

        contentBuilder.AppendLine("ET");

        var content = contentBuilder.ToString();
        var contentLength = Encoding.ASCII.GetByteCount(content);
        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            $"<< /Length {contentLength} >>\nstream\n{content}endstream"
        };

        var builder = new StringBuilder();
        builder.AppendLine("%PDF-1.4");

        var offsets = new List<int> { 0 };
        for (var index = 0; index < objects.Length; index++)
        {
            offsets.Add(builder.Length);
            builder.Append(CultureInfo.InvariantCulture, $"{index + 1} 0 obj\n{objects[index]}\nendobj\n");
        }

        var xrefOffset = builder.Length;
        builder.Append(CultureInfo.InvariantCulture, $"xref\n0 {objects.Length + 1}\n");
        builder.AppendLine("0000000000 65535 f ");

        foreach (var offset in offsets.Skip(1))
            builder.Append(CultureInfo.InvariantCulture, $"{offset.ToString("D10", CultureInfo.InvariantCulture)} 00000 n \n");

        builder.Append(CultureInfo.InvariantCulture, $"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");

        return Encoding.ASCII.GetBytes(builder.ToString());
    }

    private static string EscapePdfString(string value)
    {
        var sanitized = SanitizePdfText(value);
        return sanitized
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);
    }

    private static string SanitizePdfText(string value)
    {
        var builder = new StringBuilder(value.Length);

        foreach (var character in value)
        {
            if (character is >= ' ' and <= '~')
                builder.Append(character);
            else if (char.IsWhiteSpace(character))
                builder.Append(' ');
        }

        return builder.ToString();
    }

    private static string ColumnName(int columnNumber)
    {
        var dividend = columnNumber;
        var columnName = string.Empty;

        while (dividend > 0)
        {
            var modulo = (dividend - 1) % 26;
            columnName = Convert.ToChar('A' + modulo) + columnName;
            dividend = (dividend - modulo) / 26;
        }

        return columnName;
    }

    private static string XmlEscape(string value)
        => SecurityElement.Escape(value) ?? string.Empty;

    private static string FormatDate(DateTimeOffset value)
        => value.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string FormatMoney(decimal value)
        => value.ToString("N2", CultureInfo.InvariantCulture);

    private static string FormatPercent(decimal value)
        => value.ToString("N2", CultureInfo.InvariantCulture) + "%";

    private static string FormatDecimal(decimal value)
        => value.ToString("0.##", CultureInfo.InvariantCulture);

    private sealed record CellValue(string Value, bool IsNumber);
}
