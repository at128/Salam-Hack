using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SalamHack.Api.Controllers;

public sealed class ClientRiskController(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<ClientRiskController> logger) : ApiController
{
    private const string OpenRouterApiUrl = "https://openrouter.ai/api/v1/chat/completions";
    private const string OpenRouterModel = "openai/gpt-4o-mini";
    private const string SiteUrl = "https://salamhack.com";
    private const string SiteTitle = "SalamHack";
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

        var apiKey = configuration["OpenRouter:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "OpenRouter API key is not configured." });

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, OpenRouterApiUrl);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Headers.Add("HTTP-Referer", SiteUrl);
        httpRequest.Headers.Add("X-Title", SiteTitle);
        httpRequest.Content = JsonContent.Create(new
        {
            model = OpenRouterModel,
            messages = new[] { new { role = "user", content = request.Prompt } }
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
            logger.LogError(ex, "OpenRouter request failed before receiving a response.");
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Could not connect to OpenRouter API." });
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            logger.LogError(ex, "OpenRouter request timed out.");
            return StatusCode(StatusCodes.Status504GatewayTimeout, new { message = "OpenRouter API request timed out." });
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "OpenRouter returned non-success status {StatusCode}: {Body}",
                    (int)response.StatusCode,
                    body);

                return StatusCode(StatusCodes.Status502BadGateway, new { message = "OpenRouter API request failed." });
            }
        }

        string content;
        try
        {
            using var document = JsonDocument.Parse(body);
            if (!TryGetContent(document.RootElement, out content))
                return StatusCode(StatusCodes.Status502BadGateway, new { message = "OpenRouter did not return analysis text." });
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "OpenRouter returned invalid JSON: {Body}", body);
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "OpenRouter API returned an invalid response." });
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
            !message.TryGetProperty("content", out var contentElement))
            return false;

        content = contentElement.GetString() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(content);
    }
}

public sealed record ClientRiskAnalysisRequest(string Prompt);

public sealed record ClientRiskAnalysisResponse(string Content);
