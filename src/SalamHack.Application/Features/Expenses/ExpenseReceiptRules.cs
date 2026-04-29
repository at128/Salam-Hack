namespace SalamHack.Application.Features.Expenses;

public static class ExpenseReceiptRules
{
    public const int MaxFileSizeBytes = 10 * 1024 * 1024;

    public static readonly IReadOnlySet<string> AllowedContentTypes =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "image/jpeg",
            "image/png",
            "image/webp"
        };

    public static bool IsAllowedContentType(string? contentType)
        => !string.IsNullOrWhiteSpace(contentType) &&
           AllowedContentTypes.Contains(contentType.Trim());
}
