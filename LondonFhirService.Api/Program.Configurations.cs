// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

#nullable enable annotations

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Hl7.Fhir.Serialization;
using ISL.Providers.Captcha.Abstractions;
using ISL.Providers.Captcha.FakeCaptcha.Providers.FakeCaptcha;
using ISL.Providers.Captcha.GoogleReCaptcha.Models.Brokers.GoogleReCaptcha;
using ISL.Providers.Captcha.GoogleReCaptcha.Providers;
using ISL.Security.Client.Models.Clients;
using LondonFhirService.Api.Dispatchers;
using LondonFhirService.Api.Middlewares;
using LondonFhirService.Api.Workers;
using LondonFhirService.Core.Workers;
using LondonFhirService.Clients.AuditAndMetrics.Clients;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using LondonFhirService.Clients.AuditAndMetrics.Models.Configurations;
using LondonFhirService.Core.Abstractions.Brokers;
using LondonFhirService.Core.Brokers.AuditAndMetrics;
using LondonFhirService.Core.Services.Foundations.Metrics;
using LondonFhirService.Core.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Brokers.Correlations;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Fhirs.STU3;
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Abstractions.Models;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Models.Foundations.Patients;
using LondonFhirService.Core.Models.Orchestrations.Accesses;
using LondonFhirService.Core.Services.Coordinations.Patients.STU3;
using LondonFhirService.Core.Services.Foundations.Audits;
using LondonFhirService.Core.Services.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Services.Orchestrations.FhirReconciliations.STU3;
using LondonFhirService.Core.Services.Foundations.FhirRecordDifferences;
using LondonFhirService.Core.Services.Foundations.FhirRecords;
using LondonFhirService.Core.Services.Foundations.JsonElements;
using LondonFhirService.Core.Services.Foundations.Patients.STU3;
using LondonFhirService.Core.Services.Foundations.Providers;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.AllergyIntolerances;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Appointments;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Conditions;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.DiagnosticReports;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Encounters;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.EpisodeOfCares;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.FamilyMemberHistories;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Immunizations;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Lists;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Locations;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.MedicationRequests;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Medications;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.MedicationStatements;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Observations;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Organizations;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Patients;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.PractitionerRoles;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Practitioners;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.ProcedureRequests;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Procedures;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.ReferralRequests;
using LondonFhirService.Core.Services.Orchestrations.CompareQueue;
using LondonFhirService.Core.Services.Orchestrations.Comparisons;
using LondonFhirService.Core.Services.Orchestrations.Patients.STU3;
using LondonFhirService.Core.Services.Processings.JsonIgnoreRules;
using LondonFhirService.Core.Services.Processings.ListEntryComparisons;
using LondonFhirService.Core.Services.Processings.ResourceMatchings;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Brokers.DdsHttp;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Providers;
using LondonFhirService.Providers.FHIR.STU3.LondonDataService.Models.Brokers.LdsHttp;
using LondonFhirService.Providers.FHIR.STU3.LondonDataService.Providers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using STU3FhirAbstractions = LondonFhirService.Providers.FHIR.STU3.Abstractions;
using LondonFhirService.Clients.AuditAndMetrics.Clients.Metrics;
using LondonFhirService.Clients.AuditAndMetrics.Clients.Audits;

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

        builder.Services.AddRequestTimeouts(options =>
            options.DefaultPolicy =
                new Microsoft.AspNetCore.Http.Timeouts.RequestTimeoutPolicy
                {
                    Timeout = TimeSpan.FromSeconds(130)
                });

        builder.Services.AddDbContextFactory<StorageBroker>();
        builder.Services.AddDbContext<StorageBroker>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddEndpointsApiExplorer();

        // ----------------- Domain registrations -----------------
        AddProviders(builder.Services, configuration);
        AddBrokers(builder.Services, configuration);
        AddFoundationServices(builder.Services);
        AddOrchestrationServices(builder.Services, configuration);
        AddProcessingServices(builder.Services);
        AddCoordinationServices(builder.Services, configuration);
        AddClients(builder.Services, builder.Configuration);
        AddBackgroundWorkers(builder.Services, configuration);

        // IConfiguration registration (optional, but mirrors original)
        builder.Services.AddSingleton<IConfiguration>(configuration);

        JsonNamingPolicy jsonNamingPolicy = JsonNamingPolicy.CamelCase;

        // No OData here. The Audits, FhirRecords and FhirRecordDifference entity sets used to be
        // exposed from this host, which put whole patient payloads behind a queryable surface on
        // the public API. They live on the internal management host instead.
        builder.Services
            .AddControllers()
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
        // First, deliberately. Every response leaving this host - including the ones produced by
        // the authentication, authorization and timeout middleware below, which never reach a
        // controller - carries the correlation id its logs and metric spans were written under.
        app.UseMiddleware<CorrelationMiddleware>();

        // Immediately behind it, because without a handler an exception that escapes MVC reaches
        // Kestrel, which resets the response headers and synthesises a 500 WITHOUT firing the
        // OnStarting callbacks - so the one response class the correlation middleware exists for
        // came back bare. Catching it here turns it into an ordinary response, which starts
        // normally and therefore carries the header. The body stays empty so the FHIR contract is
        // unchanged; the id is in the header, which is what a consumer needs to quote.
        app.UseExceptionHandler(errorApplication =>
            errorApplication.Run(context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                return Task.CompletedTask;
            }));

        app.MapGet("/", () => Results.Ok(new
        {
            Name = "London FHIR Service API",
            // The release, from LondonFhirService.Core's assembly version, so it moves with each
            // release rather than reading 1.0 forever.
            Version = LondonFhirService.Core.CoreVersion.Value,
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
        app.UseRequestTimeouts();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        //app.MapFallbackToFile("/index.html");
    }

    /// <summary>
    /// Absolute on its own is not enough, which is what these checks used to ask. Uri.TryCreate
    /// reads "localhost:7284/x" as an absolute uri whose scheme is localhost, so the single
    /// likeliest mistake - dropping the https from a setting - passed the guard and failed later
    /// inside the provider instead. Naming the two schemes a provider can dial is what makes the
    /// message true.
    /// </summary>
    private static bool IsDialableUrl(string url) =>
        string.IsNullOrWhiteSpace(url) is false
        && Uri.TryCreate(url.Trim(), UriKind.Absolute, out Uri parsedUrl)
        && (parsedUrl.Scheme == Uri.UriSchemeHttp || parsedUrl.Scheme == Uri.UriSchemeHttps);

    private const string StartupLoggerCategory = "LondonFhirService.Api.Startup";

    private const string ConfigurationHint =
        "Please check appsettings.json or the corresponding environment variables/secrets.";

    /// <summary>
    /// Every problem that stops the STU3 providers being built, reported together. Checked one at
    /// a time, each fix needed its own redeploy to find the next one.
    ///
    /// Fatal problems only: a section that is absent, a url the provider's HttpClient cannot take
    /// as its base address, or a timeout HttpClient rejects. DdsStu3Provider and LdsStu3Provider are
    /// constructed unconditionally, so any of these fails the host whether or not a Provider row
    /// ever names that provider. Values that only fail when the provider is dialled - credentials,
    /// scope, the relative url - are warned about after startup instead, because an environment
    /// that does not use a provider yet legitimately ships placeholders for them.
    /// </summary>
    internal static void ValidateProviderConfigurations(
        PatientServiceConfig? patientServiceConfig,
        DdsConfigurations? ddsConfig,
        LdsConfigurations? ldsConfig,
        AccessConfigurations? accessConfig)
    {
        var problems = new List<string>();

        if (patientServiceConfig is null)
        {
            problems.Add("PatientServiceConfig is missing.");
        }

        if (ddsConfig is null)
        {
            problems.Add("DdsConfigurations is missing.");
        }
        else
        {
            if (IsDialableUrl(ddsConfig.BaseUrl) is false)
            {
                problems.Add("DdsConfigurations:BaseUrl is missing or is not an absolute http or https URI.");
            }

            if (IsDialableUrl(ddsConfig.AuthorisationUrl) is false)
            {
                problems.Add(
                    "DdsConfigurations:AuthorisationUrl is missing or is not an absolute http or https URI.");
            }

            if (IsUsableTimeout(ddsConfig.TimeoutSeconds) is false)
            {
                problems.Add(
                    $"DdsConfigurations:TimeoutSeconds must be between 1 and {MaximumTimeoutSeconds}, "
                        + $"but is {ddsConfig.TimeoutSeconds}.");
            }
        }

        if (ldsConfig is null)
        {
            problems.Add("LdsConfigurations is missing.");
        }
        else
        {
            if (IsDialableUrl(ldsConfig.BaseUrl) is false)
            {
                problems.Add("LdsConfigurations:BaseUrl is missing or is not an absolute http or https URI.");
            }

            if (IsUsableTimeout(ldsConfig.TimeoutSeconds) is false)
            {
                problems.Add(
                    $"LdsConfigurations:TimeoutSeconds must be between 1 and {MaximumTimeoutSeconds}, "
                        + $"but is {ldsConfig.TimeoutSeconds}.");
            }
        }

        if (accessConfig is null)
        {
            problems.Add("AccessConfigurations is missing.");
        }

        if (problems.Count > 0)
        {
            throw new InvalidOperationException(
                "The patient provider configuration is not usable, so the host cannot start. "
                    + string.Join(" ", problems) + " " + ConfigurationHint);
        }
    }

    /// <summary>
    /// HttpClient.Timeout is held in milliseconds and rejects anything past int.MaxValue of them,
    /// and zero or less. Either way the provider's constructor throws - and since the providers
    /// are built on first use, that used to surface as every patient request failing rather than
    /// as a host that would not start.
    /// </summary>
    private const int MaximumTimeoutSeconds = int.MaxValue / 1000;

    private static bool IsUsableTimeout(int timeoutSeconds) =>
        timeoutSeconds > 0 && timeoutSeconds <= MaximumTimeoutSeconds;

    /// <summary>
    /// Values the host starts without but a provider cannot be dialled without. Warnings, not
    /// failures: a provider is only called when a Provider row names it, and an environment not
    /// yet using one ships its placeholders. Logged at startup so a provider that is switched on
    /// with them still in place is obvious from the first line of the log, not from the first
    /// failed patient request.
    /// </summary>
    internal static List<string> FindProviderConfigurationWarnings(
        DdsConfigurations ddsConfig,
        LdsConfigurations ldsConfig)
    {
        var warnings = new List<string>();

        void WarnIfUnset(string value, string key)
        {
            if (IsUnset(value))
            {
                warnings.Add($"{key} is not set, so calls to that provider will fail.");
            }
        }

        WarnIfUnset(ddsConfig.ClientId, "DdsConfigurations:ClientId");
        WarnIfUnset(ddsConfig.ClientSecret, "DdsConfigurations:ClientSecret");
        WarnIfUnset(ddsConfig.GetStructuredRecordRelativeUrl, "DdsConfigurations:GetStructuredRecordRelativeUrl");
        WarnIfUnset(ldsConfig.Scope, "LdsConfigurations:Scope");
        WarnIfUnset(ldsConfig.GetStructuredRecordRelativeUrl, "LdsConfigurations:GetStructuredRecordRelativeUrl");

        // Blank is legitimate here - it means the system assigned identity - so only the shipped
        // placeholder is a mistake.
        if (IsPlaceholder(ldsConfig.ManagedIdentityClientId))
        {
            warnings.Add(
                "LdsConfigurations:ManagedIdentityClientId still holds the shipped placeholder, so "
                    + "calls to that provider will fail to get a token.");
        }

        return warnings;
    }

    private static bool IsUnset(string value) =>
        string.IsNullOrWhiteSpace(value) || IsPlaceholder(value);

    private static bool IsPlaceholder(string value) =>
        value is not null && value.Contains("override_this", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Run once the host is built, while startup can still fail loudly. The STU3 providers are a
    /// lazily resolved singleton, so anything wrong with building them used to wait for the first
    /// patient request and then fail every one after it. Building them here turns that into a
    /// host that does not start, with the reason logged as critical.
    /// </summary>
    internal static void VerifyStartupDependencies(WebApplication app)
    {
        ILogger logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(StartupLoggerCategory);

        List<string> warnings = FindProviderConfigurationWarnings(
            app.Services.GetRequiredService<DdsConfigurations>(),
            app.Services.GetRequiredService<LdsConfigurations>());

        foreach (string warning in warnings)
        {
            logger.LogWarning("Provider configuration: {ConfigurationWarning} {ConfigurationHint}",
                warning,
                ConfigurationHint);
        }

        try
        {
            app.Services.GetRequiredService<STU3FhirAbstractions.IFhirAbstractionProvider>();
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                "The STU3 patient providers could not be built from DdsConfigurations and "
                    + "LdsConfigurations. " + ConfigurationHint,
                exception);
        }
    }

    /// <summary>
    /// A host that fails to start used to leave nothing behind but a process exit: the guards throw
    /// from ConfigureServices, before logging or Application Insights exist, so in Azure the only
    /// symptom was a generic "failed to start" page. This makes sure the reason is logged as
    /// critical wherever it can be seen.
    ///
    /// Locally it goes through the host's own logger when there is a host, and straight to the
    /// console when there is not. Application Insights is always given it through a client built
    /// from the connection string alone - even when the host exists, because its telemetry pipeline
    /// only starts with the host, and a host that failed before app.Run never started it. Verified:
    /// logged through the host's ILogger alone, a failed migration reached the console and nothing
    /// reached Application Insights.
    ///
    /// Nothing here may throw: it runs on the way out of a failure, and an exception of its own
    /// would replace the one that explains what went wrong.
    /// </summary>
    internal static async Task LogStartupFailureAsync(
        WebApplication? app,
        IConfiguration configuration,
        Exception exception)
    {
        const string message = "London FHIR Service API failed to start. {StartupFailureReason}";
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

            using TelemetryConfiguration telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            telemetryConfiguration.ConnectionString = connectionString;
            var telemetryClient = new TelemetryClient(telemetryConfiguration);

            telemetryClient.TrackException(new ExceptionTelemetry(exception)
            {
                SeverityLevel = SeverityLevel.Critical,
                Message = $"London FHIR Service API failed to start. {exception.Message}"
            });

            telemetryClient.Flush();
        }
        catch (Exception)
        {
            // A malformed connection string must not hide the failure being reported.
        }
    }

    private static void AddProviders(IServiceCollection services, IConfiguration configuration)
    {
        PatientServiceConfig patientServiceConfig = configuration
            .GetSection("PatientServiceConfig")
            .Get<PatientServiceConfig>();

        DdsConfigurations ddsConfig = configuration
            .GetSection("DdsConfigurations")
            .Get<DdsConfigurations>();

        LdsConfigurations ldsConfig = configuration
            .GetSection("LdsConfigurations")
            .Get<LdsConfigurations>();

        // checkAccessPermissions is deliberately false for now: the consumer access service is not
        // yet in use, and the flag is the switch that turns it on when it is. A present section
        // saying false is the intended state; only an absent one is a misconfiguration.
        AccessConfigurations accessConfig = configuration
            .GetSection("AccessConfigurations")
            .Get<AccessConfigurations>();

        ValidateProviderConfigurations(patientServiceConfig, ddsConfig, ldsConfig, accessConfig);

        services.AddSingleton(patientServiceConfig);
        services.AddSingleton(ddsConfig);
        services.AddSingleton(ldsConfig);
        services.AddSingleton(accessConfig);

        services.AddSingleton<STU3FhirAbstractions.IFhirAbstractionProvider>(sp =>
        {
            var config = sp.GetRequiredService<DdsConfigurations>();
            ILogger<DdsStu3Provider> logger = sp.GetRequiredService<ILogger<DdsStu3Provider>>();

            var ldsConfigurations = sp.GetRequiredService<LdsConfigurations>();
            ILogger<LdsStu3Provider> ldsLogger = sp.GetRequiredService<ILogger<LdsStu3Provider>>();
            IHttpContextAccessor httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();

            var stu3Providers = new List<STU3FhirAbstractions.IFhirProvider>
            {
                new DdsStu3Provider(config, logger),
                new LdsStu3Provider(ldsConfigurations, httpContextAccessor, ldsLogger),
            };

            return new STU3FhirAbstractions.FhirAbstractionProvider(stu3Providers);
        });

        bool fakeCaptchaProviderMode = configuration
            .GetSection("FakeCaptchaProviderMode").Get<bool>();

        services.AddTransient<ICaptchaAbstractionProvider, CaptchaAbstractionProvider>();

        if (fakeCaptchaProviderMode)
        {
            services.AddTransient<ICaptchaProvider, FakeCaptchaProvider>();
        }
        else
        {
            // Guarded because AddSingleton(null) throws an ArgumentNullException naming
            // "implementationInstance" - which says nothing about which setting is missing.
            GoogleReCaptchaConfigurations reCaptchaConfigurations = configuration
                .GetSection("googleReCaptchaConfigurations")
                .Get<GoogleReCaptchaConfigurations>()
                ?? throw new InvalidOperationException(
                    "googleReCaptchaConfigurations is missing, and FakeCaptchaProviderMode is not "
                        + "true. Please check appsettings.json or the corresponding environment "
                        + "variables/secrets.");

            services.AddSingleton(reCaptchaConfigurations);
            services.AddTransient<ICaptchaProvider, GoogleReCaptchaProvider>();
        }
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

        ConsumerAccessConfiguration consumerAccessConfiguration = configuration
            .GetSection("ConsumerAccessConfiguration")
            .Get<ConsumerAccessConfiguration>()
                ?? throw new InvalidOperationException(
                    "ConsumerAccessConfiguration is missing or invalid. Please check appsettings.json.");

        services.AddSingleton(securityConfigurations);
        services.AddSingleton(consumerAccessConfiguration);
        services.AddSingleton<TokenCredential>(new DefaultAzureCredential());
        services.AddHttpClient<IConsumerAccessBroker, ConsumerAccessBroker>();
        services.AddTransient<IAuditAndMetricBroker, AuditAndMetricBroker>();

        // Scoped, because the correlation id is per request. The value itself lives on
        // HttpContext.Items, so a second instance within the same request still reads the first
        // one's id - the lifetime is what the broker means, not what makes it work.
        services.AddScoped<ICorrelationBroker, CorrelationBroker>();

        // Scoped for the same reason: it forwards a value captured on the request in flight.
        services.AddScoped<IRequestTraceBroker, RequestTraceBroker>();
        services.AddScoped<IAuditAndMetricStorageBroker, AuditAndMetricStorageBroker>();
        services.AddScoped<IAuditUserBroker, AuditUserBroker>();
        services.AddTransient<IDateTimeBroker, DateTimeBroker>();
        services.AddTransient<IStu3FhirBroker, Stu3FhirBroker>();
        services.AddTransient<IIdentifierBroker, IdentifierBroker>();
        services.AddTransient<ILoggingBroker, LoggingBroker>();
        services.AddTransient<ISecurityAuditBroker, SecurityAuditBroker>();
        services.AddTransient<ISecurityBroker, SecurityBroker>();

        services.AddScoped<IStorageBroker>(
            serviceProvider => serviceProvider.GetRequiredService<StorageBroker>());

        services.AddScoped<IStorageBrokerFactory, StorageBrokerFactory>();
    }

    private static void AddFoundationServices(IServiceCollection services)
    {
        services.AddTransient<IAuditService, AuditService>();
        services.AddTransient<IMetricService, MetricService>();
        services.AddTransient<IConsumerAccessService, ConsumerAccessService>();
        services.AddTransient<IFhirRecordDifferenceService, FhirRecordDifferenceService>();
        services.AddTransient<IFhirRecordService, FhirRecordService>();
        services.AddTransient<IStu3PatientService, Stu3PatientService>();
        services.AddTransient<IProviderService, ProviderService>();
        services.AddSingleton<IJsonElementService, JsonElementService>();
        services.AddTransient<IResourceMatcherService, AllergyIntoleranceMatcherService>();
        services.AddTransient<IResourceMatcherService, AppointmentMatcherService>();
        services.AddTransient<IResourceMatcherService, ConditionMatcherService>();
        services.AddTransient<IResourceMatcherService, DiagnosticReportMatcherService>();
        services.AddTransient<IResourceMatcherService, EncounterMatcherService>();
        services.AddTransient<IResourceMatcherService, EpisodeOfCareMatcherService>();
        services.AddTransient<IResourceMatcherService, FamilyMemberHistoryMatcherService>();
        services.AddTransient<IResourceMatcherService, ImmunizationMatcherService>();
        services.AddTransient<IResourceMatcherService, ListMatcherService>();
        services.AddTransient<IResourceMatcherService, LocationMatcherService>();
        services.AddTransient<IResourceMatcherService, MedicationMatcherService>();
        services.AddTransient<IResourceMatcherService, MedicationRequestMatcherService>();
        services.AddTransient<IResourceMatcherService, MedicationStatementMatcherService>();
        services.AddTransient<IResourceMatcherService, ObservationMatcherService>();
        services.AddTransient<IResourceMatcherService, OrganizationMatcherService>();
        services.AddTransient<IResourceMatcherService, PatientMatcherService>();
        services.AddTransient<IResourceMatcherService, PractitionerMatcherService>();
        services.AddTransient<IResourceMatcherService, PractitionerRoleMatcherService>();
        services.AddTransient<IResourceMatcherService, ProcedureMatcherService>();
        services.AddTransient<IResourceMatcherService, ProcedureRequestMatcherService>();
        services.AddTransient<IResourceMatcherService, ReferralRequestMatcherService>();
    }

    private static void AddProcessingServices(IServiceCollection services)
    {
        services.AddTransient<IListEntryComparisonProcessingService, ListEntryComparisonProcessingService>();
        services.AddTransient<IResourceMatcherProcessingService, ResourceMatcherProcessingService>();
        services.AddTransient<IJsonIgnoreProcessingRule, ArrayOrderIgnoreProcessingRule>();
        services.AddTransient<IJsonIgnoreProcessingRule, GuidIgnoreProcessingRule>();
        services.AddTransient<IJsonIgnoreProcessingRule, IdIgnoreProcessingRule>();
        services.AddTransient<IJsonIgnoreProcessingRule, MetaIgnoreProcessingRule>();
        services.AddTransient<IJsonIgnoreProcessingRule, ReferenceIgnoreProcessingRule>();
    }

    private static void AddOrchestrationServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IStu3PatientOrchestrationService, Stu3PatientOrchestrationService>();
        services.AddTransient<IStu3FhirReconciliationService, Stu3FhirReconciliationService>();
        services.AddTransient<ICompareQueueOrchestrationService, CompareQueueOrchestrationService>();
        services.AddTransient<IComparisonOrchestrationService, ComparisonOrchestrationService>();
    }

    private static void AddCoordinationServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IStu3PatientCoordinationService, Stu3PatientCoordinationService>();
        services.AddTransient<IComparisonCoordinationService, ComparisonCoordinationService>();
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
                serviceProvider.GetRequiredService<ILoggerFactory>(),
                serviceProvider.GetRequiredService<IAuditAndMetricsDispatcher>(),
                serviceProvider.GetRequiredService<IRequestTraceBroker>()));
    }

    private static void AddBackgroundWorkers(IServiceCollection services, IConfiguration configuration)
    {
        // Validated on start rather than trusted. Zero would turn the worker's drain loop into a
        // spin against the database, and a negative value makes Task.Delay throw from a line that
        // sits outside the loop's catch - which, under the default BackgroundService behaviour,
        // stops the host at startup with an error that names Task.Delay rather than the setting.
        services.AddOptions<ComparisonWorkerSettings>()
            .Bind(configuration.GetSection("ComparisonWorkerSettings"))
            .Validate(
                comparisonWorkerSettings => comparisonWorkerSettings.SleepIntervalSeconds >= 1,
                "ComparisonWorkerSettings:SleepIntervalSeconds must be at least 1 second.")
            .ValidateOnStart();
        services.AddHostedService<ComparisonWorker>();

        // The retention sweeps had no caller, so both tables only ever grew - and the metrics
        // one takes a row per span rather than per request. One worker runs both on a shared
        // cadence; whether either actually deletes is still decided by its own half of
        // AuditAndMetricsConfigurations.
        services.Configure<AuditAndMetricPurgeWorkerSettings>(
            configuration.GetSection("AuditAndMetricPurgeWorkerSettings"));

        services.AddHostedService<AuditAndMetricPurgeWorker>();

        // Deferred audit and metric writes go through a bounded queue rather than a thread pool
        // item each. Singleton, and shared with the worker that drains it.
        services.Configure<AuditAndMetricsDispatcherSettings>(
            configuration.GetSection("AuditAndMetricsDispatcherSettings"));

        services.AddSingleton<AuditAndMetricsDispatcher>();

        services.AddSingleton<IAuditAndMetricsDispatcher>(serviceProvider =>
            serviceProvider.GetRequiredService<AuditAndMetricsDispatcher>());

        services.AddHostedService<AuditAndMetricsDispatchWorker>();

        // Nothing was subscribed to the metric library's ActivitySource, so every span it
        // published was dropped before it reached Application Insights. The source name comes
        // from the same bound instance the client uses, so the two cannot drift apart.
        services.AddHostedService(serviceProvider =>
            new MetricTelemetryPublisher(
                serviceProvider.GetService<TelemetryClient>(),
                serviceProvider.GetRequiredService<ILogger<MetricTelemetryPublisher>>(),
                serviceProvider.GetRequiredService<AuditAndMetricsConfigurations>()
                    .ActivitySourceName));
    }
}
