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
                $"{projectName} is performing above the healthy margin threshold.",
                "The current price covers the real cost and leaves enough room for profit.",
                "Keep this pricing pattern and use it as a reference for similar work.");
        }

        if (health.MarginPercent >= ApplicationConstants.BusinessRules.AtRiskMarginThreshold)
        {
            return new ProjectNarrative(
                $"{projectName} margin is below the healthy benchmark.",
                "The project is still profitable, but extra expenses or additional hours can quickly erase margin.",
                "Review scope, protect change requests, and consider a higher price on similar projects.");
        }

        return new ProjectNarrative(
            $"{projectName} is under the at-risk margin threshold.",
            "The project cost structure is too close to the selling price.",
            "Raise the price, reduce avoidable expenses, or renegotiate scope before repeating this pattern.");
    }
}

internal sealed record ProjectNarrative(string WhatHappened, string WhatItMeans, string WhatToDo);
