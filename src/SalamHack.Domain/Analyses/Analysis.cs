using SalamHack.Domain.Common;
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

    public void Update(
        string whatHappened,
        string whatItMeans,
        string whatToDo,
        string healthStatus,
        DateTimeOffset generatedAt)
    {
        Update(
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

    public void Update(
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

    public void MarkReviewed(DateTimeOffset reviewedAtUtc)
    {
        ReviewedAtUtc = reviewedAtUtc;
    }
}
