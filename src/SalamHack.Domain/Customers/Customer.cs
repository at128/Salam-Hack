using SalamHack.Domain.Common;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Projects;

namespace SalamHack.Domain.Customers;

public class Customer : AuditableEntity, ISoftDeletable
{
    private Customer()
    {
    }

    private Customer(
        Guid id,
        Guid userId,
        string customerName,
        string email,
        string phone,
        ClientType clientType,
        string? companyName = null,
        string? notes = null)
        : base(id)
    {
        UserId = userId;
        CustomerName = customerName;
        Email = email;
        Phone = phone;
        ClientType = clientType;
        CompanyName = companyName;
        Notes = notes;
    }

    public Guid UserId { get; private set; }
    public string CustomerName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public ClientType ClientType { get; private set; }
    public string? CompanyName { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }

    public ICollection<Project> Projects { get; private set; } = [];

    public static Result<Customer> Create(
        Guid userId,
        string customerName,
        string email,
        string phone,
        ClientType clientType,
        string? companyName = null,
        string? notes = null)
    {
        var validation = Validate(userId, customerName, email, phone);
        if (validation.IsError)
            return validation.Errors;

        return new Customer(
            Guid.CreateVersion7(),
            userId,
            customerName.Trim(),
            email.Trim(),
            phone.Trim(),
            clientType,
            NormalizeOptional(companyName),
            NormalizeOptional(notes));
    }

    public Result<Success> Update(
        string customerName,
        string email,
        string phone,
        ClientType clientType,
        string? companyName,
        string? notes)
    {
        var validation = Validate(UserId, customerName, email, phone);
        if (validation.IsError)
            return validation;

        CustomerName = customerName.Trim();
        Email = email.Trim();
        Phone = phone.Trim();
        ClientType = clientType;
        CompanyName = NormalizeOptional(companyName);
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

    private static Result<Success> Validate(
        Guid userId,
        string customerName,
        string email,
        string phone)
    {
        if (userId == Guid.Empty)
            return CustomerErrors.InvalidUserId;

        if (string.IsNullOrWhiteSpace(customerName))
            return CustomerErrors.CustomerNameRequired;

        if (string.IsNullOrWhiteSpace(email))
            return CustomerErrors.EmailRequired;

        if (string.IsNullOrWhiteSpace(phone))
            return CustomerErrors.PhoneRequired;

        return Result.Success;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
