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
    private const string NvidiaApiUrl = "https://integrate.api.nvidia.com/v1/chat/completions";
    private const string GemmaModel = "google/gemma-4-31b-it";
    private static readonly TimeSpan GemmaRequestTimeout = TimeSpan.FromSeconds(180);

    [EnableRateLimiting("public-read")]
    [HttpPost("analyze")]
    [ProducesResponseType(typeof(ClientRiskAnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> Analyze([FromBody] ClientRiskAnalysisRequest? request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request?.Prompt))
            return BadRequest(new { message = "Prompt is required." });

        var apiKey = configuration["Nvidia:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Nvidia API key is not configured." });

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, NvidiaApiUrl);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpRequest.Content = JsonContent.Create(new
        {
            model = GemmaModel,
            messages = new[] { new { role = "user", content = request.Prompt } },
            max_tokens = 2048,
            temperature = 0.2,
            top_p = 0.95,
            stream = false,
            chat_template_kwargs = new { enable_thinking = false }
        });

        HttpResponseMessage response;
        string body;

        try
        {
            var client = httpClientFactory.CreateClient();
            client.Timeout = GemmaRequestTimeout;
            response = await client.SendAsync(httpRequest, ct);
            body = await response.Content.ReadAsStringAsync(ct);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "NVIDIA Gemma request failed before receiving a response.");
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Could not connect to Gemma API." });
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            logger.LogError(ex, "NVIDIA Gemma request timed out.");
            return StatusCode(StatusCodes.Status504GatewayTimeout, new { message = "Gemma API request timed out." });
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "NVIDIA Gemma returned non-success status {StatusCode}: {Body}",
                    (int)response.StatusCode,
                    body);

                return StatusCode(StatusCodes.Status502BadGateway, new { message = "Gemma API request failed." });
            }
        }

        string content;
        try
        {
            using var document = JsonDocument.Parse(body);
            if (!TryGetGemmaContent(document.RootElement, out content))
                return StatusCode(StatusCodes.Status502BadGateway, new { message = "Gemma did not return analysis text." });
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "NVIDIA Gemma returned invalid JSON: {Body}", body);
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Gemma API returned an invalid response." });
        }

        return Ok(new ClientRiskAnalysisResponse(content));
    }

    private static bool TryGetGemmaContent(JsonElement root, out string content)
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
