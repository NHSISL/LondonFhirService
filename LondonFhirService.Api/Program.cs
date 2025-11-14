// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

extern alias FhirR4;
extern alias FhirSTU3;
using System;
using System.IO;
using System.Text.Json;
using Attrify.Extensions;
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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
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

namespace LondonFhirService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var invisibleApiKey = new InvisibleApiKey();
            ConfigureServices(builder, builder.Configuration, invisibleApiKey);
            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var storageBroker = scope.ServiceProvider.GetRequiredService<StorageBroker>();
                storageBroker.Database.Migrate();
            }

            ConfigurePipeline(app, invisibleApiKey);
            app.Run();
        }

        public static void ConfigureServices(
            WebApplicationBuilder builder,
            IConfiguration configuration,
            InvisibleApiKey invisibleApiKey)
        {
            // Load settings from launchSettings.json (for testing)
            var projectDir = Directory.GetCurrentDirectory();
            var launchSettingsPath = Path.Combine(projectDir, "Properties", "launchSettings.json");

            if (File.Exists(launchSettingsPath))
            {
                builder.Configuration.AddJsonFile(launchSettingsPath);
            }

            builder.Configuration
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            // Add services to the container.
            var azureAdOptions = builder.Configuration.GetSection("AzureAd");

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(azureAdOptions);

            var instance = builder.Configuration["AzureAd:Instance"];
            var tenantId = builder.Configuration["AzureAd:TenantId"];
            var scopes = builder.Configuration["AzureAd:Scopes"];
            var missingKeys = new System.Collections.Generic.List<string>();
            if (string.IsNullOrEmpty(instance)) missingKeys.Add("Instance");
            if (string.IsNullOrEmpty(tenantId)) missingKeys.Add("TenantId");
            if (string.IsNullOrEmpty(scopes)) missingKeys.Add("Scopes");

            if (missingKeys.Count > 0)
            {
                throw new InvalidOperationException(
                    $"AzureAd configuration is incomplete. Missing keys: {string.Join(", ", missingKeys)}. " +
                    $"Please check appsettings.json.");
            }

            builder.Services.AddSwaggerGen(configuration =>
            {
                // Add an OAuth2 security definition for Azure AD
            });

            builder.Services.AddSingleton(invisibleApiKey);
            builder.Services.AddAuthorization();
            builder.Services.AddDbContext<StorageBroker>();
            builder.Services.AddHttpContextAccessor();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            AddProviders(builder.Services, builder.Configuration);
            AddBrokers(builder.Services, builder.Configuration);
            AddFoundationServices(builder.Services);
            AddOrchestrationServices(builder.Services, builder.Configuration);
            AddProcessingServices(builder.Services);
            AddCoordinationServices(builder.Services, builder.Configuration);
            AddClients(builder.Services);

            // Register IConfiguration to be available for dependency injection
            builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
            JsonNamingPolicy jsonNamingPolicy = JsonNamingPolicy.CamelCase;

            builder.Services.AddControllers(options =>
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

        public static void ConfigurePipeline(WebApplication app, InvisibleApiKey invisibleApiKey)
        {
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
            app.MapControllers().WithOpenApi();
            app.MapFallbackToFile("/index.html");
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder =
               new ODataConventionModelBuilder();

            builder.EnableLowerCamelCase();

            return builder.GetEdmModel();
        }

        private static void AddProviders(IServiceCollection services, IConfiguration configuration)
        {
            PatientServiceConfig patientServiceConfig = configuration.GetSection("PatientServiceConfig")
                .Get<PatientServiceConfig>();

            services.AddSingleton(patientServiceConfig);

            services.AddTransient<R4FhirAbstractions.IFhirAbstractionProvider,
                R4FhirAbstractions.FhirAbstractionProvider>();

            services.AddTransient<STU3FhirAbstractions.IFhirAbstractionProvider,
                STU3FhirAbstractions.FhirAbstractionProvider>();

            bool fakeCaptchaProviderMode = configuration
                .GetSection("FakeCaptchaProviderMode").Get<bool>();

            services.AddTransient<ICaptchaAbstractionProvider, CaptchaAbstractionProvider>();

            if (fakeCaptchaProviderMode == true)
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
            SecurityConfigurations securityConfigurations = new SecurityConfigurations();
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
        { }

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
}
