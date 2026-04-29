using SalamHack.Domain.Common.Results;

namespace SalamHack.Domain.Analyses;

public static class AnalysisErrors
{
    public static readonly Error InvalidProjectId = Error.Validation(
        "Analysis.InvalidProjectId",
        "Project id is required.");

    public static readonly Error WhatHappenedRequired = Error.Validation(
        "Analysis.WhatHappenedRequired",
        "What happened is required.");

    public static readonly Error WhatItMeansRequired = Error.Validation(
        "Analysis.WhatItMeansRequired",
        "What it means is required.");

    public static readonly Error WhatToDoRequired = Error.Validation(
        "Analysis.WhatToDoRequired",
        "What to do is required.");

    public static readonly Error HealthStatusRequired = Error.Validation(
        "Analysis.HealthStatusRequired",
        "Health status is required.");

    public static readonly Error ConfidenceScoreOutOfRange = Error.Validation(
        "Analysis.ConfidenceScoreOutOfRange",
        "Confidence score must be between 0 and 1.");
}
