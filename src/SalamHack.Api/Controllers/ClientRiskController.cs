using System.Net.Http.Headers;
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
    private const string DefaultEndpoint = "https://api.openai.com/v1/chat/completions";
    private const string DefaultModel = "gpt-4o-mini";
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

        var apiKey = configuration["AI:ProjectAnalysis:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "AI API key is not configured." });

        var endpoint = configuration["AI:ProjectAnalysis:Endpoint"] ?? DefaultEndpoint;
        var model = configuration["AI:ProjectAnalysis:Model"] ?? DefaultModel;
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Content = JsonContent.Create(new
        {
            model,
            temperature = 0.2,
            messages = new[]
            {
                new { role = "user", content = request.Prompt }
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
            logger.LogError(ex, "OpenAI request failed before receiving a response.");
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Could not connect to AI API." });
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            logger.LogError(ex, "OpenAI request timed out.");
            return StatusCode(StatusCodes.Status504GatewayTimeout, new { message = "AI API request timed out." });
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "OpenAI returned non-success status {StatusCode}: {Body}",
                    (int)response.StatusCode,
                    body);

                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    message = "AI API request failed.",
                    upstreamStatusCode = (int)response.StatusCode,
                    upstreamError = ExtractOpenAiError(body) ?? TrimForGateway(body),
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
                    message = "AI did not return analysis text.",
                    upstreamError = TrimForGateway(body),
                    model
                });
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "OpenAI returned invalid JSON: {Body}", body);
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "AI API returned an invalid response." });
        }

        return Ok(new ClientRiskAnalysisResponse(content));
    }

    private static bool TryGetContent(JsonElement root, out string content)
    {
        content = string.Empty;

        if (!root.TryGetProperty("choices", out var choices) ||
            choices.ValueKind != JsonValueKind.Array ||
            choices.GetArrayLength() == 0)
            return false;

        var firstChoice = choices[0];
        if (!firstChoice.TryGetProperty("message", out var message) ||
            message.ValueKind != JsonValueKind.Object ||
            !message.TryGetProperty("content", out var contentElement) ||
            contentElement.ValueKind != JsonValueKind.String)
            return false;

        content = contentElement.GetString() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(content);
    }

    private static string? ExtractOpenAiError(string body)
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
