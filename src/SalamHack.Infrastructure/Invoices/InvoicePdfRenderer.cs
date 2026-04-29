using System.Globalization;
using System.Text;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Invoices.Models;

namespace SalamHack.Infrastructure.Invoices;

public sealed class InvoicePdfRenderer : IInvoicePdfRenderer
{
    public Task<InvoicePdfFile> RenderAsync(
        InvoiceDto invoice,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        var lines = new List<string>
        {
            $"Invoice {invoice.InvoiceNumber}",
            $"Customer: {invoice.CustomerName}",
            $"Project: {invoice.ProjectName}",
            $"Status: {invoice.Status}",
            $"Issue date: {FormatDate(invoice.IssueDate)}",
            $"Due date: {FormatDate(invoice.DueDate)}",
            $"Currency: {invoice.Currency}",
            string.Empty,
            $"Subtotal: {FormatMoney(invoice.TotalAmount)}",
            $"Tax: {FormatMoney(invoice.TaxAmount)}",
            $"Total: {FormatMoney(invoice.TotalWithTax)}",
            $"Advance: {FormatMoney(invoice.AdvanceAmount)}",
            $"Paid: {FormatMoney(invoice.PaidAmount)}",
            $"Remaining: {FormatMoney(invoice.RemainingAmount)}"
        };

        if (!string.IsNullOrWhiteSpace(invoice.Notes))
        {
            lines.Add(string.Empty);
            lines.Add("Notes:");
            lines.Add(invoice.Notes);
        }

        if (invoice.Payments.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("Payments:");

            foreach (var payment in invoice.Payments.OrderBy(p => p.PaymentDate).Take(20))
            {
                lines.Add(
                    $"{FormatDate(payment.PaymentDate)} - {payment.Method} - {FormatMoney(payment.Amount)} {payment.Currency}");
            }
        }

        var fileName = $"invoice-{SafeFilePart(invoice.InvoiceNumber)}.pdf";

        return Task.FromResult(new InvoicePdfFile(
            fileName,
            "application/pdf",
            BuildSimplePdf(lines)));
    }

    private static byte[] BuildSimplePdf(IReadOnlyList<string> lines)
    {
        var contentBuilder = new StringBuilder();
        contentBuilder.AppendLine("BT");
        contentBuilder.AppendLine("/F1 16 Tf");
        contentBuilder.AppendLine("50 790 Td");
        contentBuilder.AppendLine($"({EscapePdfString(lines.FirstOrDefault() ?? "Invoice")}) Tj");
        contentBuilder.AppendLine("/F1 10 Tf");

        foreach (var line in lines.Skip(1).Take(44))
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

    private static string SafeFilePart(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var safe = new string(value.Select(c => invalidChars.Contains(c) ? '-' : c).ToArray());

        return string.IsNullOrWhiteSpace(safe) ? "invoice" : safe;
    }

    private static string FormatDate(DateTimeOffset value)
        => value.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string FormatMoney(decimal value)
        => value.ToString("N2", CultureInfo.InvariantCulture);
}
