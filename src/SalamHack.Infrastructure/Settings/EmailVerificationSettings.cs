namespace SalamHack.Infrastructure.Settings;

public sealed class EmailVerificationSettings
{
    public const string SectionName = "EmailVerification";

    public bool Enabled { get; init; } = true;

    public int OtpLength { get; init; } = 6;

    public int ExpiryMinutes { get; init; } = 10;

    public int MaxAttempts { get; init; } = 5;
}

public sealed class MailSettings
{
    public const string SectionName = "MailSettings";

    public string Host { get; init; } = string.Empty;

    public int Port { get; init; } = 587;

    public bool EnableSsl { get; init; } = true;

    public string User { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string FromEmail { get; init; } = string.Empty;

    public string FromName { get; init; } = "SalamHack";
}
