// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Services.Coordinations.Patients.STU3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LondonFhirService.Api.Workers
{
    public class ComparisonWorker : BackgroundService
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<ComparisonWorker> logger;
        private readonly IOptions<ComparisonWorkerSettings> settings;

        public ComparisonWorker(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<ComparisonWorker> logger,
            IOptions<ComparisonWorkerSettings> settings)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
            this.settings = settings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("ComparisonWorker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = serviceScopeFactory.CreateScope();

                    var comparisonCoordinationService =
                        scope.ServiceProvider.GetRequiredService<IComparisonCoordinationService>();

                    await comparisonCoordinationService.ProcessFhirRecords();
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "ComparisonWorker encountered an error during processing.");
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(settings.Value.SleepIntervalSeconds),
                    stoppingToken);
            }

            logger.LogInformation("ComparisonWorker stopped.");
        }
    }
}
