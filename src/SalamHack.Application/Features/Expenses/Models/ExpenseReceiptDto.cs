namespace SalamHack.Application.Features.Expenses.Models;

public sealed record ExpenseReceiptDto(
    Guid ExpenseId,
    string FileName,
    string ContentType,
    long SizeInBytes,
    DateTimeOffset UploadedAtUtc);

public sealed record ExpenseReceiptFileDto(
    Guid ExpenseId,
    string FileName,
    string ContentType,
    long SizeInBytes,
    DateTimeOffset UploadedAtUtc,
    byte[] Content);
