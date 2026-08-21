// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.AuditAndMetrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LondonFhirService.Api.Workers
{
    /// <summary>
    /// Runs the metric retention sweep. The purge itself has existed since metrics were added but
    /// nothing ever called it, so the table only ever grew - and it takes a row per span rather
    /// than per request.
    ///
    /// Whether anything is actually deleted is still governed by IsPurgingAllowed and
    /// RetentionPeriodInDays in configuration; this worker only decides when to ask. The delete
    /// runs in bounded batches inside the service, so a first sweep against a table that has
    /// never been purged does not take one long lock.
    /// </summary>
    public class MetricPurgeWorker : BackgroundService
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<MetricPurgeWorker> logger;
        private readonly IOptions<MetricPurgeWorkerSettings> settings;

        public MetricPurgeWorker(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<MetricPurgeWorker> logger,
            IOptions<MetricPurgeWorkerSettings> settings)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
            this.settings = settings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("MetricPurgeWorker started.");

            if (await DelayAsync(
                TimeSpan.FromMinutes(this.settings.Value.InitialDelayMinutes), stoppingToken) is false)
            {
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // A scope per sweep, because the broker beneath resolves a scoped client that
                    // holds the request-shaped services this worker has none of.
                    using IServiceScope scope = this.serviceScopeFactory.CreateScope();

                    IAuditAndMetricBroker auditAndMetricBroker =
                        scope.ServiceProvider.GetRequiredService<IAuditAndMetricBroker>();

                    int purgedCount =
                        await auditAndMetricBroker.PurgeMetricsOlderThanRetentionPeriodAsync(stoppingToken);

                    this.logger.LogInformation(
                        "MetricPurgeWorker purged {PurgedCount} metric(s).", purgedCount);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    // A failed sweep must not stop the worker: the next one will pick up
                    // everything this one would have deleted plus whatever has since expired.
                    this.logger.LogError(
                        exception, "MetricPurgeWorker encountered an error during the retention sweep.");
                }

                if (await DelayAsync(
                    TimeSpan.FromHours(this.settings.Value.SweepIntervalHours), stoppingToken) is false)
                {
                    break;
                }
            }

            this.logger.LogInformation("MetricPurgeWorker stopped.");
        }

        /// <summary>Returns false when the host is shutting down.</summary>
        private static async ValueTask<bool> DelayAsync(TimeSpan delay, CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(delay, stoppingToken);

                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }
    }
}
