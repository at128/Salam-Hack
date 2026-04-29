using System.Text.Json;
using SalamHack.Application.Features.Analyses.Models;

namespace SalamHack.Application.Features.Analyses;

internal static class ProjectAnalysisAiPromptBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public static ProjectAiAnalysisPrompt Build(ProjectAiAnalysisInputDto input)
    {
        const string systemPrompt = """
You are a senior financial and project profitability analyst for freelancers and small service businesses.

Analyze the project using ONLY the numbers and facts provided by the system.
Do not invent missing values, invoices, expenses, dates, or payments.
Do not recalculate totals differently from the provided system numbers.
Use the systemHealthStatus as the baseline classification unless the provided risk rules clearly indicate a stricter status.
Focus on profitability, delivery risk, scope creep, cashflow, and practical next actions.

Return valid JSON only. No markdown, no prose outside JSON.
Write all human-readable text fields in Arabic. Keep enum/code fields exactly in English where the schema specifies fixed values, including overallStatus, severity, priority, and confidence.
The response must match this exact shape:
{
  "overallStatus": "Healthy | AtRisk | Critical",
  "score": 0,
  "summary": "short business summary",
  "mainRisks": [
    {
      "title": "risk title",
      "severity": "Low | Medium | High | Critical",
      "reason": "number-backed reason"
    }
  ],
  "opportunities": [
    {
      "title": "opportunity title",
      "impact": "expected business impact"
    }
  ],
  "recommendedActions": [
    {
      "action": "specific action",
      "priority": "Low | Medium | High",
      "expectedEffect": "expected measurable effect"
    }
  ],
  "clientMessage": "optional short client-facing message",
  "whatHappened": "one concise paragraph explaining the numbers",
  "whatItMeans": "one concise paragraph explaining the business implication",
  "whatToDo": "one concise paragraph with the next operational action",
  "confidence": "Low | Medium | High"
}

Scoring rules:
- Healthy projects should usually score 75-100.
- AtRisk projects should usually score 45-74.
- Critical projects should usually score 0-44.
- If marginPercent < 15, classify as Critical.
- If marginPercent is between 15 and 30, classify as AtRisk.
- If actual hours exceed estimated hours by more than 20%, mention scope/time risk.
- If remainingAmount or overdueAmount is high compared to profit, mention cashflow risk.
- Recommendations must be specific and actionable.
- Avoid generic advice.
""";

        var userPrompt = $"""
Analyze this project and return the required JSON.

Project data:
{JsonSerializer.Serialize(input, JsonOptions)}
""";

        return new ProjectAiAnalysisPrompt(systemPrompt, userPrompt);
    }
}
