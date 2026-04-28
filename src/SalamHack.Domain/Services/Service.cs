using SalamHack.Domain.Common;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Projects;

namespace SalamHack.Domain.Services;

public class Service : AuditableEntity, ISoftDeletable
{
    private Service()
    {
    }

    private Service(
        Guid id,
        Guid userId,
        string serviceName,
        ServiceCategory category,
        decimal defaultHourlyRate,
        int defaultRevisions,
        bool isActive = true)
        : base(id)
    {
        UserId = userId;
        ServiceName = serviceName;
        Category = category;
        DefaultHourlyRate = defaultHourlyRate;
        DefaultRevisions = defaultRevisions;
        IsActive = isActive;
    }

    public Guid UserId { get; private set; }
    public string ServiceName { get; private set; } = null!;
    public ServiceCategory Category { get; private set; }
    public decimal DefaultHourlyRate { get; private set; }
    public int DefaultRevisions { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset? DeletedAtUtc { get; set; }

    public ICollection<Project> Projects { get; private set; } = [];

    public static Result<Service> Create(
        Guid userId,
        string serviceName,
        ServiceCategory category,
        decimal defaultHourlyRate,
        int defaultRevisions,
        bool isActive = true)
    {
        var validation = Validate(userId, serviceName, defaultHourlyRate, defaultRevisions);
        if (validation.IsError)
            return validation.Errors;

        return new Service(
            Guid.CreateVersion7(),
            userId,
            serviceName.Trim(),
            category,
            defaultHourlyRate,
            defaultRevisions,
            isActive);
    }

    public Result<Success> Update(
        string serviceName,
        ServiceCategory category,
        decimal defaultHourlyRate,
        int defaultRevisions)
    {
        var validation = Validate(UserId, serviceName, defaultHourlyRate, defaultRevisions);
        if (validation.IsError)
            return validation;

        ServiceName = serviceName.Trim();
        Category = category;
        DefaultHourlyRate = defaultHourlyRate;
        DefaultRevisions = defaultRevisions;

        return Result.Success;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Delete(DateTimeOffset deletedAtUtc)
    {
        DeletedAtUtc = deletedAtUtc;
        Deactivate();
    }

    public void Restore()
    {
        DeletedAtUtc = null;
    }

    private static Result<Success> Validate(
        Guid userId,
        string serviceName,
        decimal defaultHourlyRate,
        int defaultRevisions)
    {
        if (userId == Guid.Empty)
            return ServiceErrors.InvalidUserId;

        if (string.IsNullOrWhiteSpace(serviceName))
            return ServiceErrors.ServiceNameRequired;

        if (defaultHourlyRate <= 0)
            return ServiceErrors.DefaultHourlyRateMustBePositive;

        if (defaultRevisions < 0)
            return ServiceErrors.DefaultRevisionsCannotBeNegative;

        return Result.Success;
    }
}
