using System.Text;
using System.Text.Json;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace SalamHack.Infrastructure.Storage;

public sealed class FileSystemExpenseReceiptStorage(
    IOptions<ExpenseReceiptStorageSettings> options,
    TimeProvider timeProvider) : IExpenseReceiptStorage
{
    private const string ContentFileName = "receipt.bin";
    private const string MetadataFileName = "metadata.json";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<ExpenseReceiptStorageFile> SaveAsync(
        Guid userId,
        Guid expenseId,
        string fileName,
        string contentType,
        byte[] content,
        CancellationToken cancellationToken = default)
    {
        var directory = GetReceiptDirectory(userId, expenseId);
        Directory.CreateDirectory(directory);

        var uploadedAtUtc = timeProvider.GetUtcNow();
        var safeFileName = Path.GetFileName(fileName.Trim());
        var metadata = new StoredReceiptMetadata(
            string.IsNullOrWhiteSpace(safeFileName) ? "receipt" : safeFileName,
            contentType.Trim(),
            content.LongLength,
            uploadedAtUtc);

        var contentPath = Path.Combine(directory, ContentFileName);
        var metadataPath = Path.Combine(directory, MetadataFileName);

        await File.WriteAllBytesAsync(contentPath, content, cancellationToken);
        await File.WriteAllTextAsync(
            metadataPath,
            JsonSerializer.Serialize(metadata, JsonOptions),
            Encoding.UTF8,
            cancellationToken);

        return new ExpenseReceiptStorageFile(
            expenseId,
            metadata.FileName,
            metadata.ContentType,
            metadata.SizeInBytes,
            metadata.UploadedAtUtc,
            content);
    }

    public async Task<ExpenseReceiptStorageFile?> GetAsync(
        Guid userId,
        Guid expenseId,
        CancellationToken cancellationToken = default)
    {
        var directory = GetReceiptDirectory(userId, expenseId);
        var contentPath = Path.Combine(directory, ContentFileName);
        var metadataPath = Path.Combine(directory, MetadataFileName);

        if (!File.Exists(contentPath) || !File.Exists(metadataPath))
            return null;

        var metadataJson = await File.ReadAllTextAsync(metadataPath, Encoding.UTF8, cancellationToken);
        var metadata = JsonSerializer.Deserialize<StoredReceiptMetadata>(metadataJson, JsonOptions);
        if (metadata is null)
            return null;

        var content = await File.ReadAllBytesAsync(contentPath, cancellationToken);

        return new ExpenseReceiptStorageFile(
            expenseId,
            metadata.FileName,
            metadata.ContentType,
            metadata.SizeInBytes,
            metadata.UploadedAtUtc,
            content);
    }

    public Task<bool> DeleteAsync(
        Guid userId,
        Guid expenseId,
        CancellationToken cancellationToken = default)
    {
        var directory = GetReceiptDirectory(userId, expenseId);
        if (!Directory.Exists(directory))
            return Task.FromResult(false);

        Directory.Delete(directory, recursive: true);
        return Task.FromResult(true);
    }

    private string GetReceiptDirectory(Guid userId, Guid expenseId)
        => Path.Combine(
            GetRootPath(),
            userId.ToString("N"),
            expenseId.ToString("N"));

    private string GetRootPath()
    {
        var configuredPath = string.IsNullOrWhiteSpace(options.Value.RootPath)
            ? "App_Data/expense-receipts"
            : options.Value.RootPath;

        return Path.GetFullPath(
            Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(AppContext.BaseDirectory, configuredPath));
    }

    private sealed record StoredReceiptMetadata(
        string FileName,
        string ContentType,
        long SizeInBytes,
        DateTimeOffset UploadedAtUtc);
}
