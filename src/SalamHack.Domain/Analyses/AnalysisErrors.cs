using SalamHack.Domain.Common.Results;

namespace SalamHack.Domain.Analyses;

public static class AnalysisErrors
{
    public static readonly Error InvalidProjectId = Error.Validation(
        "Analysis.InvalidProjectId",
        "معرف المشروع مطلوب.");

    public static readonly Error WhatHappenedRequired = Error.Validation(
        "Analysis.WhatHappenedRequired",
        "وصف ما حدث مطلوب.");

    public static readonly Error WhatItMeansRequired = Error.Validation(
        "Analysis.WhatItMeansRequired",
        "وصف ما يعنيه الأمر مطلوب.");

    public static readonly Error WhatToDoRequired = Error.Validation(
        "Analysis.WhatToDoRequired",
        "وصف ما يجب فعله مطلوب.");

    public static readonly Error HealthStatusRequired = Error.Validation(
        "Analysis.HealthStatusRequired",
        "حالة الصحة مطلوبة.");

    public static readonly Error ConfidenceScoreOutOfRange = Error.Validation(
        "Analysis.ConfidenceScoreOutOfRange",
        "يجب أن تكون درجة الثقة بين 0 و1.");
}
