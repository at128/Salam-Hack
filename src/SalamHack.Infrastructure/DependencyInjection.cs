using System.Text;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Settings;
using SalamHack.Infrastructure.Analytics;
using SalamHack.Infrastructure.BackgroundJobs;
using SalamHack.Infrastructure.Caching;
using SalamHack.Infrastructure.Data;
using SalamHack.Infrastructure.Data.Interceptors;
using SalamHack.Infrastructure.Email;
using SalamHack.Infrastructure.Identity;
using SalamHack.Infrastructure.Invoices;
using SalamHack.Infrastructure.Notifications;
using SalamHack.Infrastructure.Reports;
using SalamHack.Infrastructure.Settings;
using SalamHack.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace SalamHack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);

        var (jwtSection, jwtSecret) = GetAndValidateJwtConfiguration(configuration);
        services.Configure<JwtSettings>(jwtSection);

        services.AddPersistence(configuration);
        services.AddIdentityServices();
        services.AddJwtAuthentication(jwtSection, jwtSecret);
        services.AddAuthorization();

        services.AddScoped<ITokenProvider, TokenProvider>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IEmailVerificationService, EmailVerificationService>();
        services.AddScoped<IServiceHistoryAnalyzer, ServiceHistoryAnalyzer>();
        services.AddScoped<IProjectAnalysisAiClient, OpenAiProjectAnalysisClient>();
        services.AddScoped<IExpenseClassifier, RuleBasedExpenseClassifier>();
        services.AddScoped<IInvoicePdfRenderer, InvoicePdfRenderer>();
        services.AddScoped<IExpenseReceiptStorage, FileSystemExpenseReceiptStorage>();
        services.AddScoped<INotificationDeliveryService, InAppNotificationDeliveryService>();
        services.AddScoped<IReportExporter, ReportExporter>();

        services.AddHybridCache();
        services.AddScoped<ICacheInvalidator, CacheInvalidator>();

        services.AddAdminBootstrap(configuration);
        services.AddRefreshToken(configuration);
        services.AddExpenseReceiptStorage(configuration);
        services.AddProjectAnalysisAi(configuration);
        services.AddEmailVerification(configuration);

        return services;
    }

    private static (IConfigurationSection JwtSection, string Secret) GetAndValidateJwtConfiguration(
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("JWT");
        var secret = jwtSection["Secret"];
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var expiryMinutes = jwtSection.GetValue<int?>("ExpiryMinutes");

        if (string.IsNullOrWhiteSpace(secret) || secret.Length < 32)
            throw new InvalidOperationException("JWT:Secret must be configured and at least 32 characters.");

        if (string.IsNullOrWhiteSpace(issuer))
            throw new InvalidOperationException("JWT:Issuer must be configured.");

        if (string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException("JWT:Audience must be configured.");

        if (expiryMinutes is not > 0)
            throw new InvalidOperationException("JWT:ExpiryMinutes must be configured and greater than 0.");

        return (jwtSection, secret);
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }

    private static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;

            options.User.RequireUniqueEmail = true;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        services.AddHostedService<AdminBootstrapService>();

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfigurationSection jwtSection,
        string jwtSecret)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidAudience = jwtSection["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }

    private static IServiceCollection AddAdminBootstrap(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<AdminBootstrapSettings>()
            .Bind(configuration.GetSection(AdminBootstrapSettings.SectionName))
            .Validate(s => !s.Enabled || !string.IsNullOrWhiteSpace(s.Email),
                $"{AdminBootstrapSettings.SectionName}:Email is required when Enabled=true.")
            .Validate(s => !s.Enabled || !string.IsNullOrWhiteSpace(s.Password),
                $"{AdminBootstrapSettings.SectionName}:Password is required when Enabled=true.")
            .Validate(s => !s.Enabled || s.Password.Length >= 8,
                $"{AdminBootstrapSettings.SectionName}:Password must be at least 8 characters when Enabled=true.")
            .ValidateOnStart();

        return services;
    }

    private static IServiceCollection AddRefreshToken(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddOptions<RefreshTokenSettings>()
            .Bind(configuration.GetSection("RefreshToken"))
            .Validate(x => x.ExpiryDays is > 0 and <= 90,
                "RefreshToken:ExpiryDays must be between 1 and 90.")
            .Validate(x => x.TokenBytes is >= 32 and <= 128,
                "RefreshToken:TokenBytes must be between 32 and 128.")
            .ValidateOnStart();

        services.AddHostedService<RefreshTokenCleanupService>();

        return services;
    }

    private static IServiceCollection AddExpenseReceiptStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<ExpenseReceiptStorageSettings>()
            .Bind(configuration.GetSection(ExpenseReceiptStorageSettings.SectionName))
            .Validate(s => !string.IsNullOrWhiteSpace(s.RootPath),
                $"{ExpenseReceiptStorageSettings.SectionName}:RootPath is required.")
            .ValidateOnStart();

        return services;
    }

    private static IServiceCollection AddProjectAnalysisAi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<ProjectAnalysisAiSettings>()
            .Bind(configuration.GetSection(ProjectAnalysisAiSettings.SectionName))
            .Validate(s => !s.Enabled || !string.IsNullOrWhiteSpace(s.ApiKey),
                $"{ProjectAnalysisAiSettings.SectionName}:ApiKey is required when Enabled=true.")
            .Validate(s => !s.Enabled || !string.IsNullOrWhiteSpace(s.Endpoint),
                $"{ProjectAnalysisAiSettings.SectionName}:Endpoint is required when Enabled=true.")
            .Validate(s => !s.Enabled || !string.IsNullOrWhiteSpace(s.Model),
                $"{ProjectAnalysisAiSettings.SectionName}:Model is required when Enabled=true.")
            .Validate(s => s.TimeoutSeconds is >= 5 and <= 120,
                $"{ProjectAnalysisAiSettings.SectionName}:TimeoutSeconds must be between 5 and 120.")
            .Validate(s => s.Temperature is >= 0 and <= 2,
                $"{ProjectAnalysisAiSettings.SectionName}:Temperature must be between 0 and 2.");

        return services;
    }

    private static IServiceCollection AddEmailVerification(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<EmailVerificationSettings>()
            .Bind(configuration.GetSection(EmailVerificationSettings.SectionName))
            .Validate(s => s.OtpLength is >= 4 and <= 8,
                $"{EmailVerificationSettings.SectionName}:OtpLength must be between 4 and 8.")
            .Validate(s => s.ExpiryMinutes is >= 1 and <= 60,
                $"{EmailVerificationSettings.SectionName}:ExpiryMinutes must be between 1 and 60.")
            .Validate(s => s.MaxAttempts is >= 1 and <= 10,
                $"{EmailVerificationSettings.SectionName}:MaxAttempts must be between 1 and 10.");

        services.AddOptions<MailSettings>()
            .Bind(configuration.GetSection(MailSettings.SectionName))
            .Validate(s => s.Port is > 0 and <= 65535,
                $"{MailSettings.SectionName}:Port must be a valid TCP port.");

        return services;
    }
}
