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
    private const string OpenRouterApiUrl = "https://openrouter.ai/api/v1/chat/completions";
    private const string OpenRouterModel = "openai/gpt-4o-mini";
    private const string SiteUrl = "https://salamhack.com";
    private const string SiteTitle = "SalamHack";
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

        var apiKey = configuration["OpenRouter:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "OpenRouter API key is not configured." });

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
        var openRouterContent = await RequestOpenRouterAsync(prompt, apiKey, ct);
        if (openRouterContent is null)
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "OpenRouter did not return analysis text." });

        return OkResponse(new ProfitAiAnalysisResponse(openRouterContent, DateTimeOffset.UtcNow));
    }

    private async Task<string?> RequestOpenRouterAsync(string prompt, string apiKey, CancellationToken ct)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, OpenRouterApiUrl);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Headers.Add("HTTP-Referer", SiteUrl);
        httpRequest.Headers.Add("X-Title", SiteTitle);
        httpRequest.Content = JsonContent.Create(new
        {
            model = OpenRouterModel,
            messages = new[] { new { role = "user", content = prompt } },
            temperature = 0.2
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
            return null;
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            logger.LogError(ex, "OpenRouter request timed out.");
            return null;
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "OpenRouter returned non-success status {StatusCode}: {Body}",
                    (int)response.StatusCode,
                    body);

                return null;
            }
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            return TryGetContent(document.RootElement, out var content) ? content : null;
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "OpenRouter returned invalid JSON: {Body}", body);
            return null;
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
        builder.AppendLine("أنت محلل أرباح ذكي داخل منصة SalamHack للمستقلين.");
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
            !message.TryGetProperty("content", out var contentElement))
            return false;

        content = contentElement.GetString() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(content);
    }
}

public sealed record ProfitAiAnalysisResponse(string Content, DateTimeOffset GeneratedAt);
