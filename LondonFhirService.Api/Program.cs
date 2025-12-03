// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

extern alias FhirR4;
extern alias FhirSTU3;

using System.IO;
using Attrify.InvisibleApi.Models;
using LondonFhirService.Core.Brokers.Storages.Sql;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Load settings from launchSettings.json (for local debug / tooling)
var projectDir = Directory.GetCurrentDirectory();
var launchSettingsPath = Path.Combine(projectDir, "Properties", "launchSettings.json");

if (File.Exists(launchSettingsPath))
{
    builder.Configuration.AddJsonFile(launchSettingsPath, optional: true);
}

Program.ConfigurationOverridesForTesting(builder);

// Shared InvisibleApiKey instance, also available via DI
var invisibleApiKey = new InvisibleApiKey();
builder.Services.AddSingleton(invisibleApiKey);

// Register health checks
builder.Services.AddHealthChecks();

// Register services using the host configuration (which tests can override)
Program.ConfigureServices(builder);

var app = builder.Build();

// Always run migrations at startup
using (var scope = app.Services.CreateScope())
{
    var storageBroker = scope.ServiceProvider.GetRequiredService<StorageBroker>();
    storageBroker.Database.Migrate();
}

// Configure middleware pipeline
Program.ConfigurePipeline(app);

app.Run();

// Exposed so WebApplicationFactory<Program> has a concrete entry point type
public partial class Program { }
