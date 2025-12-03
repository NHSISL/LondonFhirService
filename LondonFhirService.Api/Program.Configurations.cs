// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

extern alias FhirR4;
extern alias FhirSTU3;

using System;
using System.Collections.Generic;
using System.Text.Json;
using Attrify.InvisibleApi.Models;
using ISL.Providers.Captcha.Abstractions;
using ISL.Providers.Captcha.FakeCaptcha.Providers.FakeCaptcha;
using ISL.Providers.Captcha.GoogleReCaptcha.Models.Brokers.GoogleReCaptcha;
using ISL.Providers.Captcha.GoogleReCaptcha.Providers;
using ISL.Security.Client.Models.Clients;
using LondonFhirService.Api.Formatters;
using LondonFhirService.Core.Brokers.Audits;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Fhirs.R4;
using LondonFhirService.Core.Brokers.Fhirs.STU3;
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Clients.Audits;
using LondonFhirService.Core.Models.Foundations.Patients;
using LondonFhirService.Core.Services.Coordinations.Patients.R4;
using LondonFhirService.Core.Services.Coordinations.Patients.STU3;
using LondonFhirService.Core.Services.Foundations.Audits;
using LondonFhirService.Core.Services.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Services.Foundations.Consumers;
using LondonFhirService.Core.Services.Foundations.FhirReconciliations.R4;
using LondonFhirService.Core.Services.Foundations.FhirReconciliations.STU3;
using LondonFhirService.Core.Services.Foundations.OdsDatas;
using LondonFhirService.Core.Services.Foundations.Patients.R4;
using LondonFhirService.Core.Services.Foundations.Patients.STU3;
using LondonFhirService.Core.Services.Foundations.PdsDatas;
using LondonFhirService.Core.Services.Foundations.Providers;
using LondonFhirService.Core.Services.Orchestrations.Accesses;
using LondonFhirService.Core.Services.Orchestrations.Patients.R4;
using LondonFhirService.Core.Services.Orchestrations.Patients.STU3;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Brokers.DdsHttp;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Providers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using R4FhirAbstractions = LondonFhirService.Providers.FHIR.R4.Abstractions;
using STU3FhirAbstractions = LondonFhirService.Providers.FHIR.STU3.Abstractions;

public partial class Program
{
    internal static Action<WebApplicationBuilder>? TestConfigurationOverrides { get; set; }

    internal static void ConfigurationOverridesForTesting(WebApplicationBuilder builder)
    {
        TestConfigurationOverrides?.Invoke(builder);
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
        builder.Services.AddDbContext<StorageBroker>();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // ----------------- Domain registrations -----------------
        AddProviders(builder.Services, configuration);
        AddBrokers(builder.Services, configuration);
        AddFoundationServices(builder.Services);
        AddOrchestrationServices(builder.Services, configuration);
        AddProcessingServices(builder.Services);
        AddCoordinationServices(builder.Services, configuration);
        AddClients(builder.Services);

        // IConfiguration registration (optional, but mirrors original)
        builder.Services.AddSingleton<IConfiguration>(configuration);

        JsonNamingPolicy jsonNamingPolicy = JsonNamingPolicy.CamelCase;

        builder.Services
            .AddControllers(options =>
            {
                options.InputFormatters.Insert(0, new FhirJsonInputFormatter());
                options.OutputFormatters.Insert(0, new FhirJsonOutputFormatter());
            })
            .AddOData(options =>
            {
                options.AddRouteComponents("odata", GetEdmModel());
                options.Select().Filter().Expand().OrderBy().Count().SetMaxTop(100);
            })
            .AddJsonOptions(options =>
            {
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

        app.MapGet("/", () => Results.Ok(new
        {
            Name = "London FHIR Service API",
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
        //app.UseInvisibleApiMiddleware(invisibleApiKey);
        app.MapControllers();
        //app.MapFallbackToFile("/index.html");
    }

    private static IEdmModel GetEdmModel()
    {
        ODataConventionModelBuilder builder = new();
        builder.EnableLowerCamelCase();
        return builder.GetEdmModel();
    }

    private static void AddProviders(IServiceCollection services, IConfiguration configuration)
    {
        PatientServiceConfig patientServiceConfig = configuration
            .GetSection("PatientServiceConfig")
            .Get<PatientServiceConfig>();

        DdsConfigurations ddsConfig = configuration
            .GetSection("DdsConfigurations")
            .Get<DdsConfigurations>();

        services.AddSingleton(patientServiceConfig);
        services.AddSingleton(ddsConfig);

        var stu3Providers = new List<STU3FhirAbstractions.IFhirProvider>
        {
            new DdsStu3Provider(ddsConfig)
        };

        services.AddTransient<R4FhirAbstractions.IFhirAbstractionProvider,
            R4FhirAbstractions.FhirAbstractionProvider>();

        services.AddSingleton<STU3FhirAbstractions.IFhirAbstractionProvider>(
            new STU3FhirAbstractions.FhirAbstractionProvider(stu3Providers));

        bool fakeCaptchaProviderMode = configuration
            .GetSection("FakeCaptchaProviderMode").Get<bool>();

        services.AddTransient<ICaptchaAbstractionProvider, CaptchaAbstractionProvider>();

        if (fakeCaptchaProviderMode)
        {
            services.AddTransient<ICaptchaProvider, FakeCaptchaProvider>();
        }
        else
        {
            GoogleReCaptchaConfigurations reCaptchaConfigurations = configuration
                .GetSection("googleReCaptchaConfigurations")
                .Get<GoogleReCaptchaConfigurations>();

            services.AddSingleton(reCaptchaConfigurations);
            services.AddTransient<ICaptchaProvider, GoogleReCaptchaProvider>();
        }
    }

    private static void AddBrokers(IServiceCollection services, IConfiguration configuration)
    {
        SecurityConfigurations securityConfigurations = new();
        services.AddSingleton(securityConfigurations);
        services.AddTransient<IAuditBroker, AuditBroker>();
        services.AddTransient<IDateTimeBroker, DateTimeBroker>();
        services.AddTransient<IR4FhirBroker, R4FhirBroker>();
        services.AddTransient<IStu3FhirBroker, Stu3FhirBroker>();
        services.AddTransient<IIdentifierBroker, IdentifierBroker>();
        services.AddTransient<ILoggingBroker, LoggingBroker>();
        services.AddTransient<ISecurityAuditBroker, SecurityAuditBroker>();
        services.AddTransient<ISecurityBroker, SecurityBroker>();
        services.AddTransient<IStorageBroker, StorageBroker>();
    }

    private static void AddFoundationServices(IServiceCollection services)
    {
        services.AddTransient<IAuditService, AuditService>();
        services.AddTransient<IConsumerAccessService, ConsumerAccessService>();
        services.AddTransient<IConsumerService, ConsumerService>();
        services.AddTransient<IR4FhirReconciliationService, R4FhirReconciliationService>();
        services.AddTransient<IStu3FhirReconciliationService, Stu3FhirReconciliationService>();
        services.AddTransient<IOdsDataService, OdsDataService>();
        services.AddTransient<IR4PatientService, R4PatientService>();
        services.AddTransient<IStu3PatientService, Stu3PatientService>();
        services.AddTransient<IPdsDataService, PdsDataService>();
        services.AddTransient<IProviderService, ProviderService>();
    }

    private static void AddProcessingServices(IServiceCollection services)
    {
        // intentionally empty for now
    }

    private static void AddOrchestrationServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IAccessOrchestrationService, AccessOrchestrationService>();
        services.AddTransient<IR4PatientOrchestrationService, R4PatientOrchestrationService>();
        services.AddTransient<IStu3PatientOrchestrationService, Stu3PatientOrchestrationService>();
    }

    private static void AddCoordinationServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IR4PatientCoordinationService, R4PatientCoordinationService>();
        services.AddTransient<IStu3PatientCoordinationService, Stu3PatientCoordinationService>();
    }

    private static void AddClients(IServiceCollection services)
    {
        services.AddTransient<IAuditClient, AuditClient>();
    }
}
