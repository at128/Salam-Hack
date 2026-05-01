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
    private const string GeminiApiBaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";
    private const string DefaultGeminiModel = "gemini-1.5-flash";
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

        var apiKey = configuration["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Gemini API key is not configured." });

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
        var model = configuration["Gemini:Model"] ?? DefaultGeminiModel;
        var geminiResult = await RequestGeminiAsync(prompt, apiKey, model, forceJson: true, ct);
        if (!geminiResult.Success)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = geminiResult.Message,
                upstreamStatusCode = geminiResult.StatusCode,
                upstreamError = geminiResult.UpstreamError,
                model
            });
        }

        return OkResponse(new ProfitAiAnalysisResponse(geminiResult.Content!, DateTimeOffset.UtcNow));
    }

    private async Task<GeminiAnalysisResult> RequestGeminiAsync(string prompt, string apiKey, string model, bool forceJson, CancellationToken ct)
    {
        var requestUrl = $"{GeminiApiBaseUrl}/{model}:generateContent?key={Uri.EscapeDataString(apiKey)}";
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);
        httpRequest.Content = JsonContent.Create(new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = prompt } }
                }
            },
            generationConfig = new
            {
                temperature = 0.2,
                responseMimeType = forceJson ? "application/json" : null
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
            return GeminiAnalysisResult.Failure("Could not connect to Gemini API.", upstreamError: ex.Message);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            logger.LogError(ex, "Gemini request timed out.");
            return GeminiAnalysisResult.Failure("Gemini API request timed out.", statusCode: StatusCodes.Status504GatewayTimeout, upstreamError: ex.Message);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Gemini returned non-success status {StatusCode}: {Body}",
                    (int)response.StatusCode,
                    body);

                return GeminiAnalysisResult.Failure(
                    "Gemini API request failed.",
                    (int)response.StatusCode,
                    ExtractGeminiError(body) ?? TrimForGateway(body));
            }
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            if (TryGetContent(document.RootElement, out var content))
                return GeminiAnalysisResult.Ok(content);

            var finishReason = TryGetFinishReason(document.RootElement);
            var upstreamError = ExtractGeminiError(document.RootElement)
                ?? (finishReason is null ? null : $"finish_reason: {finishReason}")
                ?? TrimForGateway(body);

            logger.LogWarning("Gemini returned a successful response without message content: {Body}", body);
            return GeminiAnalysisResult.Failure(
                "Gemini returned a successful response, but no analysis text was found.",
                (int)response.StatusCode,
                upstreamError);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Gemini returned invalid JSON: {Body}", body);
            return GeminiAnalysisResult.Failure(
                "Gemini API returned an invalid JSON response.",
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

        if (!root.TryGetProperty("candidates", out var candidates) ||
            candidates.ValueKind != JsonValueKind.Array ||
            candidates.GetArrayLength() == 0)
            return false;

        var firstCandidate = candidates[0];
        if (!firstCandidate.TryGetProperty("content", out var contentElement))
            return false;

        content = ReadContent(contentElement);
        return !string.IsNullOrWhiteSpace(content);
    }

    private static string ReadContent(JsonElement contentElement)
    {
        if (contentElement.ValueKind != JsonValueKind.Object ||
            !contentElement.TryGetProperty("parts", out var parts) ||
            parts.ValueKind != JsonValueKind.Array)
            return string.Empty;

        var builder = new StringBuilder();
        foreach (var item in parts.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Object &&
                item.TryGetProperty("text", out var textElement) &&
                textElement.ValueKind == JsonValueKind.String)
            {
                builder.Append(textElement.GetString());
            }
        }

        return builder.ToString();
    }

    private static string? ExtractGeminiError(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            return ExtractGeminiError(document.RootElement);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? ExtractGeminiError(JsonElement root)
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
        if (!root.TryGetProperty("candidates", out var candidates) ||
            candidates.ValueKind != JsonValueKind.Array ||
            candidates.GetArrayLength() == 0)
            return null;

        var firstCandidate = candidates[0];
        return firstCandidate.TryGetProperty("finishReason", out var finishReason) && finishReason.ValueKind == JsonValueKind.String
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

sealed record GeminiAnalysisResult(
    bool Success,
    string? Content,
    string Message,
    int? StatusCode = null,
    string? UpstreamError = null)
{
    public static GeminiAnalysisResult Ok(string content) =>
        new(true, content, "Gemini analysis completed.");

    public static GeminiAnalysisResult Failure(string message, int? statusCode = null, string? upstreamError = null) =>
        new(false, null, message, statusCode, upstreamError);
}
