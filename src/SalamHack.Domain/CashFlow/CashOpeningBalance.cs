namespace SalamHack.Domain.CashFlow;

public sealed record CashOpeningBalance(decimal Amount, DateTimeOffset? EffectiveAtUtc)
{
    public static CashOpeningBalance Zero { get; } = new(0, null);

    public static CashOpeningBalance Create(decimal amount, DateTimeOffset? effectiveAtUtc = null)
        => amount == 0 && effectiveAtUtc is null
            ? Zero
            : new CashOpeningBalance(amount, effectiveAtUtc);
}
