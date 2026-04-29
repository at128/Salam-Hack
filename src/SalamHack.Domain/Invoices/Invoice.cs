using SalamHack.Domain.Common;
using SalamHack.Domain.Common.Constants;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Invoices.Events;
using SalamHack.Domain.Notifications;
using SalamHack.Domain.Payments;
using SalamHack.Domain.Projects;

namespace SalamHack.Domain.Invoices;

public class Invoice : AuditableEntity, ISoftDeletable
{
    private Invoice()
    {
    }

    private Invoice(
        Guid id,
        Guid userId,
        Guid projectId,
        Guid customerId,
        string invoiceNumber,
        decimal totalAmount,
        decimal taxAmount,
        decimal totalWithTax,
        decimal advanceAmount,
        DateTimeOffset issueDate,
        DateTimeOffset dueDate,
        string currency,
        string? notes)
        : base(id)
    {
        UserId = userId;
        ProjectId = projectId;
        CustomerId = customerId;
        InvoiceNumber = invoiceNumber;
        TotalAmount = totalAmount;
        TaxAmount = taxAmount;
        TotalWithTax = totalWithTax;
        AdvanceAmount = advanceAmount;
        PaidAmount = 0;
        Status = InvoiceStatus.Draft;
        IssueDate = issueDate;
        DueDate = dueDate;
        Currency = currency;
        Notes = notes;
    }

    public Guid UserId { get; private set; }
    public Guid ProjectId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string InvoiceNumber { get; private set; } = null!;
    public decimal TotalAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalWithTax { get; private set; }
    public decimal AdvanceAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public DateTimeOffset IssueDate { get; private set; }
    public DateTimeOffset DueDate { get; private set; }
    public string? Notes { get; private set; }
    public string Currency { get; private set; } = null!;
    public DateTimeOffset? DeletedAtUtc { get; set; }

    public decimal RemainingAmount => TotalWithTax - PaidAmount;
    public decimal AdvanceRemainingAmount => Math.Max(AdvanceAmount - PaidAmount, 0);
    public bool HasAdvanceBeenPaid => AdvanceRemainingAmount <= 0;
    public bool IsFullyPaid => RemainingAmount <= 0;

    public bool IsOverdueAt(DateTimeOffset asOfUtc) =>
        Status != InvoiceStatus.Draft &&
        Status != InvoiceStatus.Cancelled &&
        !IsFullyPaid &&
        DueDate < asOfUtc;

    public Project Project { get; private set; } = null!;
    public ICollection<Payment> Payments { get; private set; } = [];
    public ICollection<Notification> Notifications { get; private set; } = [];

    public static Result<Invoice> Create(
        Guid userId,
        Guid projectId,
        Guid customerId,
        string invoiceNumber,
        decimal totalAmount,
        decimal advanceAmount,
        DateTimeOffset issueDate,
        DateTimeOffset dueDate,
        string currency,
        string? notes = null)
    {
        if (userId == Guid.Empty)
            return InvoiceErrors.InvalidUserId;

        if (projectId == Guid.Empty)
            return InvoiceErrors.InvalidProjectId;

        if (customerId == Guid.Empty)
            return InvoiceErrors.InvalidCustomerId;

        if (string.IsNullOrWhiteSpace(invoiceNumber))
            return InvoiceErrors.InvoiceNumberRequired;

        var validation = ValidateAmountsAndDates(totalAmount, advanceAmount, issueDate, dueDate, currency);
        if (validation.IsError)
            return validation.Errors;

        var taxAmount = CalculateTax(totalAmount);
        var totalWithTax = totalAmount + taxAmount;

        return new Invoice(
            id: Guid.CreateVersion7(),
            userId: userId,
            projectId: projectId,
            customerId: customerId,
            invoiceNumber: invoiceNumber.Trim(),
            totalAmount: totalAmount,
            taxAmount: taxAmount,
            totalWithTax: totalWithTax,
            advanceAmount: advanceAmount,
            issueDate: issueDate,
            dueDate: dueDate,
            currency: currency.Trim(),
            notes: NormalizeOptional(notes));
    }

    public Result<Payment> RecordPayment(
        decimal amount,
        PaymentMethod method,
        DateTimeOffset paymentDate,
        string? notes,
        string currency)
    {
        if (Status == InvoiceStatus.Cancelled)
            return InvoiceErrors.CannotPayCancelledInvoice;

        if (IsFullyPaid)
            return InvoiceErrors.CannotPayFullyPaidInvoice;

        if (amount <= 0)
            return InvoiceErrors.PaymentAmountMustBePositive;

        if (amount > RemainingAmount)
            return InvoiceErrors.PaymentExceedsRemainingAmount;

        if (string.IsNullOrWhiteSpace(currency))
            return InvoiceErrors.CurrencyRequired;

        if (!string.Equals(currency.Trim(), Currency, StringComparison.OrdinalIgnoreCase))
            return InvoiceErrors.PaymentCurrencyMismatch;

        var paymentResult = Payment.Create(Id, amount, method, paymentDate, currency, notes);
        if (paymentResult.IsError)
            return paymentResult.Errors;

        var payment = paymentResult.Value;
        Payments.Add(payment);
        PaidAmount += amount;
        RefreshStatus();

        AddDomainEvent(new PaymentRecordedDomainEvent(
            invoiceId: Id,
            paymentId: payment.Id,
            projectId: ProjectId,
            amount: amount,
            remainingAmount: RemainingAmount,
            method: method,
            paymentDate: paymentDate));

        return payment;
    }

    public Result<Payment> RecordAdvancePayment(
        PaymentMethod method,
        DateTimeOffset paymentDate,
        string currency,
        string? notes = null)
    {
        if (AdvanceAmount <= 0)
            return InvoiceErrors.AdvanceNotRequired;

        if (HasAdvanceBeenPaid)
            return InvoiceErrors.AdvanceAlreadyPaid;

        return RecordPayment(
            AdvanceRemainingAmount,
            method,
            paymentDate,
            notes,
            currency);
    }

    public Result<Success> Cancel()
    {
        if (IsFullyPaid)
            return InvoiceErrors.CannotCancelPaidInvoice;

        if (Status == InvoiceStatus.Cancelled)
            return InvoiceErrors.AlreadyCancelled;

        Status = InvoiceStatus.Cancelled;
        AddDomainEvent(new InvoiceCancelledDomainEvent(Id, ProjectId));

        return Result.Success;
    }

    public Result<Success> MarkAsOverdue(Guid customerId, DateTimeOffset asOfUtc)
    {
        if (IsFullyPaid)
            return InvoiceErrors.CannotMarkPaidInvoiceOverdue;

        if (Status == InvoiceStatus.Cancelled)
            return InvoiceErrors.CannotMarkCancelledInvoiceOverdue;

        if (Status == InvoiceStatus.Draft)
            return InvoiceErrors.CannotMarkDraftInvoiceOverdue;

        if (Status == InvoiceStatus.Overdue)
            return Result.Success;

        if (DueDate >= asOfUtc)
            return InvoiceErrors.InvoiceNotDue;

        Status = InvoiceStatus.Overdue;

        AddDomainEvent(new InvoiceOverdueDomainEvent(
            invoiceId: Id,
            projectId: ProjectId,
            customerId: customerId,
            remainingAmount: RemainingAmount,
            dueDate: DueDate));

        return Result.Success;
    }

    public Result<Success> Send(DateTimeOffset asOfUtc)
    {
        if (Status != InvoiceStatus.Draft && Status != InvoiceStatus.PartiallyPaid)
            return InvoiceErrors.OnlyDraftOrPartiallyPaidCanBeSent;

        if (!IsFullyPaid && DueDate < asOfUtc)
            return InvoiceErrors.CannotSendOverdueInvoice;

        if (Status == InvoiceStatus.Draft)
            Status = InvoiceStatus.Sent;

        AddDomainEvent(new InvoiceSentDomainEvent(
            invoiceId: Id,
            projectId: ProjectId,
            customerId: CustomerId,
            totalWithTax: TotalWithTax,
            dueDate: DueDate));

        return Result.Success;
    }

    public Result<Success> UpdateDetails(
        decimal totalAmount,
        decimal advanceAmount,
        DateTimeOffset issueDate,
        DateTimeOffset dueDate,
        string currency,
        string? notes)
    {
        if (Status != InvoiceStatus.Draft)
            return InvoiceErrors.OnlyDraftCanBeEdited;

        var validation = ValidateAmountsAndDates(totalAmount, advanceAmount, issueDate, dueDate, currency);
        if (validation.IsError)
            return validation;

        var taxAmount = CalculateTax(totalAmount);

        TotalAmount = totalAmount;
        TaxAmount = taxAmount;
        TotalWithTax = totalAmount + taxAmount;
        AdvanceAmount = advanceAmount;
        IssueDate = issueDate;
        DueDate = dueDate;
        Currency = currency.Trim();
        Notes = NormalizeOptional(notes);

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

    public InvoiceStatus GetEffectiveStatus(DateTimeOffset asOfUtc)
    {
        if (Status == InvoiceStatus.Draft ||
            Status == InvoiceStatus.Cancelled)
        {
            return Status;
        }

        if (IsFullyPaid)
            return InvoiceStatus.Paid;

        return DueDate < asOfUtc
            ? InvoiceStatus.Overdue
            : Status;
    }

    public static decimal CalculateTax(decimal amount)
        => Math.Round(amount * ApplicationConstants.BusinessRules.TaxRate, 2);

    public static decimal CalculateTotalWithTax(decimal amount)
        => amount + CalculateTax(amount);

    private static Result<Success> ValidateAmountsAndDates(
        decimal totalAmount,
        decimal advanceAmount,
        DateTimeOffset issueDate,
        DateTimeOffset dueDate,
        string currency)
    {
        if (totalAmount <= 0)
            return InvoiceErrors.TotalAmountMustBePositive;

        if (advanceAmount < 0)
            return InvoiceErrors.AdvanceCannotBeNegative;

        if (dueDate < issueDate)
            return InvoiceErrors.DueDateBeforeIssueDate;

        if (string.IsNullOrWhiteSpace(currency))
            return InvoiceErrors.CurrencyRequired;

        if (advanceAmount > CalculateTotalWithTax(totalAmount))
            return InvoiceErrors.AdvanceExceedsTotal;

        return Result.Success;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private void RefreshStatus()
    {
        if (IsFullyPaid)
        {
            Status = InvoiceStatus.Paid;
            return;
        }

        if (PaidAmount > 0 && Status != InvoiceStatus.Overdue)
        {
            Status = InvoiceStatus.PartiallyPaid;
            return;
        }
    }
}
