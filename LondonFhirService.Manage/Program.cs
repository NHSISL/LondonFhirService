// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

#nullable enable annotations

using System;
using System.IO;
using Attrify.InvisibleApi.Models;
using LondonFhirService.Core.Brokers.Storages.Sql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
WebApplication? app = null;

// Everything that can stop the host starting is inside this try, so the reason is logged as
// critical rather than lost with the process. It is rethrown afterwards: bad configuration should
// still stop the host, just not silently.
try
{
    // Load settings from launchSettings.json (for local debug / tooling)
    var contentRoot = builder.Environment.ContentRootPath;
    var projectDir = Directory.GetCurrentDirectory();
    var launchSettingsPath = Path.Combine(projectDir, "Properties", "launchSettings.json");

    if (File.Exists(launchSettingsPath))
    {
        builder.Configuration.AddJsonFile(launchSettingsPath, optional: true);
    }

    builder.Configuration
        .AddJsonFile(Path.Combine(projectDir, "appsettings.json"), optional: false)
        .AddJsonFile(Path.Combine(projectDir, "appsettings.Development.json"), optional: true)
        .AddEnvironmentVariables();

    Program.ConfigurationOverridesForTesting(builder);

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    });

    // Shared InvisibleApiKey instance, also available via DI
    var invisibleApiKey = new InvisibleApiKey();
    builder.Services.AddSingleton(invisibleApiKey);

    // Register health checks
    builder.Services.AddHealthChecks();

    // Register services using the host configuration (which tests can override)
    Program.ConfigureServices(builder);
    Program.ConfigureApplicationInsightsTelemetry(builder);

    app = builder.Build();
    Program.VerifyStartupDependencies(app);

    // Always run migrations at startup
    using (var scope = app.Services.CreateScope())
    {
        var storageBroker = scope.ServiceProvider.GetRequiredService<StorageBroker>();
        storageBroker.Database.Migrate();
    }

    // Configure middleware pipeline
    Program.ConfigurePipeline(app);

    app.Run();
}
// HostAbortedException is how WebApplicationFactory and the EF tooling stop the host once they have
// what they need from it. It is not a failure, and logging it would put a critical line in every
// test run and every migration.
catch (Exception exception) when (exception is not HostAbortedException)
{
    await Program.LogStartupFailureAsync(app, builder.Configuration, exception);

    throw;
}

// Exposed so WebApplicationFactory<Program> has a concrete entry point type
public partial class Program { }
