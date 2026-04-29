namespace SalamHack.Application.Common.Interfaces;

public interface IExpenseReceiptStorage
{
    Task<ExpenseReceiptStorageFile> SaveAsync(
        Guid userId,
        Guid expenseId,
        string fileName,
        string contentType,
        Stream content,
        CancellationToken cancellationToken = default);

    Task<ExpenseReceiptStorageFile?> GetAsync(
        Guid userId,
        Guid expenseId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid userId,
        Guid expenseId,
        CancellationToken cancellationToken = default);
}

public sealed record ExpenseReceiptStorageFile(
    Guid ExpenseId,
    string FileName,
    string ContentType,
    long SizeInBytes,
    DateTimeOffset UploadedAtUtc,
    byte[] Content);
