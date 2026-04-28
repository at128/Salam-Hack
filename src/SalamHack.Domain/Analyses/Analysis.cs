using SalamHack.Domain.Common;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Projects;

namespace SalamHack.Domain.Analyses;

public class Analysis : AuditableEntity
{
    private Analysis()
    {
    }

    public Analysis(
        Guid id,
        Guid projectId,
        string whatHappened,
        string whatItMeans,
        string whatToDo,
        string healthStatus,
        DateTimeOffset generatedAt)
        : this(
            id,
            projectId,
            AnalysisType.ProjectHealth,
            whatHappened,
            whatItMeans,
            whatToDo,
            healthStatus,
            generatedAt)
    {
    }

    public Analysis(
        Guid id,
        Guid projectId,
        AnalysisType type,
        string whatHappened,
        string whatItMeans,
        string whatToDo,
        string healthStatus,
        DateTimeOffset generatedAt,
        string? title = null,
        string? summary = null,
        decimal? confidenceScore = null,
        string? metadataJson = null)
        : base(id)
    {
        ProjectId = projectId;
        Type = type;
        WhatHappened = whatHappened;
        WhatItMeans = whatItMeans;
        WhatToDo = whatToDo;
        HealthStatus = healthStatus;
        GeneratedAt = generatedAt;
        Title = title;
        Summary = summary;
        ConfidenceScore = confidenceScore;
        MetadataJson = metadataJson;
    }

    public Guid ProjectId { get; private set; }
    public AnalysisType Type { get; private set; }
    public string WhatHappened { get; private set; } = null!;
    public string WhatItMeans { get; private set; } = null!;
    public string WhatToDo { get; private set; } = null!;
    public string HealthStatus { get; private set; } = null!;
    public DateTimeOffset GeneratedAt { get; private set; }
    public string? Title { get; private set; }
    public string? Summary { get; private set; }
    public decimal? ConfidenceScore { get; private set; }
    public string? MetadataJson { get; private set; }
    public DateTimeOffset? ReviewedAtUtc { get; private set; }

    public Project Project { get; private set; } = null!;

    public static Result<Analysis> Create(
        Guid projectId,
        AnalysisType type,
        string whatHappened,
        string whatItMeans,
        string whatToDo,
        string healthStatus,
        DateTimeOffset generatedAt,
        string? title = null,
        string? summary = null,
        decimal? confidenceScore = null,
        string? metadataJson = null)
    {
        var validation = Validate(projectId, whatHappened, whatItMeans, whatToDo, healthStatus, confidenceScore);
        if (validation.IsError)
            return validation.Errors;

        return new Analysis(
            Guid.CreateVersion7(),
            projectId,
            type,
            whatHappened.Trim(),
            whatItMeans.Trim(),
            whatToDo.Trim(),
            healthStatus.Trim(),
            generatedAt,
            NormalizeOptional(title),
            NormalizeOptional(summary),
            confidenceScore,
            NormalizeOptional(metadataJson));
    }

    public Result<Success> Update(
        string whatHappened,
        string whatItMeans,
        string whatToDo,
        string healthStatus,
        DateTimeOffset generatedAt)
    {
        return Update(
            Type,
            whatHappened,
            whatItMeans,
            whatToDo,
            healthStatus,
            generatedAt,
            Title,
            Summary,
            ConfidenceScore,
            MetadataJson);
    }

    public Result<Success> Update(
        AnalysisType type,
        string whatHappened,
        string whatItMeans,
        string whatToDo,
        string healthStatus,
        DateTimeOffset generatedAt,
        string? title,
        string? summary,
        decimal? confidenceScore,
        string? metadataJson)
    {
        var validation = Validate(ProjectId, whatHappened, whatItMeans, whatToDo, healthStatus, confidenceScore);
        if (validation.IsError)
            return validation;

        Type = type;
        WhatHappened = whatHappened.Trim();
        WhatItMeans = whatItMeans.Trim();
        WhatToDo = whatToDo.Trim();
        HealthStatus = healthStatus.Trim();
        GeneratedAt = generatedAt;
        Title = NormalizeOptional(title);
        Summary = NormalizeOptional(summary);
        ConfidenceScore = confidenceScore;
        MetadataJson = NormalizeOptional(metadataJson);

        return Result.Success;
    }

    public void MarkReviewed(DateTimeOffset reviewedAtUtc)
    {
        ReviewedAtUtc = reviewedAtUtc;
    }

    private static Result<Success> Validate(
        Guid projectId,
        string whatHappened,
        string whatItMeans,
        string whatToDo,
        string healthStatus,
        decimal? confidenceScore)
    {
        if (projectId == Guid.Empty)
            return AnalysisErrors.InvalidProjectId;

        if (string.IsNullOrWhiteSpace(whatHappened))
            return AnalysisErrors.WhatHappenedRequired;

        if (string.IsNullOrWhiteSpace(whatItMeans))
            return AnalysisErrors.WhatItMeansRequired;

        if (string.IsNullOrWhiteSpace(whatToDo))
            return AnalysisErrors.WhatToDoRequired;

        if (string.IsNullOrWhiteSpace(healthStatus))
            return AnalysisErrors.HealthStatusRequired;

        if (confidenceScore is < 0 or > 1)
            return AnalysisErrors.ConfidenceScoreOutOfRange;

        return Result.Success;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
