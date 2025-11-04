// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.IO;
using System.Text.Json;
using Attrify.Extensions;
using Attrify.InvisibleApi.Models;
using ISL.Security.Client.Models.Clients;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Identifiers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Services.Coordinations.Patients;
using LondonFhirService.Core.Services.Foundations.Audits;
using LondonFhirService.Core.Services.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Services.Foundations.Consumers;
using LondonFhirService.Core.Services.Foundations.FhirReconciliations;
using LondonFhirService.Core.Services.Foundations.OdsDatas;
using LondonFhirService.Core.Services.Foundations.Patients;
using LondonFhirService.Core.Services.Foundations.PdsDatas;
using LondonFhirService.Core.Services.Foundations.Providers;
using LondonFhirService.Core.Services.Orchestrations.Accesses;
using LondonFhirService.Core.Services.Orchestrations.Patients;
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
            builder.Services.AddControllers();
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

            builder.Services.AddControllers()
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
        { }

        private static void AddBrokers(IServiceCollection services, IConfiguration configuration)
        {
            SecurityConfigurations securityConfigurations = new SecurityConfigurations();
            services.AddSingleton(securityConfigurations);
            services.AddTransient<IDateTimeBroker, DateTimeBroker>();
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
            services.AddTransient<IFhirReconciliationService, FhirReconciliationService>();
            services.AddTransient<IOdsDataService, OdsDataService>();
            services.AddTransient<IPatientService, PatientService>();
            services.AddTransient<IPdsDataService, PdsDataService>();
            services.AddTransient<IProviderService, ProviderService>();
        }

        private static void AddProcessingServices(IServiceCollection services)
        { }

        private static void AddOrchestrationServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IAccessOrchestrationService, AccessOrchestrationService>();
            services.AddTransient<IPatientOrchestrationService, PatientOrchestrationService>();
        }

        private static void AddCoordinationServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IPatientCoordinationService, PatientCoordinationService>();
        }

        private static void AddClients(IServiceCollection services)
        { }
    }
}
