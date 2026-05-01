using Microsoft.AspNetCore.Identity;

namespace SalamHack.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? BankName { get; set; }
    public string? BankAccountName { get; set; }
    public string? BankIban { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }
}
