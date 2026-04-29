using SalamHack.Domain.Common.Constants;
using SalamHack.Domain.Projects;

namespace SalamHack.Application.Features.Analyses;

internal static class ProjectAnalysisNarrative
{
    public static ProjectNarrative Build(string projectName, ProjectHealthSnapshot health)
    {
        if (health.MarginPercent >= ApplicationConstants.BusinessRules.HealthyMarginThreshold)
        {
            return new ProjectNarrative(
                $"المشروع {projectName} يحقق هامش ربح أعلى من الحد الصحي.",
                "السعر الحالي يغطي التكلفة الفعلية ويترك مساحة كافية للربح.",
                "حافظ على نمط التسعير الحالي واستخدمه كمرجع للأعمال المشابهة.");
        }

        if (health.MarginPercent >= ApplicationConstants.BusinessRules.AtRiskMarginThreshold)
        {
            return new ProjectNarrative(
                $"هامش المشروع {projectName} أقل من المستوى الصحي.",
                "المشروع ما زال مربحا، لكن المصاريف الإضافية أو الساعات الزائدة قد تقلل الهامش بسرعة.",
                "راجع نطاق العمل، واضبط طلبات التغيير، وفكر في سعر أعلى للمشاريع المشابهة.");
        }

        return new ProjectNarrative(
            $"المشروع {projectName} أقل من حد الخطر لهامش الربح.",
            "تكلفة المشروع قريبة جدا من سعر البيع.",
            "ارفع السعر، أو قلل المصاريف غير الضرورية، أو أعد التفاوض على النطاق قبل تكرار هذا النمط.");
    }
}

internal sealed record ProjectNarrative(string WhatHappened, string WhatItMeans, string WhatToDo);
