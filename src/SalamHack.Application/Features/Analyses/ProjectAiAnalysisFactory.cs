using System.Text.Json;
using SalamHack.Application.Features.Analyses.Models;
using SalamHack.Domain.Common.Constants;
using SalamHack.Domain.Projects;

namespace SalamHack.Application.Features.Analyses;

internal static class ProjectAiAnalysisFactory
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public static ProjectAiAnalysisDto BuildFallback(
        ProjectAiAnalysisInputDto input,
        ProjectNarrative narrative)
    {
        var riskSeverity = input.SystemHealthStatus switch
        {
            nameof(ProjectHealthStatus.Critical) => "حرج",
            nameof(ProjectHealthStatus.AtRisk) => "مرتفع",
            _ => "منخفض"
        };

        var risks = new List<ProjectAiAnalysisRiskDto>();
        if (input.MarginPercent < ApplicationConstants.BusinessRules.HealthyMarginThreshold)
        {
            risks.Add(new ProjectAiAnalysisRiskDto(
                "ضغط على هامش الربح",
                riskSeverity,
                $"هامش الربح {input.MarginPercent:0.##}% وهو أقل من الحد الصحي {input.HealthyMarginThreshold:0.##}%."));
        }

        if (input.HoursOverrunPercent > 20)
        {
            risks.Add(new ProjectAiAnalysisRiskDto(
                "تجاوز في النطاق أو الوقت",
                "مرتفع",
                $"الساعات الفعلية أعلى من التقدير بنسبة {input.HoursOverrunPercent:0.##}%."));
        }

        if (input.InvoiceSummary.OverdueAmount > 0)
        {
            risks.Add(new ProjectAiAnalysisRiskDto(
                "مخاطر تحصيل",
                "مرتفع",
                $"المبلغ المتأخر هو {input.InvoiceSummary.OverdueAmount:0.##}."));
        }

        if (risks.Count == 0)
        {
            risks.Add(new ProjectAiAnalysisRiskDto(
                "لا توجد مخاطر ربحية كبيرة",
                "منخفض",
                "الأرقام الحالية أعلى من حد هامش الربح الصحي."));
        }

        var opportunities = new[]
        {
            new ProjectAiAnalysisOpportunityDto(
                "استخدم هذا المشروع كدليل للتسعير",
                input.MarginPercent >= input.HealthyMarginThreshold
                    ? "يمكن الاعتماد عليه لتسعير الأعمال المشابهة مستقبلا."
                    : "يوضح أين يحتاج التسعير أو ضبط النطاق إلى تحسين.")
        };

        var actions = BuildFallbackActions(input);

        return new ProjectAiAnalysisDto(
            input.SystemHealthStatus,
            CalculateScore(input.MarginPercent),
            $"هامش المشروع {input.ProjectName} هو {input.MarginPercent:0.##}% مع ربح بقيمة {input.Profit:0.##}.",
            risks,
            opportunities,
            actions,
            BuildClientMessage(input),
            narrative.WhatHappened,
            narrative.WhatItMeans,
            narrative.WhatToDo,
            "High");
    }

    public static ProjectAiAnalysisDto Normalize(
        ProjectAiAnalysisDto? analysis,
        ProjectAiAnalysisInputDto input,
        ProjectNarrative fallbackNarrative)
    {
        var fallback = BuildFallback(input, fallbackNarrative);
        if (analysis is null)
            return fallback;

        var overallStatus = NormalizeStatus(analysis.OverallStatus, input);
        var score = Math.Clamp(analysis.Score, 0, 100);

        return new ProjectAiAnalysisDto(
            overallStatus,
            score,
            NormalizeText(analysis.Summary, fallback.Summary),
            NormalizeList(analysis.MainRisks, fallback.MainRisks),
            NormalizeList(analysis.Opportunities, fallback.Opportunities),
            NormalizeList(analysis.RecommendedActions, fallback.RecommendedActions),
            NormalizeOptionalText(analysis.ClientMessage, fallback.ClientMessage),
            NormalizeText(analysis.WhatHappened, fallback.WhatHappened),
            NormalizeText(analysis.WhatItMeans, fallback.WhatItMeans),
            NormalizeText(analysis.WhatToDo, fallback.WhatToDo),
            NormalizeConfidence(analysis.Confidence));
    }

    public static ProjectAiAnalysisDto? TryParseMetadata(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
            return null;

        try
        {
            var metadata = JsonSerializer.Deserialize<ProjectAiAnalysisMetadataDto>(metadataJson, JsonOptions);
            return metadata?.StructuredAnalysis;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static IReadOnlyCollection<ProjectAiAnalysisActionDto> BuildFallbackActions(ProjectAiAnalysisInputDto input)
    {
        var actions = new List<ProjectAiAnalysisActionDto>();

        if (input.MarginPercent < input.HealthyMarginThreshold)
        {
            actions.Add(new ProjectAiAnalysisActionDto(
                "راجع التسعير قبل تكرار عمل مشابه",
                "مرتفع",
                "يحمي المشروع القادم من ضغط هامش الربح نفسه."));
        }

        if (input.HoursOverrunPercent > 20)
        {
            actions.Add(new ProjectAiAnalysisActionDto(
                "وثق النطاق الإضافي واعتمد طلبات التغيير مبكرا",
                "مرتفع",
                "يقلل الساعات غير المدفوعة ويحمي ربحية الساعة."));
        }

        if (input.InvoiceSummary.RemainingAmount > 0)
        {
            actions.Add(new ProjectAiAnalysisActionDto(
                "تابع الرصيد المتبقي من الفاتورة",
                input.InvoiceSummary.OverdueAmount > 0 ? "مرتفع" : "متوسط",
                "يحسن التدفق النقدي ويقلل مخاطر التحصيل."));
        }

        if (actions.Count == 0)
        {
            actions.Add(new ProjectAiAnalysisActionDto(
                "حافظ على نمط التسعير والتنفيذ الحالي",
                "متوسط",
                "يحافظ على ربحية صحية في المشاريع المشابهة."));
        }

        return actions;
    }

    private static string? BuildClientMessage(ProjectAiAnalysisInputDto input)
        => input.HoursOverrunPercent > 20
            ? "احتاج المشروع وقتا أكثر من التقدير الأولي، لذلك يجب تأكيد أي نطاق إضافي قبل الاستمرار."
            : null;

    private static int CalculateScore(decimal marginPercent)
    {
        if (marginPercent <= 0)
            return 10;

        if (marginPercent < ApplicationConstants.BusinessRules.AtRiskMarginThreshold)
            return (int)Math.Clamp(Math.Round(marginPercent * 2), 0, 44);

        if (marginPercent < ApplicationConstants.BusinessRules.HealthyMarginThreshold)
            return (int)Math.Clamp(Math.Round(45 + (marginPercent - 15) * 2), 45, 74);

        return (int)Math.Clamp(Math.Round(75 + Math.Min(marginPercent - 30, 25)), 75, 100);
    }

    private static string NormalizeStatus(string? value, ProjectAiAnalysisInputDto input)
    {
        var requiredStatus = GetRequiredStatus(input);
        var candidate = string.Equals(value, nameof(ProjectHealthStatus.Healthy), StringComparison.OrdinalIgnoreCase)
            ? nameof(ProjectHealthStatus.Healthy)
            : string.Equals(value, nameof(ProjectHealthStatus.AtRisk), StringComparison.OrdinalIgnoreCase)
                ? nameof(ProjectHealthStatus.AtRisk)
                : string.Equals(value, nameof(ProjectHealthStatus.Critical), StringComparison.OrdinalIgnoreCase)
                    ? nameof(ProjectHealthStatus.Critical)
                    : input.SystemHealthStatus;

        return SeverityRank(candidate) >= SeverityRank(requiredStatus)
            ? candidate
            : requiredStatus;
    }

    private static string GetRequiredStatus(ProjectAiAnalysisInputDto input)
    {
        if (input.MarginPercent < ApplicationConstants.BusinessRules.AtRiskMarginThreshold)
            return nameof(ProjectHealthStatus.Critical);

        if (input.MarginPercent < ApplicationConstants.BusinessRules.HealthyMarginThreshold)
            return nameof(ProjectHealthStatus.AtRisk);

        return input.SystemHealthStatus;
    }

    private static int SeverityRank(string status)
        => status switch
        {
            nameof(ProjectHealthStatus.Critical) => 3,
            nameof(ProjectHealthStatus.AtRisk) => 2,
            _ => 1
        };

    private static string NormalizeConfidence(string? value)
        => string.Equals(value, "Low", StringComparison.OrdinalIgnoreCase)
            ? "Low"
            : string.Equals(value, "Medium", StringComparison.OrdinalIgnoreCase)
                ? "Medium"
                : "High";

    private static string NormalizeText(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string? NormalizeOptionalText(string? value, string? fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static IReadOnlyCollection<T> NormalizeList<T>(
        IReadOnlyCollection<T>? values,
        IReadOnlyCollection<T> fallback)
        => values is null || values.Count == 0 ? fallback : values;
}
