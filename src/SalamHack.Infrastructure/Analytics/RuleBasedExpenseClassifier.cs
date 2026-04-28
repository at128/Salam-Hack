using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Expenses;

namespace SalamHack.Infrastructure.Analytics;

public sealed class RuleBasedExpenseClassifier : IExpenseClassifier
{
    private static readonly string[] SubscriptionTerms =
    [
        "subscription",
        "saas",
        "figma",
        "adobe",
        "notion",
        "workspace",
        "\u0627\u0634\u062a\u0631\u0627\u0643"
    ];

    private static readonly string[] ToolTerms =
    [
        "tool",
        "tools",
        "server",
        "hosting",
        "font",
        "\u0627\u062f\u0627\u0629",
        "\u0627\u062f\u0648\u0627\u062a",
        "\u0633\u064a\u0631\u0641\u0631",
        "\u0627\u0633\u062a\u0636\u0627\u0641\u0629",
        "\u062e\u0637\u0648\u0637"
    ];

    private static readonly string[] MarketingTerms =
    [
        "marketing",
        "ads",
        "ad ",
        "instagram",
        "twitter",
        "meta",
        "\u0627\u0639\u0644\u0627\u0646",
        "\u0627\u0639\u0644\u0627\u0646\u0627\u062a",
        "\u062a\u0633\u0648\u064a\u0642"
    ];

    private static readonly string[] ProfessionalDevelopmentTerms =
    [
        "course",
        "training",
        "learning",
        "\u0643\u0648\u0631\u0633",
        "\u062a\u062f\u0631\u064a\u0628",
        "\u062a\u0639\u0644\u0645",
        "\u062a\u0637\u0648\u064a\u0631 \u0645\u0647\u0646\u064a"
    ];

    private static readonly string[] CommunicationTerms =
    [
        "phone",
        "internet",
        "communication",
        "\u0627\u062a\u0635\u0627\u0644",
        "\u0627\u062a\u0635\u0627\u0644\u0627\u062a",
        "\u0627\u0646\u062a\u0631\u0646\u062a"
    ];

    public Task<ExpenseCategory> ClassifyAsync(
        string description,
        CancellationToken cancellationToken = default)
    {
        var text = description.Trim().ToLowerInvariant();

        var category = text switch
        {
            _ when ContainsAny(text, SubscriptionTerms)
                => ExpenseCategory.Subscriptions,
            _ when ContainsAny(text, ToolTerms)
                => ExpenseCategory.Tools,
            _ when ContainsAny(text, MarketingTerms)
                => ExpenseCategory.Marketing,
            _ when ContainsAny(text, ProfessionalDevelopmentTerms)
                => ExpenseCategory.ProfessionalDevelopment,
            _ when ContainsAny(text, CommunicationTerms)
                => ExpenseCategory.Communications,
            _ => ExpenseCategory.Other
        };

        return Task.FromResult(category);
    }

    private static bool ContainsAny(string text, params string[] terms)
        => terms.Any(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
}
