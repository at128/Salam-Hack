namespace SalamHack.Infrastructure.Settings;

public sealed class ExpenseReceiptStorageSettings
{
    public const string SectionName = "ExpenseReceipts";

    public string RootPath { get; set; } = "App_Data/expense-receipts";
}
