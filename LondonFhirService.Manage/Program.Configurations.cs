// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

#nullable enable annotations

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using Attrify.Extensions;
using Attrify.InvisibleApi.Models;
using Hl7.Fhir.Serialization;
using ISL.Security.Client.Models.Clients;
using LondonFhirService.Clients.AuditAndMetrics.Clients;
using LondonFhirService.Clients.AuditAndMetrics.Models.Configurations;
using LondonFhirService.Core.Abstractions.Brokers;
using LondonFhirService.Core.Abstractions.Models;
using LondonFhirService.Core.Brokers.AuditAndMetrics;
using LondonFhirService.Core.Brokers.CsvHelpers;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.Audits;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Services.Foundations.Audits;
using LondonFhirService.Core.Services.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Services.Foundations.FhirRecords;
using LondonFhirService.Core.Services.Foundations.Metrics;
using LondonFhirService.Core.Services.Foundations.Providers;
using LondonFhirService.Core.Services.Orchestrations.FhirReconciliations.STU3;
using LondonFhirService.Core.Services.Orchestrations.Metrics;
using LondonFhirService.Core.Services.Processings.Metrics;
using LondonFhirService.Manage.Brokers.Https;
using LondonFhirService.Manage.Models.Foundations.Patients;
using LondonFhirService.Manage.Services.Foundations.Patients;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LondonFhirService.Core.Workers;
// Aliased rather than imported whole: Microsoft.ApplicationInsights.Metric collides with
// this solution's own Metric entity, which this file also registers.
using TelemetryClient = Microsoft.ApplicationInsights.TelemetryClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

public partial class Program
{
    internal static Action<WebApplicationBuilder>? TestConfigurationOverrides { get; set; } = null;
    internal static bool ExcludeAppInsightsForTesting { get; set; } = false;

    internal static void ConfigurationOverridesForTesting(WebApplicationBuilder builder)
    {
        TestConfigurationOverrides?.Invoke(builder);
    }

    internal static void ConfigureApplicationInsightsTelemetry(WebApplicationBuilder builder)
    {
        if (ExcludeAppInsightsForTesting == false)
        {
            builder.Services.AddApplicationInsightsTelemetry();
        }
    }

    internal static void ConfigureServices(WebApplicationBuilder builder)
    {
        IConfiguration configuration = builder.Configuration;

        // ----------------- Authentication / Azure AD -----------------
        var azureAdSection = configuration.GetSection("AzureAd");

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(azureAdSection);

        var instance = configuration["AzureAd:Instance"];
        var tenantId = configuration["AzureAd:TenantId"];
        var scopes = configuration["AzureAd:Scopes"];

        var missingKeys = new List<string>();
        if (string.IsNullOrEmpty(instance)) missingKeys.Add("Instance");
        if (string.IsNullOrEmpty(tenantId)) missingKeys.Add("TenantId");
        if (string.IsNullOrEmpty(scopes)) missingKeys.Add("Scopes");

        if (missingKeys.Count > 0)
        {
            throw new InvalidOperationException(
                $"AzureAd configuration is incomplete. Missing keys: {string.Join(", ", missingKeys)}. " +
                "Please check appsettings.json.");
        }

        // ----------------- Core ASP.NET services -----------------
        builder.Services.AddSwaggerGen();
        builder.Services.AddAuthorization();
        builder.Services.AddDbContextFactory<StorageBroker>();
        builder.Services.AddDbContext<StorageBroker>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddEndpointsApiExplorer();

        // ----------------- Domain registrations -----------------
        AddConfigurations(builder.Services, configuration);
        AddProviders(builder.Services, configuration);
        AddBrokers(builder.Services, configuration);
        AddFoundationServices(builder.Services);
        AddOrchestrationServices(builder.Services, configuration);
        AddProcessingServices(builder.Services);
        AddCoordinationServices(builder.Services, configuration);
        AddClients(builder.Services, builder.Configuration);
        AddBackgroundWorkers(builder.Services);

        // IConfiguration registration (optional, but mirrors original)
        builder.Services.AddSingleton<IConfiguration>(configuration);

        JsonNamingPolicy jsonNamingPolicy = JsonNamingPolicy.CamelCase;

        builder.Services
            .AddControllers()
            .AddOData(options =>
            {
                options.AddRouteComponents("odata", GetEdmModel());
                options.Select().Filter().Expand().OrderBy().Count().SetMaxTop(100);
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ForFhir(Hl7.Fhir.Model.ModelInfo.ModelInspector);

                options.JsonSerializerOptions.PropertyNamingPolicy = jsonNamingPolicy;
                options.JsonSerializerOptions.DictionaryKeyPolicy = jsonNamingPolicy;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.WriteIndented = true;
            });
    }

    internal static void ConfigurePipeline(WebApplication app)
    {
        // Resolve InvisibleApiKey from DI
        var invisibleApiKey = app.Services.GetRequiredService<InvisibleApiKey>();

        app.MapGet("/status", () => Results.Ok(new
        {
            Name = "London FHIR Service Management Portal",
            Version = "1.0",
            Status = "Running"
        }));

        app.MapHealthChecks("/health");               // Basic liveness check
        app.MapHealthChecks("/health/ready");         // Readiness endpoint if needed
        app.UseDefaultFiles();
        app.UseStaticFiles();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(configuration =>
            {
                configuration.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");

                // Configure OAuth2 for Swagger UI
                configuration.OAuthClientId(app.Configuration["AzureAd:ClientId"]); // Use the application ClientId
                configuration.OAuthClientSecret("");
                configuration.OAuthUsePkce(); // Enable PKCE (Proof Key for Code Exchange)
                configuration.OAuthScopes(app.Configuration["AzureAd:Scopes"]); // Add required scopes
            });
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseInvisibleApiMiddleware(invisibleApiKey);
        app.MapControllers();
        app.MapFallbackToFile("/index.html");
    }

    private static IEdmModel GetEdmModel()
    {
        ODataConventionModelBuilder builder = new();
        builder.EntitySet<Audit>("Audits");
        // RequestSpanId is transport only - it carries the request's span id from the request
        // that produced a metric to the worker that replays it into telemetry, and EF ignores it
        // rather than giving it a column. The convention builder reflects over every public
        // property though, so without this it would land in the EDM: $metadata would advertise a
        // field that is always null, and $filter or $orderby against it would reach EF with no
        // column to translate to and fail the request rather than return nothing.
        builder.EntitySet<Metric>("Metrics")
            .EntityType
                .Ignore(metric => metric.RequestSpanId);
        builder.EntitySet<Provider>("Providers");
        builder.EntitySet<FhirRecord>("FhirRecords");
        builder.EntitySet<FhirRecordDifference>("FhirRecordDifferences");
        builder.EnableLowerCamelCase();
        return builder.GetEdmModel();
    }

    private const string StartupLoggerCategory = "LondonFhirService.Manage.Startup";

    private const string ConfigurationHint =
        "Please check appsettings.json or the corresponding environment variables/secrets.";

    /// <summary>
    /// Absolute on its own is not enough: Uri.TryCreate reads "localhost:7284/x" as an absolute
    /// uri whose scheme is localhost, so dropping the https from a setting would pass. Naming the
    /// two schemes HttpClient can dial is what makes the check true.
    /// </summary>
    private static bool IsDialableUrl(string? url) =>
        string.IsNullOrWhiteSpace(url) is false
        && Uri.TryCreate(url.Trim(), UriKind.Absolute, out Uri? parsedUrl)
        && (parsedUrl.Scheme == Uri.UriSchemeHttp || parsedUrl.Scheme == Uri.UriSchemeHttps);

    private static bool IsPlaceholder(string? value) =>
        value is not null && value.Contains("override_this", StringComparison.OrdinalIgnoreCase);

    private static bool IsUnset(string? value) =>
        string.IsNullOrWhiteSpace(value) || IsPlaceholder(value);

    /// <summary>
    /// What stops the structured record page working, found at startup rather than by the first
    /// operator to use it. Warnings, not failures: PatientConfiguration is allowed to be absent -
    /// the host must start without it, and PatientService answers that one screen with a 400
    /// naming the missing settings - so these only make the gap visible in the log.
    ///
    /// ClientId and ClientSecret ship blank on purpose. They are the fallback for what an operator
    /// types into the page, so only the shipped placeholder is a mistake there.
    /// </summary>
    internal static List<string> FindPatientConfigurationWarnings(PatientConfiguration patientConfiguration)
    {
        var warnings = new List<string>();

        if (IsDialableUrl(patientConfiguration.AuthUrl) is false)
        {
            warnings.Add("PatientConfiguration:AuthUrl is missing or is not an absolute http or https URI.");
        }

        if (IsDialableUrl(patientConfiguration.GetStructuredRecordUrl) is false)
        {
            warnings.Add(
                "PatientConfiguration:GetStructuredRecordUrl is missing or is not an absolute http or https URI.");
        }

        if (IsUnset(patientConfiguration.Scope))
        {
            warnings.Add("PatientConfiguration:Scope is not set.");
        }

        if (IsUnset(patientConfiguration.GrantType))
        {
            warnings.Add("PatientConfiguration:GrantType is not set.");
        }

        if (IsPlaceholder(patientConfiguration.ClientId))
        {
            warnings.Add("PatientConfiguration:ClientId still holds the shipped placeholder.");
        }

        if (IsPlaceholder(patientConfiguration.ClientSecret))
        {
            warnings.Add("PatientConfiguration:ClientSecret still holds the shipped placeholder.");
        }

        return warnings;
    }

    /// <summary>
    /// Run once the host is built, so its own logger - and every sink behind it - carries the
    /// result.
    /// </summary>
    internal static void VerifyStartupDependencies(WebApplication app)
    {
        ILogger logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(StartupLoggerCategory);

        List<string> warnings =
            FindPatientConfigurationWarnings(app.Services.GetRequiredService<PatientConfiguration>());

        foreach (string warning in warnings)
        {
            logger.LogWarning(
                "Structured record configuration: {ConfigurationWarning} The Get Structured Record "
                    + "page will not work until it is fixed. {ConfigurationHint}",
                warning,
                ConfigurationHint);
        }
    }

    /// <summary>
    /// A host that fails to start used to leave nothing behind but a process exit: the AzureAd
    /// guard and everything else in ConfigureServices throw before logging or Application Insights
    /// exist, so in Azure the only symptom was a generic "failed to start" page. This makes sure
    /// the reason is logged as critical wherever it can be seen.
    ///
    /// Locally it goes through the host's own logger when there is a host, and straight to the
    /// console when there is not. Application Insights is always given it through a client built
    /// from the connection string alone - even when the host exists, because its telemetry pipeline
    /// only starts with the host, and a host that failed before app.Run never started it.
    ///
    /// Nothing here may throw: it runs on the way out of a failure, and an exception of its own
    /// would replace the one that explains what went wrong.
    /// </summary>
    internal static async Task LogStartupFailureAsync(
        WebApplication? app,
        IConfiguration configuration,
        Exception exception)
    {
        const string message = "London FHIR Service Manage failed to start. {StartupFailureReason}";
        bool loggedByHost = false;

        if (app is not null)
        {
            try
            {
                app.Services
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger(StartupLoggerCategory)
                    .LogCritical(exception, message, exception.Message);

                loggedByHost = true;
            }
            catch (Exception)
            {
                // Fall back to the console below.
            }
        }

        if (loggedByHost is false)
        {
            try
            {
                // Disposed before moving on, because the console logger writes on a background
                // thread and the process is about to end.
                using ILoggerFactory loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());

                loggerFactory
                    .CreateLogger(StartupLoggerCategory)
                    .LogCritical(exception, message, exception.Message);
            }
            catch (Exception)
            {
                // Nothing further can be done without a console.
            }
        }

        TrackStartupFailureInApplicationInsights(configuration, exception);

        if (app is not null)
        {
            try
            {
                // Flushes the host's own logging providers before the process ends.
                await app.DisposeAsync();
            }
            catch (Exception)
            {
                // The failure has already been reported.
            }
        }
    }

    private static void TrackStartupFailureInApplicationInsights(
        IConfiguration configuration,
        Exception exception)
    {
        if (ExcludeAppInsightsForTesting)
        {
            return;
        }

        try
        {
            string? connectionString =
                configuration["ApplicationInsights:ConnectionString"]
                    ?? configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

            if (string.IsNullOrWhiteSpace(connectionString)
                || connectionString.Contains("InstrumentationKey=", StringComparison.OrdinalIgnoreCase) is false)
            {
                return;
            }

            // Fully qualified: importing Microsoft.ApplicationInsights would collide its Metric
            // with this solution's own Metric entity.
            using Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration telemetryConfiguration =
                Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.CreateDefault();

            telemetryConfiguration.ConnectionString = connectionString;
            var telemetryClient = new TelemetryClient(telemetryConfiguration);

            telemetryClient.TrackException(
                new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(exception)
                {
                    SeverityLevel = Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Critical,
                    Message = $"London FHIR Service Manage failed to start. {exception.Message}"
                });

            telemetryClient.Flush();
        }
        catch (Exception)
        {
            // A malformed connection string must not hide the failure being reported.
        }
    }

    /// <summary>
    /// Bound once here and registered as a singleton, rather than each reader taking IConfiguration
    /// and binding its own copy. PatientService then takes the configuration it needs instead of
    /// the host's whole configuration tree, and there is one instance behind every reader.
    ///
    /// A missing section binds to an all-null instance rather than throwing. That is deliberate:
    /// the host must still start without the structured record endpoints configured, and
    /// PatientService validates AuthUrl and GetStructuredRecordUrl per request, so an unconfigured
    /// deployment answers that one screen with a 400 naming the missing settings instead of
    /// refusing to boot.
    /// </summary>
    private static void AddConfigurations(IServiceCollection services, IConfiguration configuration)
    {
        PatientConfiguration patientConfiguration =
            configuration
                .GetSection(nameof(PatientConfiguration))
                .Get<PatientConfiguration>() ?? new PatientConfiguration();

        services.AddSingleton(patientConfiguration);
    }

    private static void AddProviders(IServiceCollection services, IConfiguration configuration)
    {
    }

    private static void AddBrokers(IServiceCollection services, IConfiguration configuration)
    {
        SecurityConfigurations securityConfigurations = new()
        {
            CreatedByPropertyName = nameof(IAuditable.CreatedBy),
            CreatedByPropertyType = typeof(string),
            CreatedWhenPropertyName = nameof(IAuditable.CreatedDate),
            CreatedWhenPropertyType = typeof(DateTimeOffset),
            UpdatedByPropertyName = nameof(IAuditable.UpdatedBy),
            UpdatedByPropertyType = typeof(string),
            UpdatedWhenPropertyName = nameof(IAuditable.UpdatedDate),
            UpdatedWhenPropertyType = typeof(DateTimeOffset),
            DeletedByPropertyName = "DeletedBy",
            DeletedByPropertyType = typeof(string),
            DeletedWhenPropertyName = "DeletedDate",
            DeletedWhenPropertyType = typeof(DateTimeOffset)
        };

        services.AddSingleton(securityConfigurations);
        services.AddTransient<IAuditAndMetricBroker, AuditAndMetricBroker>();
        services.AddScoped<IAuditAndMetricStorageBroker, AuditAndMetricStorageBroker>();
        services.AddScoped<IAuditUserBroker, AuditUserBroker>();
        services.AddTransient<IDateTimeBroker, DateTimeBroker>();
        services.AddTransient<IIdentifierBroker, IdentifierBroker>();
        services.AddTransient<ILoggingBroker, LoggingBroker>();
        services.AddTransient<ISecurityAuditBroker, SecurityAuditBroker>();
        services.AddTransient<ISecurityBroker, SecurityBroker>();
        services.AddSingleton<ICsvHelperBroker, CsvHelperBroker>();

        services.AddScoped<IStorageBroker>(
            serviceProvider => serviceProvider.GetRequiredService<StorageBroker>());

        services.AddScoped<IStorageBrokerFactory, StorageBrokerFactory>();

        // A typed client, so the outbound handler is pooled and rotated by the factory rather
        // than a socket being held open - or a fresh one burned - per structured record request.
        services.AddHttpClient<IHttpBroker, HttpBroker>(httpClient =>
        {
            // Longer than the endpoint this is pointed at is allowed to take. The Api host's
            // structured record endpoint carries a 130 second request timeout and fans out to
            // providers configured at 120, so HttpClient's 100 second default aborted calls the
            // Api would have completed - reported to the operator as a request that timed out,
            // about a lookup that was not in trouble. Slow providers are the normal case on the
            // screen this serves, so the client has to outlast the thing it waits for.
            httpClient.Timeout = TimeSpan.FromSeconds(150);

            // No MaxResponseContentBufferSize. It was set here, and measurement showed it doing
            // nothing: HttpBroker now sends with HttpCompletionOption.ResponseHeadersRead, so the
            // body is never buffered by HttpClient and the cap has nothing to apply to - a 70MB
            // response came back whole with it set to 64MB. Worse than useless, it would have been
            // misleading: a reader would take it for a limit that is not in force. A failed
            // response is bounded by the broker, which stops reading after 16KB; a successful one
            // is the payload the operator asked for and is theirs to have.
        });
    }

    private static void AddFoundationServices(IServiceCollection services)
    {
        services.AddTransient<IAuditService, AuditService>();
        services.AddTransient<IPatientService, PatientService>();
        services.AddTransient<IMetricService, MetricService>();
        services.AddTransient<IProviderService, ProviderService>();
        services.AddTransient<IFhirRecordService, FhirRecordService>();
        services.AddTransient<IFhirRecordDifferenceService, FhirRecordDifferenceService>();
    }

    private static void AddProcessingServices(IServiceCollection services)
    {
        services.AddTransient<IMetricProcessingService, MetricProcessingService>();
    }

    private static void AddOrchestrationServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IStu3FhirReconciliationService, Stu3FhirReconciliationService>();
        services.AddTransient<IMetricOrchestrationService, MetricOrchestrationService>();
    }

    private static void AddCoordinationServices(IServiceCollection services, IConfiguration configuration)
    {
    }

    private static void AddClients(IServiceCollection services, IConfiguration configuration)
    {
        // Bound once and registered, rather than bound here and again inside the client. The
        // client takes the configuration it needs instead of the host's whole IConfiguration,
        // so there is one instance behind every reader - and the binding itself still belongs to
        // the library, which owns the section name.
        AuditAndMetricsConfigurations auditAndMetricsConfigurations =
            AuditAndMetricsClient.BindConfigurations(configuration);

        services.AddSingleton(auditAndMetricsConfigurations);

        // Scoped, and it has to stay that way. SecurityAuditBroker captures the ClaimsPrincipal
        // in its constructor, so a singleton client would hold the first request's identity and
        // stamp every audit entry after that with the wrong user - silently. It would also
        // capture a DbContext past the scope that owns it.
        services.AddScoped<IAuditAndMetricsClient>(serviceProvider =>
            new AuditAndMetricsClient(
                serviceProvider.GetRequiredService<IAuditAndMetricStorageBroker>(),
                serviceProvider.GetRequiredService<IAuditUserBroker>(),
                serviceProvider.GetRequiredService<AuditAndMetricsConfigurations>(),
                serviceProvider.GetRequiredService<ILoggerFactory>()));
    }

    private static void AddBackgroundWorkers(IServiceCollection services)
    {
        // Registered so this host is not the one that silently drops spans, NOT because it
        // currently produces many: the three services that record metric spans
        // (Stu3PatientCoordinationService, Stu3PatientOrchestrationService, Stu3PatientService)
        // are registered on the API host only, so the only producer here today is the metrics
        // seeding endpoint. It costs one listener and earns correctness the moment this host
        // grows a path that records a span - metric rows are purged on a retention timer, so the
        // telemetry copy is the only record that outlives the sweep.
        //
        // This host registers no ICorrelationBroker, so the library falls back to its null
        // request-trace port: these spans group by trace but are not anchored under a request.
        // Wiring the correlation middleware here would fix that; it has not been done.
        //
        // The source name comes from the same bound AuditAndMetricsConfigurations instance the
        // client uses, so the two cannot drift apart. GetService rather than GetRequiredService
        // for the telemetry client: Application Insights is absent in some hosts and test runs,
        // and the publisher idles rather than taking the host down when it is.
        //
        // This host registers no dispatcher, so the metric library falls back to its
        // ThreadPoolDispatcher. Deferred writes still happen; they are simply not bounded or
        // drained on shutdown the way the API host's are.
        services.AddHostedService(serviceProvider =>
            new MetricTelemetryPublisher(
                serviceProvider.GetService<TelemetryClient>(),
                serviceProvider.GetRequiredService<ILogger<MetricTelemetryPublisher>>(),
                serviceProvider.GetRequiredService<AuditAndMetricsConfigurations>()
                    .ActivitySourceName));
    }
}
