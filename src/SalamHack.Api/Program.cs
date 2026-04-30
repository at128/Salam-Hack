using SalamHack.Api;
using SalamHack.Application;
using SalamHack.Domain.Common.Constants;
using SalamHack.Infrastructure;
using SalamHack.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Serilog;
using System.Data.Common;
using System.Reflection;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

ConfigureHostLogging(builder);
ValidateProductionSecrets(builder);
AddApplicationServices(builder);
AddHealthChecks(builder);

// Auto-migrate on startup in non-development environments, or when explicitly
// enabled for local Docker development.
// Using a BackgroundService ensures this never runs during EF design-time
// tool invocations (dotnet ef / PMC), which would cause double-migration errors.
var migrationsEnabled = builder.Configuration.GetValue<bool>("DatabaseMigration:Enabled");
if ((migrationsEnabled || !builder.Environment.IsDevelopment()) && !builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHostedService<DatabaseMigrationService>();
}

var app = builder.Build();

app.UseCoreMiddlewares();
app.MapControllers();
app.MapHub<SalamHack.Infrastructure.Notifications.Hubs.NotificationHub>("/hubs/notifications");
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

    builder.Services.AddSignalR();
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

internal sealed class DatabaseMigrationService(IServiceProvider services) : BackgroundService
{
    private static readonly string[] LegacyMigrationIds =
    [
        "20260425155110_InitialIdentitySchema",
        "20260425182653_AddRefreshTokens",
        "20260427151744_AddDomainEntities",
        "20260428190000_AddInvoiceUserScopedNumbers",
        "20260429120000_AddExpenseSoftDelete",
        "20260429160226_ExpandProjectProfitMarginPrecision"
    ];

    private static readonly string[] RequiredSchemaTables =
    [
        "AspNetRoles",
        "AspNetUsers",
        "customers",
        "services",
        "projects",
        "invoices",
        "expenses",
        "payments",
        "analyses",
        "notifications",
        "refresh_tokens"
    ];

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await ApplyLegacyCompatibilityAsync(scope.ServiceProvider, context, ct);
        await context.Database.MigrateAsync(ct);
    }

    private static async Task ApplyLegacyCompatibilityAsync(
        IServiceProvider serviceProvider,
        AppDbContext context,
        CancellationToken ct)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        if (!configuration.GetValue<bool>("DatabaseMigration:LegacyCompatibilityEnabled"))
            return;

        if (!await context.Database.CanConnectAsync(ct))
            return;

        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync(ct);
        var currentMigrationId = context.Database.GetMigrations().LastOrDefault();
        if (currentMigrationId is null || appliedMigrations.Contains(currentMigrationId, StringComparer.Ordinal))
            return;

        if (!appliedMigrations.Any(id => LegacyMigrationIds.Contains(id, StringComparer.Ordinal)))
            return;

        var scriptPath = configuration["DatabaseMigration:LegacyScriptPath"] ?? "deploy.sql";
        var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
        var resolvedScriptPath = Path.IsPathRooted(scriptPath)
            ? scriptPath
            : Path.Combine(environment.ContentRootPath, scriptPath);

        if (!File.Exists(resolvedScriptPath))
            throw new FileNotFoundException("Legacy database compatibility script was not found.", resolvedScriptPath);

        var script = await File.ReadAllTextAsync(resolvedScriptPath, ct);
        await ExecuteSqlScriptAsync(context, script, ct);

        await EnsureCurrentMigrationRecordedAsync(context, currentMigrationId, ct);
    }

    private static async Task ExecuteSqlScriptAsync(
        AppDbContext context,
        string script,
        CancellationToken ct)
    {
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;
        if (shouldClose)
            await connection.OpenAsync(ct);

        try
        {
            var batches = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
                .Select(batch => batch.Trim())
                .Where(batch => !string.IsNullOrWhiteSpace(batch));

            foreach (var batch in batches)
            {
                await using var command = connection.CreateCommand();
                command.CommandText = """
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;
""" + Environment.NewLine + batch;
                command.CommandTimeout = 120;
                await command.ExecuteNonQueryAsync(ct);
            }
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }

    private static async Task EnsureCurrentMigrationRecordedAsync(
        AppDbContext context,
        string currentMigrationId,
        CancellationToken ct)
    {
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != System.Data.ConnectionState.Open;
        if (shouldClose)
            await connection.OpenAsync(ct);

        try
        {
            var missingTables = new List<string>();
            foreach (var table in RequiredSchemaTables)
            {
                if (!await TableExistsAsync(connection, table, ct))
                    missingTables.Add(table);
            }

            if (missingTables.Count > 0)
            {
                throw new InvalidOperationException(
                    "Legacy database compatibility script did not create all required tables: "
                    + string.Join(", ", missingTables));
            }

            var productVersion = typeof(Migration).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion
                .Split('+')[0] ?? "9.0.0";

            await using var command = connection.CreateCommand();
            command.CommandText = """
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = @migrationId)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (@migrationId, @productVersion);
END;
""";
            AddParameter(command, "@migrationId", currentMigrationId);
            AddParameter(command, "@productVersion", productVersion);
            await command.ExecuteNonQueryAsync(ct);
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }

    private static async Task<bool> TableExistsAsync(
        DbConnection connection,
        string tableName,
        CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1 WHERE OBJECT_ID(@tableName, 'U') IS NOT NULL;";
        AddParameter(command, "@tableName", tableName);
        var result = await command.ExecuteScalarAsync(ct);

        return result is not null;
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
