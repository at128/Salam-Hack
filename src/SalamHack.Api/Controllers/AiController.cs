using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SalamHack.Application.Features.Analyses.Queries.GetProjectAnalysisDashboard;
using SalamHack.Application.Features.Dashboard.Queries.GetDashboardSummary;
using SalamHack.Application.Features.Invoices.Queries.GetPaymentsSummary;

namespace SalamHack.Api.Controllers;

[Authorize]
public sealed class AiController(
    ISender sender,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<AiController> logger) : ApiController
{
    private const string DefaultEndpoint = "https://api.openai.com/v1/chat/completions";
    private const string DefaultModel = "gpt-4o-mini";
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(60);

    [HttpPost("profit")]
    [EnableRateLimiting("user-write")]
    [ProducesResponseType(typeof(ProfitAiAnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> AnalyzeProfit(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return UnauthorizedResponse();

        var apiKey = configuration["AI:ProjectAnalysis:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "AI API key is not configured." });

        var dashboardResult = await sender.Send(new GetDashboardSummaryQuery(userId, null, 8), ct);
        if (dashboardResult.IsError)
            return Problem(dashboardResult.Errors);

        var paymentsResult = await sender.Send(new GetPaymentsSummaryQuery(userId, null, 10), ct);
        if (paymentsResult.IsError)
            return Problem(paymentsResult.Errors);

        var projectAnalysisResult = await sender.Send(new GetProjectAnalysisDashboardQuery(userId), ct);
        if (projectAnalysisResult.IsError)
            return Problem(projectAnalysisResult.Errors);

        var prompt = BuildPrompt(dashboardResult.Value, paymentsResult.Value, projectAnalysisResult.Value);
        var endpoint = configuration["AI:ProjectAnalysis:Endpoint"] ?? DefaultEndpoint;
        var model = configuration["AI:ProjectAnalysis:Model"] ?? DefaultModel;
        var aiResult = await RequestOpenAiAsync(prompt, apiKey, endpoint, model, forceJson: true, ct);
        if (!aiResult.Success)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = aiResult.Message,
                upstreamStatusCode = aiResult.StatusCode,
                upstreamError = aiResult.UpstreamError,
                model
            });
        }

        return OkResponse(new ProfitAiAnalysisResponse(aiResult.Content!, DateTimeOffset.UtcNow));
    }

    private async Task<AiAnalysisResult> RequestOpenAiAsync(string prompt, string apiKey, string endpoint, string model, bool forceJson, CancellationToken ct)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        object payload = forceJson
            ? new
            {
                model,
                temperature = 0.2,
                response_format = new { type = "json_object" },
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            }
            : new
            {
                model,
                temperature = 0.2,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

        httpRequest.Content = JsonContent.Create(payload);

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
            return AiAnalysisResult.Failure("Could not connect to AI API.", upstreamError: ex.Message);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            logger.LogError(ex, "OpenAI request timed out.");
            return AiAnalysisResult.Failure("AI API request timed out.", statusCode: StatusCodes.Status504GatewayTimeout, upstreamError: ex.Message);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "OpenAI returned non-success status {StatusCode}: {Body}",
                    (int)response.StatusCode,
                    body);

                return AiAnalysisResult.Failure(
                    "AI API request failed.",
                    (int)response.StatusCode,
                    ExtractOpenAiError(body) ?? TrimForGateway(body));
            }
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            if (TryGetContent(document.RootElement, out var content))
                return AiAnalysisResult.Ok(content);

            var finishReason = TryGetFinishReason(document.RootElement);
            var upstreamError = ExtractOpenAiError(document.RootElement)
                ?? (finishReason is null ? null : $"finish_reason: {finishReason}")
                ?? TrimForGateway(body);

            logger.LogWarning("OpenAI returned a successful response without message content: {Body}", body);
            return AiAnalysisResult.Failure(
                "AI returned a successful response, but no analysis text was found.",
                (int)response.StatusCode,
                upstreamError);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "OpenAI returned invalid JSON: {Body}", body);
            return AiAnalysisResult.Failure(
                "AI API returned an invalid JSON response.",
                (int)response.StatusCode,
                TrimForGateway(body));
        }
    }

    private static string BuildPrompt(object dashboard, object payments, object projectAnalysis)
    {
        var input = JsonSerializer.Serialize(new
        {
            dashboard,
            payments,
            projectAnalysis
        });

        var builder = new StringBuilder();
        builder.AppendLine("أنت محلل أرباح ذكي داخل منصة مالي للمستقلين.");
        builder.AppendLine("حلل البيانات الفعلية التالية وكأن صاحب الحساب مستقل يدير خدمات ومشاريع وفواتير، وليس شركة.");
        builder.AppendLine("أرجع الرد باللغة العربية فقط وبصياغة موجهة للمستقل مباشرة.");
        builder.AppendLine("استخدم عبارات مثل: عملك المستقل، دخلك، مشاريعك، عملاؤك، فواتيرك. لا تستخدم كلمة الشركة أو المنشأة أو المؤسسة.");
        builder.AppendLine("لا تذكر أنك نموذج ذكاء اصطناعي، ولا تضف Markdown.");
        builder.AppendLine("أرجع JSON صالحا فقط بهذا الشكل:");
        builder.AppendLine("""
{
  "executiveSummary": "ملخص عربي قصير من جملتين",
  "recommendations": [
    { "title": "عنوان قصير", "description": "توصية عملية مبنية على الأرقام", "priority": "high" }
  ],
  "alerts": [
    { "title": "تنبيه قصير", "description": "سبب التنبيه", "actionLabel": "إجراء قصير", "severity": "warning" }
  ],
  "opportunities": [
    { "title": "فرصة نمو", "description": "خطوة واضحة لزيادة الربح", "impact": "high" }
  ]
}
""");
        builder.AppendLine("القيم المقبولة للأولوية أو التأثير: high, medium, low.");
        builder.AppendLine("القيم المقبولة للتنبيه: critical, warning, success, info.");
        builder.AppendLine("اكتب 3 توصيات مالية، و3 تنبيهات ذكية، و2 فرص نمو. استخدم ريال سعودي عند ذكر المبالغ، واجعل النص مناسبا للوحة تحكم مستقل.");
        builder.AppendLine("البيانات:");
        builder.AppendLine(input);

        return builder.ToString();
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
            return ExtractOpenAiError(document.RootElement);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? ExtractOpenAiError(JsonElement root)
    {
        if (!root.TryGetProperty("error", out var error))
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

        return null;
    }

    private static string? TryGetFinishReason(JsonElement root)
    {
        if (!root.TryGetProperty("choices", out var choices) ||
            choices.ValueKind != JsonValueKind.Array ||
            choices.GetArrayLength() == 0)
            return null;

        var firstChoice = choices[0];
        return firstChoice.TryGetProperty("finish_reason", out var finishReason) && finishReason.ValueKind == JsonValueKind.String
            ? finishReason.GetString()
            : null;
    }

    private static string TrimForGateway(string value)
    {
        const int maxLength = 600;
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}

public sealed record ProfitAiAnalysisResponse(string Content, DateTimeOffset GeneratedAt);

sealed record AiAnalysisResult(
    bool Success,
    string? Content,
    string Message,
    int? StatusCode = null,
    string? UpstreamError = null)
{
    public static AiAnalysisResult Ok(string content) =>
        new(true, content, "AI analysis completed.");

    public static AiAnalysisResult Failure(string message, int? statusCode = null, string? upstreamError = null) =>
        new(false, null, message, statusCode, upstreamError);
}
