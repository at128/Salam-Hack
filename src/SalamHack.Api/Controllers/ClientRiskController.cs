using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SalamHack.Api.Controllers;

public sealed class ClientRiskController(IHttpClientFactory httpClientFactory, IConfiguration configuration) : ApiController
{
    private const string NvidiaApiUrl = "https://integrate.api.nvidia.com/v1/chat/completions";
    private const string GemmaModel = "google/gemma-4-31b-it";

    [EnableRateLimiting("public-read")]
    [HttpPost("analyze")]
    [ProducesResponseType(typeof(ClientRiskAnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> Analyze([FromBody] ClientRiskAnalysisRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest(new { message = "Prompt is required." });

        var apiKey = configuration["Nvidia:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Nvidia API key is not configured.");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, NvidiaApiUrl);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpRequest.Content = JsonContent.Create(new
        {
            model = GemmaModel,
            messages = new[] { new { role = "user", content = request.Prompt } },
            max_tokens = 16384,
            temperature = 0.2,
            top_p = 0.95,
            stream = false,
            chat_template_kwargs = new { enable_thinking = true }
        });

        var client = httpClientFactory.CreateClient();
        using var response = await client.SendAsync(httpRequest, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Gemma API request failed." });

        using var document = JsonDocument.Parse(body);
        if (!TryGetGemmaContent(document.RootElement, out var content))
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Gemma did not return analysis text." });

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
