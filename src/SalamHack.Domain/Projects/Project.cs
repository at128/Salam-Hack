using SalamHack.Domain.Analyses;
using SalamHack.Domain.Common;
using SalamHack.Domain.Common.Constants;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Customers;
using SalamHack.Domain.Expenses;
using SalamHack.Domain.Invoices;
using SalamHack.Domain.Projects.Events;
using SalamHack.Domain.Services;

namespace SalamHack.Domain.Projects;

public class Project : AuditableEntity, ISoftDeletable
{
    private Project()
    {
    }

    private Project(
        Guid id,
        Guid userId,
        Guid customerId,
        Guid serviceId,
        string projectName,
        decimal estimatedHours,
        decimal toolCost,
        int revision,
        bool isUrgent,
        decimal suggestedPrice,
        DateTimeOffset startDate,
        DateTimeOffset endDate)
        : base(id)
    {
        UserId = userId;
        CustomerId = customerId;
        ServiceId = serviceId;
        ProjectName = projectName;
        EstimatedHours = estimatedHours;
        ToolCost = toolCost;
        Revision = revision;
        IsUrgent = isUrgent;
        SuggestedPrice = suggestedPrice;

        var realCost = CalculateRealCost(estimatedHours, toolCost);
        MinPrice = Math.Round(realCost * ApplicationConstants.BusinessRules.MinimumPriceMultiplier, 2);
        AdvanceAmount = Math.Round(suggestedPrice * ApplicationConstants.BusinessRules.AdvancePaymentRate, 2);
        ProfitMargin = CalculateMarginPercent(suggestedPrice, realCost);

        Status = ProjectStatus.Planning;
        ActualHours = 0;
        StartDate = startDate;
        EndDate = endDate;
    }

    public Guid UserId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid ServiceId { get; private set; }
    public string ProjectName { get; private set; } = null!;
    public decimal EstimatedHours { get; private set; }
    public decimal ToolCost { get; private set; }
    public int Revision { get; private set; }
    public bool IsUrgent { get; private set; }
    public decimal ProfitMargin { get; private set; }
    public decimal SuggestedPrice { get; private set; }
    public decimal MinPrice { get; private set; }
    public decimal AdvanceAmount { get; private set; }
    public ProjectStatus Status { get; private set; }
    public decimal ActualHours { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset EndDate { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }

    public bool IsHealthy => ProfitMargin >= ApplicationConstants.BusinessRules.HealthyMarginThreshold;

    public Customer Customer { get; private set; } = null!;
    public Service Service { get; private set; } = null!;
    public ICollection<Expense> Expenses { get; private set; } = [];
    public ICollection<Invoice> Invoices { get; private set; } = [];
    public ICollection<Analysis> Analyses { get; private set; } = [];

    public static Result<Project> Create(
        Guid userId,
        Guid customerId,
        Guid serviceId,
        string projectName,
        decimal estimatedHours,
        decimal toolCost,
        int revision,
        bool isUrgent,
        decimal suggestedPrice,
        DateTimeOffset startDate,
        DateTimeOffset endDate)
    {
        var validation = ValidateIdentity(userId, customerId, serviceId);
        if (validation.IsError)
            return validation.Errors;

        validation = ValidateName(projectName);
        if (validation.IsError)
            return validation.Errors;

        validation = ValidateEstimate(estimatedHours, toolCost, revision, suggestedPrice);
        if (validation.IsError)
            return validation.Errors;

        validation = ValidateSchedule(startDate, endDate);
        if (validation.IsError)
            return validation.Errors;

        return new Project(
            Guid.CreateVersion7(),
            userId,
            customerId,
            serviceId,
            projectName.Trim(),
            estimatedHours,
            toolCost,
            revision,
            isUrgent,
            suggestedPrice,
            startDate,
            endDate);
    }

    public Result<ProjectHealthSnapshot> GetHealthSnapshot(decimal additionalExpenses = 0)
    {
        if (SuggestedPrice <= 0)
            return ProjectErrors.PriceCannotBeZero;

        if (additionalExpenses < 0)
            return ProjectErrors.AdditionalExpensesCannotBeNegative;

        var hoursForHealth = ActualHours > 0 ? ActualHours : EstimatedHours;
        var baseCost = CalculateRealCost(hoursForHealth, ToolCost);
        var totalCost = baseCost + additionalExpenses;
        var profit = SuggestedPrice - totalCost;
        var marginPercent = Math.Round((profit / SuggestedPrice) * 100, 2);
        var hourlyProfit = ActualHours > 0
            ? Math.Round(profit / ActualHours, 2)
            : EstimatedHours > 0 ? Math.Round(profit / EstimatedHours, 2) : 0;

        var healthStatus = marginPercent switch
        {
            >= ApplicationConstants.BusinessRules.HealthyMarginThreshold => ProjectHealthStatus.Healthy,
            >= ApplicationConstants.BusinessRules.AtRiskMarginThreshold => ProjectHealthStatus.AtRisk,
            _ => ProjectHealthStatus.Critical
        };

        return new ProjectHealthSnapshot(
            BaseCost: baseCost,
            AdditionalExpenses: additionalExpenses,
            TotalCost: totalCost,
            Profit: profit,
            MarginPercent: marginPercent,
            HourlyProfit: hourlyProfit,
            HealthStatus: healthStatus);
    }

    public Result<Success> Start()
    {
        if (Status == ProjectStatus.Cancelled)
            return ProjectErrors.CannotStartCancelledProject;

        if (Status == ProjectStatus.Completed)
            return ProjectErrors.AlreadyCompleted;

        if (Status == ProjectStatus.InProgress)
            return ProjectErrors.AlreadyInProgress;

        var previousStatus = Status;
        Status = ProjectStatus.InProgress;
        AddDomainEvent(new ProjectStatusChangedDomainEvent(Id, CustomerId, previousStatus, Status));

        return Result.Success;
    }

    public Result<Success> Complete()
    {
        if (Status == ProjectStatus.Cancelled)
            return ProjectErrors.CannotCompleteCancelledProject;

        if (Status == ProjectStatus.Completed)
            return ProjectErrors.AlreadyCompleted;

        var previousStatus = Status;
        Status = ProjectStatus.Completed;
        AddDomainEvent(new ProjectStatusChangedDomainEvent(Id, CustomerId, previousStatus, Status));

        return Result.Success;
    }

    public Result<Success> Cancel()
    {
        if (Status == ProjectStatus.Completed)
            return ProjectErrors.CannotCancelCompletedProject;

        if (Status == ProjectStatus.Cancelled)
            return ProjectErrors.AlreadyCancelled;

        var previousStatus = Status;
        Status = ProjectStatus.Cancelled;
        AddDomainEvent(new ProjectStatusChangedDomainEvent(Id, CustomerId, previousStatus, Status));

        return Result.Success;
    }

    public Result<Success> UpdateEstimate(
        decimal estimatedHours,
        decimal toolCost,
        int revision,
        bool isUrgent,
        decimal suggestedPrice)
    {
        var validation = ValidateEstimate(estimatedHours, toolCost, revision, suggestedPrice);
        if (validation.IsError)
            return validation;

        EstimatedHours = estimatedHours;
        ToolCost = toolCost;
        Revision = revision;
        IsUrgent = isUrgent;
        SuggestedPrice = suggestedPrice;

        var realCost = CalculateRealCost(estimatedHours, toolCost);
        MinPrice = Math.Round(realCost * ApplicationConstants.BusinessRules.MinimumPriceMultiplier, 2);
        AdvanceAmount = Math.Round(suggestedPrice * ApplicationConstants.BusinessRules.AdvancePaymentRate, 2);
        ProfitMargin = CalculateMarginPercent(suggestedPrice, realCost);

        return Result.Success;
    }

    public Result<Success> UpdateSchedule(DateTimeOffset startDate, DateTimeOffset endDate)
    {
        var validation = ValidateSchedule(startDate, endDate);
        if (validation.IsError)
            return validation;

        StartDate = startDate;
        EndDate = endDate;

        return Result.Success;
    }

    public Result<Success> UpdateProjectName(string projectName)
    {
        var validation = ValidateName(projectName);
        if (validation.IsError)
            return validation;

        ProjectName = projectName.Trim();
        return Result.Success;
    }

    public Result<Success> Rename(string newProjectName)
        => UpdateProjectName(newProjectName);

    public Result<Success> SetActualHours(decimal actualHours)
    {
        if (actualHours < 0)
            return ProjectErrors.ActualHoursCannotBeNegative;

        ActualHours = actualHours;
        return Result.Success;
    }

    public void Delete(DateTimeOffset deletedAtUtc)
    {
        DeletedAtUtc = deletedAtUtc;
    }

    public void Restore()
    {
        DeletedAtUtc = null;
    }

    public static decimal CalculateRealCost(decimal estimatedHours, decimal toolCost)
        => Math.Round((estimatedHours * ApplicationConstants.BusinessRules.CostRatePerHour) + toolCost, 2);

    public static decimal CalculateMarginPercent(decimal price, decimal cost)
        => price > 0 ? Math.Round((price - cost) / price * 100, 2) : 0;

    private static Result<Success> ValidateIdentity(Guid userId, Guid customerId, Guid serviceId)
    {
        if (userId == Guid.Empty)
            return ProjectErrors.InvalidUserId;

        if (customerId == Guid.Empty)
            return ProjectErrors.InvalidCustomerId;

        if (serviceId == Guid.Empty)
            return ProjectErrors.InvalidServiceId;

        return Result.Success;
    }

    private static Result<Success> ValidateName(string projectName)
    {
        if (string.IsNullOrWhiteSpace(projectName))
            return ProjectErrors.ProjectNameRequired;

        return Result.Success;
    }

    private static Result<Success> ValidateEstimate(
        decimal estimatedHours,
        decimal toolCost,
        int revision,
        decimal suggestedPrice)
    {
        if (estimatedHours <= 0)
            return ProjectErrors.EstimatedHoursMustBePositive;

        if (toolCost < 0)
            return ProjectErrors.ToolCostCannotBeNegative;

        if (revision < 0)
            return ProjectErrors.RevisionCannotBeNegative;

        if (suggestedPrice <= 0)
            return ProjectErrors.SuggestedPriceMustBePositive;

        return Result.Success;
    }

    private static Result<Success> ValidateSchedule(DateTimeOffset startDate, DateTimeOffset endDate)
    {
        if (endDate < startDate)
            return ProjectErrors.EndDateBeforeStartDate;

        return Result.Success;
    }
}
