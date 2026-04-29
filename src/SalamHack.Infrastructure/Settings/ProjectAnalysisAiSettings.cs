namespace SalamHack.Infrastructure.Settings;

public sealed class ProjectAnalysisAiSettings
{
    public const string SectionName = "AI:ProjectAnalysis";

    public bool Enabled { get; init; }
    public string Provider { get; init; } = "OpenAI";
    public string Endpoint { get; init; } = "https://api.openai.com/v1/chat/completions";
    public string Model { get; init; } = "gpt-4o-mini";
    public string? ApiKey { get; init; }
    public decimal Temperature { get; init; } = 0.2m;
    public int TimeoutSeconds { get; init; } = 30;
}
