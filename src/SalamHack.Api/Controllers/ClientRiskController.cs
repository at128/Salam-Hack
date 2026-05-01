using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SalamHack.Api.Controllers;

public sealed class ClientRiskController(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<ClientRiskController> logger) : ApiController
{
    private const string GeminiApiBaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";
    private const string DefaultGeminiModel = "gemini-1.5-flash";
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(60);

    [EnableRateLimiting("public-read")]
    [HttpPost("analyze")]
    [ProducesResponseType(typeof(ClientRiskAnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> Analyze([FromBody] ClientRiskAnalysisRequest? request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request?.Prompt))
            return BadRequest(new { message = "Prompt is required." });

        var apiKey = configuration["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Gemini API key is not configured." });

        var model = configuration["Gemini:Model"] ?? DefaultGeminiModel;
        var requestUrl = $"{GeminiApiBaseUrl}/{model}:generateContent?key={Uri.EscapeDataString(apiKey)}";
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);
        httpRequest.Content = JsonContent.Create(new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = request.Prompt } }
                }
            },
            generationConfig = new
            {
                temperature = 0.2
            }
        });

        HttpResponseMessage response;
        string body;

        try
        {
            var client = httpClientFactory.CreateClient();
            client.Timeout = RequestTimeout;
            response = await client.SendAsync(httpRequest, ct);
            body = await response.Content.ReadAsStringAsync(ct);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Gemini request failed before receiving a response.");
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Could not connect to Gemini API." });
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            logger.LogError(ex, "Gemini request timed out.");
            return StatusCode(StatusCodes.Status504GatewayTimeout, new { message = "Gemini API request timed out." });
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Gemini returned non-success status {StatusCode}: {Body}",
                    (int)response.StatusCode,
                    body);

                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    message = "Gemini API request failed.",
                    upstreamStatusCode = (int)response.StatusCode,
                    upstreamError = ExtractGeminiError(body) ?? TrimForGateway(body),
                    model
                });
            }
        }

        string content;
        try
        {
            using var document = JsonDocument.Parse(body);
            if (!TryGetContent(document.RootElement, out content))
                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    message = "Gemini did not return analysis text.",
                    upstreamError = TrimForGateway(body),
                    model
                });
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Gemini returned invalid JSON: {Body}", body);
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Gemini API returned an invalid response." });
        }

        return Ok(new ClientRiskAnalysisResponse(content));
    }

    private static bool TryGetContent(JsonElement root, out string content)
    {
        content = string.Empty;

        if (!root.TryGetProperty("candidates", out var candidates) ||
            candidates.ValueKind != JsonValueKind.Array ||
            candidates.GetArrayLength() == 0)
            return false;

        var firstCandidate = candidates[0];
        if (!firstCandidate.TryGetProperty("content", out var contentElement) ||
            contentElement.ValueKind != JsonValueKind.Object ||
            !contentElement.TryGetProperty("parts", out var parts) ||
            parts.ValueKind != JsonValueKind.Array)
            return false;

        var builder = new StringBuilder();
        foreach (var part in parts.EnumerateArray())
        {
            if (part.ValueKind == JsonValueKind.Object &&
                part.TryGetProperty("text", out var textElement) &&
                textElement.ValueKind == JsonValueKind.String)
            {
                builder.Append(textElement.GetString());
            }
        }

        content = builder.ToString();
        return !string.IsNullOrWhiteSpace(content);
    }

    private static string? ExtractGeminiError(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            if (!document.RootElement.TryGetProperty("error", out var error))
                return null;

            if (error.ValueKind == JsonValueKind.String)
                return error.GetString();

            if (error.ValueKind == JsonValueKind.Object)
            {
                if (error.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
                    return message.GetString();

                if (error.TryGetProperty("code", out var code))
                    return code.ToString();
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    private static string TrimForGateway(string value)
    {
        const int maxLength = 600;
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}

public sealed record ClientRiskAnalysisRequest(string Prompt);

public sealed record ClientRiskAnalysisResponse(string Content);
