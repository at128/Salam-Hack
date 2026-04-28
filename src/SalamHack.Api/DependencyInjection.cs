using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.RateLimiting;
using SalamHack.Api.Infrastructure;
using SalamHack.Api.Services;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Infrastructure.Settings;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using ForwardedIpNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

namespace SalamHack.Api;

public static class DependencyInjection
{
    private const string FrontendCorsPolicy = "Frontend";
    private const string AuthRateLimitPolicy = "auth";
    private const string AuthRefreshRateLimitPolicy = "auth-refresh";
    private const string PublicReadRateLimitPolicy = "public-read";
    private const string UserReadRateLimitPolicy = "user-read";
    private const string UserWriteRateLimitPolicy = "user-write";
    private const string AdminRateLimitPolicy = "admin";
    private const string WebhooksRateLimitPolicy = "webhooks";

    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPresentationCore();
        services.AddApiVersioningServices();
        services.AddSwaggerDocumentation();
        services.AddObservability(configuration);
        services.AddRateLimitingPolicies(configuration);
        services.AddForwardedHeadersSupport(configuration);
        services.AddCorsPolicy(configuration);
        services.AddCookieSettings(configuration);
        services.AddDataProtectionServices(configuration);

        services.Configure<CookieSettings>(configuration.GetSection("CookieSettings"));

        return services;
    }

    public static WebApplication UseCoreMiddlewares(this WebApplication app)
    {
        app.UseDiagnosticsAndErrorHandling();
        app.UseSwaggerAndHsts();
        app.UseHttpSecurityPipeline();
        app.UseStaticFiles();

        if (app.Configuration.GetValue<bool>("OpenTelemetry:Enabled"))
            app.MapPrometheusScrapingEndpoint();

        return app;
    }

    private static IServiceCollection AddPresentationCore(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddHttpContextAccessor();
        services.AddHttpClient();
        services.AddScoped<IUser, CurrentUser>();
        services.AddScoped<ICookieService, CookieService>();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddMemoryCache();

        return services;
    }

    private static IServiceCollection AddApiVersioningServices(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new Asp.Versioning.UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    private static IServiceCollection AddRateLimitingPolicies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var globalTokenLimit = GetPositiveIntOrDefault(configuration, "RateLimiting:Global:TokenLimit", 300);
        var globalTokensPerPeriod = GetPositiveIntOrDefault(configuration, "RateLimiting:Global:TokensPerPeriod", 300);
        var globalReplenishmentSeconds = GetPositiveIntOrDefault(configuration, "RateLimiting:Global:ReplenishmentPeriodSeconds", 60);
        var authPermitLimit = GetPositiveIntOrDefault(configuration, "RateLimiting:Auth:PermitLimit", 8);
        var authWindowSeconds = GetPositiveIntOrDefault(configuration, "RateLimiting:Auth:WindowSeconds", 60);
        var authRefreshPermitLimit = GetPositiveIntOrDefault(configuration, "RateLimiting:AuthRefresh:PermitLimit", 30);
        var authRefreshWindowSeconds = GetPositiveIntOrDefault(configuration, "RateLimiting:AuthRefresh:WindowSeconds", 60);
        var publicReadTokenLimit = GetPositiveIntOrDefault(configuration, "RateLimiting:PublicRead:TokenLimit", 600);
        var publicReadTokensPerPeriod = GetPositiveIntOrDefault(configuration, "RateLimiting:PublicRead:TokensPerPeriod", 600);
        var publicReadReplenishmentSeconds = GetPositiveIntOrDefault(configuration, "RateLimiting:PublicRead:ReplenishmentPeriodSeconds", 60);
        var userReadTokenLimit = GetPositiveIntOrDefault(configuration, "RateLimiting:UserRead:TokenLimit", 900);
        var userReadTokensPerPeriod = GetPositiveIntOrDefault(configuration, "RateLimiting:UserRead:TokensPerPeriod", 900);
        var userReadReplenishmentSeconds = GetPositiveIntOrDefault(configuration, "RateLimiting:UserRead:ReplenishmentPeriodSeconds", 60);
        var userWritePermitLimit = GetPositiveIntOrDefault(configuration, "RateLimiting:UserWrite:PermitLimit", 120);
        var userWriteWindowSeconds = GetPositiveIntOrDefault(configuration, "RateLimiting:UserWrite:WindowSeconds", 60);
        var adminPermitLimit = GetPositiveIntOrDefault(configuration, "RateLimiting:Admin:PermitLimit", 300);
        var adminWindowSeconds = GetPositiveIntOrDefault(configuration, "RateLimiting:Admin:WindowSeconds", 60);
        var webhooksPermitLimit = GetPositiveIntOrDefault(configuration, "RateLimiting:Webhooks:PermitLimit", 60);
        var webhooksWindowSeconds = GetPositiveIntOrDefault(configuration, "RateLimiting:Webhooks:WindowSeconds", 60);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, token) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    var seconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));
                    context.HttpContext.Response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
                }

                if (!context.HttpContext.Response.HasStarted)
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsync(
                        "{\"error\":\"rate_limited\",\"message\":\"Too many requests. Please retry later.\"}",
                        token);
                }
            };

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var ip = GetClientIp(httpContext);

                return RateLimitPartition.GetTokenBucketLimiter(ip, _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = globalTokenLimit,
                    TokensPerPeriod = globalTokensPerPeriod,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(globalReplenishmentSeconds),
                    AutoReplenishment = true,
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
            });

            options.AddPolicy(AuthRateLimitPolicy, httpContext =>
            {
                var ip = GetClientIp(httpContext);

                return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = authPermitLimit,
                    Window = TimeSpan.FromSeconds(authWindowSeconds),
                    AutoReplenishment = true,
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
            });

            options.AddPolicy(AuthRefreshRateLimitPolicy, httpContext =>
            {
                var ip = GetClientIp(httpContext);

                return RateLimitPartition.GetFixedWindowLimiter($"auth-refresh:{ip}", _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = authRefreshPermitLimit,
                    Window = TimeSpan.FromSeconds(authRefreshWindowSeconds),
                    AutoReplenishment = true,
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
            });

            options.AddPolicy(PublicReadRateLimitPolicy, httpContext =>
            {
                var ip = GetClientIp(httpContext);

                return RateLimitPartition.GetTokenBucketLimiter($"public-read:{ip}", _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = publicReadTokenLimit,
                    TokensPerPeriod = publicReadTokensPerPeriod,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(publicReadReplenishmentSeconds),
                    AutoReplenishment = true,
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
            });

            options.AddPolicy(UserReadRateLimitPolicy, httpContext =>
            {
                var key = GetUserOrIpKey(httpContext);

                return RateLimitPartition.GetTokenBucketLimiter($"user-read:{key}", _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = userReadTokenLimit,
                    TokensPerPeriod = userReadTokensPerPeriod,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(userReadReplenishmentSeconds),
                    AutoReplenishment = true,
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
            });

            options.AddPolicy(UserWriteRateLimitPolicy, httpContext =>
            {
                var key = GetUserOrIpKey(httpContext);

                return RateLimitPartition.GetFixedWindowLimiter($"user-write:{key}", _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = userWritePermitLimit,
                    Window = TimeSpan.FromSeconds(userWriteWindowSeconds),
                    AutoReplenishment = true,
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
            });

            options.AddPolicy(AdminRateLimitPolicy, httpContext =>
            {
                var key = GetUserOrIpKey(httpContext);

                return RateLimitPartition.GetFixedWindowLimiter($"admin:{key}", _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = adminPermitLimit,
                    Window = TimeSpan.FromSeconds(adminWindowSeconds),
                    AutoReplenishment = true,
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
            });

            options.AddPolicy(WebhooksRateLimitPolicy, httpContext =>
            {
                var ip = GetClientIp(httpContext);

                return RateLimitPartition.GetFixedWindowLimiter($"webhook:{ip}", _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = webhooksPermitLimit,
                    Window = TimeSpan.FromSeconds(webhooksWindowSeconds),
                    AutoReplenishment = true,
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
            });
        });

        return services;
    }

    private static int GetPositiveIntOrDefault(IConfiguration configuration, string key, int defaultValue)
    {
        var configuredValue = configuration.GetValue<int?>(key);
        return configuredValue is > 0 ? configuredValue.Value : defaultValue;
    }

    private static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(FrontendCorsPolicy, policy =>
            {
                var allowedOrigins = configuration
                    .GetSection("Cors:AllowedOrigins")
                    .Get<string[]>() ?? [];

                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
            });
        });

        return services;
    }

    private static IServiceCollection AddForwardedHeadersSupport(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            var forwardLimit = configuration.GetValue<int?>("ForwardedHeaders:ForwardLimit");
            if (forwardLimit is > 0)
                options.ForwardLimit = forwardLimit.Value;

            var knownProxies = configuration
                .GetSection("ForwardedHeaders:KnownProxies")
                .Get<string[]>() ?? [];

            foreach (var proxy in knownProxies)
            {
                if (IPAddress.TryParse(proxy, out var parsed))
                    options.KnownProxies.Add(parsed);
            }

            var knownNetworks = configuration
                .GetSection("ForwardedHeaders:KnownNetworks")
                .Get<string[]>() ?? [];

            foreach (var network in knownNetworks)
            {
                if (TryParseIpNetwork(network, out var parsed))
                    options.KnownNetworks.Add(parsed);
            }
        });

        return services;
    }

    private static bool TryParseIpNetwork(string? value, out ForwardedIpNetwork network)
    {
        network = default!;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        var parts = value.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
            return false;

        if (!IPAddress.TryParse(parts[0], out var address))
            return false;

        if (!int.TryParse(parts[1], out var prefixLength))
            return false;

        var maxPrefixLength = address.AddressFamily == AddressFamily.InterNetwork ? 32 : 128;
        if (prefixLength < 0 || prefixLength > maxPrefixLength)
            return false;

        network = new ForwardedIpNetwork(address, prefixLength);
        return true;
    }

    private static string GetClientIp(HttpContext httpContext)
        => httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static string GetUserOrIpKey(HttpContext httpContext)
    {
        var userId = httpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrWhiteSpace(userId)
            ? $"user:{userId}"
            : $"ip:{GetClientIp(httpContext)}";
    }

    private static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "SalamHack API",
                Version = "v1",
                Description = "Clean Architecture RESTful API"
            });

            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "Enter your JWT token"
            });

            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            if (File.Exists(xmlPath))
                c.IncludeXmlComments(xmlPath);
        });

        return services;
    }

    private static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (!configuration.GetValue<bool>("OpenTelemetry:Enabled"))
            return services;

        var serviceName = configuration["OpenTelemetry:ServiceName"]
            ?? typeof(DependencyInjection).Assembly.GetName().Name
            ?? "SalamHack.Api";

        var serviceVersion = configuration["OpenTelemetry:ServiceVersion"]
            ?? typeof(DependencyInjection).Assembly.GetName().Version?.ToString()
            ?? "1.0.0";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion)
                .AddAttributes([
                    new KeyValuePair<string, object>(
                        "deployment.environment",
                        configuration["ASPNETCORE_ENVIRONMENT"] ?? "Unknown")
                ]))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];
                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddPrometheusExporter();

                var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];
                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    metrics.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
                }
            });

        return services;
    }

    private static WebApplication UseDiagnosticsAndErrorHandling(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseSerilogRequestLogging();
        app.UseExceptionHandler();

        return app;
    }

    private static WebApplication UseSwaggerAndHsts(this WebApplication app)
    {
        var swaggerEnabled = app.Configuration.GetValue<bool>("Swagger:Enabled");

        if (app.Environment.IsDevelopment() || swaggerEnabled)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        if (!app.Environment.IsDevelopment())
            app.UseHsts();

        return app;
    }

    private static WebApplication UseHttpSecurityPipeline(this WebApplication app)
    {
        app.UseForwardedHeaders();
        app.UseHttpsRedirection();
        app.UseCors(FrontendCorsPolicy);
        app.UseAuthentication();
        app.UseRateLimiter();
        app.UseAuthorization();

        return app;
    }

    private static IServiceCollection AddCookieSettings(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<CookieSettings>, CookieSettingsValidator>();

        services.AddOptions<CookieSettings>()
            .Bind(configuration.GetSection("CookieSettings"))
            .ValidateOnStart();

        return services;
    }

    private static IServiceCollection AddDataProtectionServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var builder = services.AddDataProtection()
            .SetApplicationName(configuration["DataProtection:ApplicationName"] ?? "SalamHack");

        var keysPath = configuration["DataProtection:KeysPath"];
        if (!string.IsNullOrWhiteSpace(keysPath))
            builder.PersistKeysToFileSystem(new DirectoryInfo(keysPath));

        return services;
    }
}
