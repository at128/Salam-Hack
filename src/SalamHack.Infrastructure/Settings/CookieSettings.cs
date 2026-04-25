namespace SalamHack.Infrastructure.Settings;

public sealed class CookieSettings
{
    public string RefreshTokenCookieName { get; set; } = "app_rt";
    public int RefreshTokenExpiryDays { get; set; } = 7;
    public bool SecureOnly { get; set; } = true;
    public string SameSite { get; set; } = "Strict";
    public string Path { get; set; } = "/api";
}
