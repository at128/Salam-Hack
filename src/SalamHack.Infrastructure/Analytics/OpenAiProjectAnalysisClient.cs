using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Analyses.Models;
using SalamHack.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SalamHack.Infrastructure.Analytics;

public sealed class OpenAiProjectAnalysisClient(
    IOptions<ProjectAnalysisAiSettings> options,
    ILogger<OpenAiProjectAnalysisClient> logger)
    : IProjectAnalysisAiClient
{
    private static readonly HttpClient HttpClient = new();
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<ProjectAiAnalysisClientResult?> AnalyzeAsync(
        ProjectAiAnalysisPrompt prompt,
        CancellationToken ct = default)
    {
        ProjectAnalysisAiSettings settings;
        try
        {
            settings = options.Value;
        }
        catch (OptionsValidationException ex)
        {
            logger.LogWarning(ex, "Project AI analysis configuration is invalid; falling back to rule-based analysis.");
            return null;
        }

        if (!settings.Enabled)
            return null;

        if (string.IsNullOrWhiteSpace(settings.ApiKey) ||
            string.IsNullOrWhiteSpace(settings.Endpoint) ||
            string.IsNullOrWhiteSpace(settings.Model))
        {
            logger.LogWarning("Project AI analysis is enabled but AI:ProjectAnalysis configuration is incomplete.");
            return null;
        }

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(settings.TimeoutSeconds, 5, 120)));

            using var request = new HttpRequestMessage(HttpMethod.Post, settings.Endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(BuildRequestBody(prompt, settings), JsonOptions),
                Encoding.UTF8,
                "application/json");

            using var response = await HttpClient.SendAsync(request, timeoutCts.Token);
            var responseBody = await response.Content.ReadAsStringAsync(timeoutCts.Token);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Project AI analysis provider returned status code {StatusCode}.",
                    (int)response.StatusCode);
                return null;
            }

            var messageContent = ExtractAssistantContent(responseBody);
            var json = ExtractJsonObject(messageContent);
            var analysis = JsonSerializer.Deserialize<ProjectAiAnalysisDto>(json, JsonOptions);
            if (analysis is null)
                return null;

            return new ProjectAiAnalysisClientResult(
                analysis,
                settings.Provider,
                settings.Model);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning("Project AI analysis provider timed out.");
            return null;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or InvalidOperationException)
        {
            logger.LogWarning(ex, "Project AI analysis provider failed; falling back to rule-based analysis.");
            return null;
        }
    }

    private static object BuildRequestBody(
        ProjectAiAnalysisPrompt prompt,
        ProjectAnalysisAiSettings settings)
        => new
        {
            model = settings.Model,
            temperature = settings.Temperature,
            response_format = new
            {
                type = "json_object"
            },
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = prompt.SystemPrompt
                },
                new
                {
                    role = "user",
                    content = prompt.UserPrompt
                }
            }
        };

    private static string ExtractAssistantContent(string responseBody)
    {
        using var document = JsonDocument.Parse(responseBody);
        var root = document.RootElement;
        var choices = root.GetProperty("choices");
        if (choices.GetArrayLength() == 0)
            throw new InvalidOperationException("AI provider response did not include choices.");

        return choices[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? throw new InvalidOperationException("AI provider response content was empty.");
    }

    private static string ExtractJsonObject(string content)
    {
        var trimmed = content.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstLineEnd = trimmed.IndexOf('\n');
            if (firstLineEnd >= 0)
                trimmed = trimmed[(firstLineEnd + 1)..];

            var fenceIndex = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (fenceIndex >= 0)
                trimmed = trimmed[..fenceIndex];
        }

        var firstBrace = trimmed.IndexOf('{');
        var lastBrace = trimmed.LastIndexOf('}');
        if (firstBrace < 0 || lastBrace <= firstBrace)
            throw new JsonException("AI provider response did not contain a JSON object.");

        return trimmed[firstBrace..(lastBrace + 1)];
    }
}
