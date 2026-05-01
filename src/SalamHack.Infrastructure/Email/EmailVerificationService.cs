using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Common.Results;
using SalamHack.Infrastructure.Settings;

namespace SalamHack.Infrastructure.Email;

public sealed class EmailVerificationService(
    IMemoryCache cache,
    IOptions<EmailVerificationSettings> emailVerificationOptions,
    IOptions<MailSettings> mailOptions,
    ILogger<EmailVerificationService> logger,
    TimeProvider timeProvider) : IEmailVerificationService
{
    private const string RegistrationCachePrefix = "registration-email-otp:";
    private const string PasswordResetCachePrefix = "password-reset-email-otp:";
    private readonly EmailVerificationSettings _settings = emailVerificationOptions.Value;
    private readonly MailSettings _mailSettings = mailOptions.Value;

    public async Task<Result<EmailVerificationChallengeResult>> SendRegistrationOtpAsync(
        string email,
        CancellationToken ct = default)
    {
        if (!IsConfigured())
            return ApplicationErrors.Auth.EmailVerificationNotConfigured;

        var normalizedEmail = NormalizeEmail(email);
        var otp = GenerateOtp(_settings.OtpLength);
        var expiresAt = timeProvider.GetUtcNow().AddMinutes(_settings.ExpiryMinutes);

        cache.Set(
            GetCacheKey(RegistrationCachePrefix, normalizedEmail),
            CreateChallenge(normalizedEmail, otp, expiresAt),
            expiresAt);

        try
        {
            await SendOtpEmailAsync(
                normalizedEmail,
                "Your SalamHack verification code",
                BuildVerificationEmailBody(otp),
                ct);
        }
        catch (Exception ex) when (ex is SmtpException or InvalidOperationException)
        {
            cache.Remove(GetCacheKey(RegistrationCachePrefix, normalizedEmail));
            logger.LogError(ex, "Failed to send registration OTP to {Email}.", normalizedEmail);
            return ApplicationErrors.Auth.EmailVerificationSendFailed;
        }

        return new EmailVerificationChallengeResult(normalizedEmail, _settings.ExpiryMinutes);
    }

    public Task<Result<Success>> VerifyRegistrationOtpAsync(
        string email,
        string otp,
        CancellationToken ct = default)
    {
        return VerifyOtpAsync(RegistrationCachePrefix, email, otp);
    }

    public async Task<Result<EmailVerificationChallengeResult>> SendPasswordResetOtpAsync(
        string email,
        CancellationToken ct = default)
    {
        if (!IsConfigured())
            return ApplicationErrors.Auth.EmailVerificationNotConfigured;

        var normalizedEmail = NormalizeEmail(email);
        var otp = GenerateOtp(_settings.OtpLength);
        var expiresAt = timeProvider.GetUtcNow().AddMinutes(_settings.ExpiryMinutes);

        cache.Set(
            GetCacheKey(PasswordResetCachePrefix, normalizedEmail),
            CreateChallenge(normalizedEmail, otp, expiresAt),
            expiresAt);

        try
        {
            await SendOtpEmailAsync(
                normalizedEmail,
                "Your SalamHack password reset code",
                BuildPasswordResetEmailBody(otp),
                ct);
        }
        catch (Exception ex) when (ex is SmtpException or InvalidOperationException)
        {
            cache.Remove(GetCacheKey(PasswordResetCachePrefix, normalizedEmail));
            logger.LogError(ex, "Failed to send password reset OTP to {Email}.", normalizedEmail);
            return ApplicationErrors.Auth.EmailVerificationSendFailed;
        }

        return new EmailVerificationChallengeResult(normalizedEmail, _settings.ExpiryMinutes);
    }

    public Task<Result<Success>> VerifyPasswordResetOtpAsync(
        string email,
        string otp,
        CancellationToken ct = default)
    {
        return VerifyOtpAsync(PasswordResetCachePrefix, email, otp);
    }

    private Task<Result<Success>> VerifyOtpAsync(string cachePrefix, string email, string otp)
    {
        var normalizedEmail = NormalizeEmail(email);
        var cacheKey = GetCacheKey(cachePrefix, normalizedEmail);

        if (!cache.TryGetValue<EmailOtpChallenge>(cacheKey, out var challenge) || challenge is null)
            return Task.FromResult<Result<Success>>(ApplicationErrors.Auth.InvalidEmailVerificationCode);

        if (challenge.ExpiresAt <= timeProvider.GetUtcNow())
        {
            cache.Remove(cacheKey);
            return Task.FromResult<Result<Success>>(ApplicationErrors.Auth.InvalidEmailVerificationCode);
        }

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(challenge.OtpHash),
                Encoding.UTF8.GetBytes(HashOtp(normalizedEmail, otp))))
        {
            challenge = challenge with { Attempts = challenge.Attempts + 1 };
            if (challenge.Attempts >= _settings.MaxAttempts)
                cache.Remove(cacheKey);
            else
                cache.Set(cacheKey, challenge, challenge.ExpiresAt);

            return Task.FromResult<Result<Success>>(ApplicationErrors.Auth.InvalidEmailVerificationCode);
        }

        cache.Remove(cacheKey);
        return Task.FromResult<Result<Success>>(Result.Success);
    }

    private async Task SendOtpEmailAsync(
        string email,
        string subject,
        string body,
        CancellationToken ct)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(_mailSettings.FromEmail, _mailSettings.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        message.To.Add(email);

        using var client = new SmtpClient(_mailSettings.Host, _mailSettings.Port)
        {
            EnableSsl = _mailSettings.EnableSsl,
            Credentials = new NetworkCredential(_mailSettings.User, _mailSettings.Password)
        };

        await client.SendMailAsync(message, ct);
    }

    private bool IsConfigured()
        => _settings.Enabled
           && !string.IsNullOrWhiteSpace(_mailSettings.Host)
           && !string.IsNullOrWhiteSpace(_mailSettings.User)
           && !string.IsNullOrWhiteSpace(_mailSettings.Password)
           && !string.IsNullOrWhiteSpace(_mailSettings.FromEmail);

    private static string BuildVerificationEmailBody(string otp)
        => $"""
           Your SalamHack verification code is: {otp}

           This code expires soon. If you did not request it, you can ignore this email.
           """;

    private static string BuildPasswordResetEmailBody(string otp)
        => $"""
           Your SalamHack password reset code is: {otp}

           This code expires soon. If you did not request a password reset, you can ignore this email.
           """;

    private static string GenerateOtp(int length)
    {
        var min = (int)Math.Pow(10, length - 1);
        var max = (int)Math.Pow(10, length);
        return RandomNumberGenerator.GetInt32(min, max).ToString(CultureInfo.InvariantCulture);
    }

    private static string HashOtp(string normalizedEmail, string otp)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{normalizedEmail}:{otp}"));
        return Convert.ToHexString(bytes);
    }

    private static string NormalizeEmail(string email)
        => email.Trim().ToUpperInvariant();

    private static EmailOtpChallenge CreateChallenge(string normalizedEmail, string otp, DateTimeOffset expiresAt)
        => new(HashOtp(normalizedEmail, otp), expiresAt, 0);

    private static string GetCacheKey(string cachePrefix, string normalizedEmail)
        => $"{cachePrefix}{normalizedEmail}";

    private sealed record EmailOtpChallenge(
        string OtpHash,
        DateTimeOffset ExpiresAt,
        int Attempts);
}
