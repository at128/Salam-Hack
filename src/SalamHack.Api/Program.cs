using SalamHack.Api;
using SalamHack.Application;
using SalamHack.Domain.Common.Constants;
using SalamHack.Infrastructure;
using SalamHack.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

ConfigureHostLogging(builder);
ValidateProductionSecrets(builder);
AddApplicationServices(builder);
AddHealthChecks(builder);

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    await VerifyDatabaseAsync(app);
}

app.UseCoreMiddlewares();
app.MapControllers();
MapHealthEndpoints(app);

Log.Information("Starting SalamHack API...");
app.Run();

static void ConfigureHostLogging(WebApplicationBuilder builder)
{
    builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));
}

static void AddApplicationServices(WebApplicationBuilder builder)
{
    builder.Services
        .AddPresentation(builder.Configuration)
        .AddApplication()
        .AddInfrastructure(builder.Configuration);
}

static void AddHealthChecks(WebApplicationBuilder builder)
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        builder.Services.AddHealthChecks()
            .AddSqlServer(
                connectionString,
                name: "database",
                timeout: TimeSpan.FromSeconds(3));
    }
}

static async Task VerifyDatabaseAsync(WebApplication app)
{
    if (!app.Environment.IsDevelopment())
        return;

    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
}

static void MapHealthEndpoints(WebApplication app)
{
    var allowAnonymousHealthEndpoints =
        app.Configuration.GetValue<bool>("Monitoring:AllowAnonymousHealthEndpoints");

    var liveEndpoint = app.MapHealthChecks("/api/v1/health/live", new HealthCheckOptions
    {
        Predicate = _ => false,
        AllowCachingResponses = false
    });

    var readyEndpoint = app.MapHealthChecks("/api/v1/health/ready", new HealthCheckOptions
    {
        Predicate = _ => true,
        AllowCachingResponses = false
    });

    if (allowAnonymousHealthEndpoints)
    {
        liveEndpoint.AllowAnonymous();
        readyEndpoint.AllowAnonymous();
        return;
    }

    var adminOnly = new AuthorizeAttribute
    {
        Roles = ApplicationConstants.Roles.Admin
    };

    liveEndpoint.RequireAuthorization(adminOnly);
    readyEndpoint.RequireAuthorization(adminOnly);
}

static void ValidateProductionSecrets(WebApplicationBuilder builder)
{
    if (!builder.Environment.IsProduction())
        return;

    var failures = new List<string>();
    ValidateSecret(builder.Configuration, "JWT:Secret", minLength: 32, failures);

    if (failures.Count == 0)
        return;

    throw new InvalidOperationException(
        "Production secret validation failed: " + string.Join(" | ", failures));
}

static void ValidateSecret(
    IConfiguration configuration,
    string key,
    int minLength,
    List<string> failures)
{
    var value = configuration[key];

    if (string.IsNullOrWhiteSpace(value) || value.Length < minLength)
    {
        failures.Add($"{key} is missing or too short.");
        return;
    }

    if (LooksLikePlaceholder(value))
        failures.Add($"{key} looks like a placeholder and must be replaced.");
}

static bool LooksLikePlaceholder(string value)
{
    var normalized = value.Trim().ToLowerInvariant();

    return normalized.Contains("changeme", StringComparison.Ordinal)
        || normalized.Contains("replace_me", StringComparison.Ordinal)
        || normalized.Contains("placeholder", StringComparison.Ordinal)
        || normalized.Contains("dummy", StringComparison.Ordinal)
        || normalized.Contains("example", StringComparison.Ordinal);
}

public partial class Program;
