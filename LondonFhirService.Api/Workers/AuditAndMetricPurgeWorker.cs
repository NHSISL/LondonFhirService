// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Services.Foundations.Audits;
using LondonFhirService.Core.Services.Foundations.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LondonFhirService.Api.Workers
{
    /// <summary>
    /// Runs both retention sweeps. The purges themselves have existed since audits and metrics
    /// were added but nothing ever called them, so the tables only ever grew - and the metrics
    /// one takes a row per span rather than per request.
    ///
    /// One worker rather than two because the two sweeps want the same cadence and the same
    /// "not during a deployment" delay, and running them on separate timers would only make it
    /// possible for both to land on the database at once.
    ///
    /// What each sweep deletes is still governed by its own half of
    /// AuditAndMetricsConfigurations - IsAuditPurgingAllowed / AuditRetentionPeriodInDays and
    /// IsMetricsPurgingAllowed / MetricsRetentionPeriodInDays. This worker only decides when to
    /// ask, and each delete runs in bounded batches inside the service, so a first sweep against
    /// a table that has never been purged does not take one long lock.
    /// </summary>
    public class AuditAndMetricPurgeWorker : BackgroundService
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<AuditAndMetricPurgeWorker> logger;
        private readonly IOptions<AuditAndMetricPurgeWorkerSettings> settings;

        public AuditAndMetricPurgeWorker(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<AuditAndMetricPurgeWorker> logger,
            IOptions<AuditAndMetricPurgeWorkerSettings> settings)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
            this.settings = settings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("AuditAndMetricPurgeWorker started.");

            if (await DelayAsync(
                TimeSpan.FromMinutes(this.settings.Value.InitialDelayMinutes), stoppingToken) is false)
            {
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                // Two sweeps, two try blocks, deliberately. They share a cadence but nothing
                // else: the audit table is usually the one an organisation is obliged to keep,
                // and letting a metrics failure skip the audit sweep - or the reverse - would
                // mean one table quietly stops ageing out because of a fault in the other.
                bool auditSweepCancelled = await SweepAsync(
                    "audit",
                    async scope =>
                    {
                        IAuditService auditService =
                            scope.ServiceProvider.GetRequiredService<IAuditService>();

                        return await auditService
                            .PurgeAuditsOlderThanRetentionPeriodAsync(stoppingToken);
                    },
                    stoppingToken);

                if (auditSweepCancelled)
                {
                    break;
                }

                bool metricSweepCancelled = await SweepAsync(
                    "metric",
                    async scope =>
                    {
                        IMetricService metricService =
                            scope.ServiceProvider.GetRequiredService<IMetricService>();

                        return await metricService
                            .PurgeMetricsOlderThanRetentionPeriodAsync(stoppingToken);
                    },
                    stoppingToken);

                if (metricSweepCancelled)
                {
                    break;
                }

                if (await DelayAsync(
                    TimeSpan.FromHours(this.settings.Value.SweepIntervalHours), stoppingToken) is false)
                {
                    break;
                }
            }

            this.logger.LogInformation("AuditAndMetricPurgeWorker stopped.");
        }

        /// <summary>
        /// Runs one sweep in its own scope and its own try block, and reports back only whether
        /// the host is shutting down. A failure is logged and swallowed here rather than
        /// returned, because the caller's decision is the same either way: carry on to the other
        /// sweep. Returns true when the worker should stop.
        /// </summary>
        private async ValueTask<bool> SweepAsync(
            string sweepName,
            Func<IServiceScope, ValueTask<int>> sweep,
            CancellationToken stoppingToken)
        {
            try
            {
                // A scope per sweep, because the broker beneath the service resolves a scoped
                // client that holds the request-shaped services this worker has none of. One
                // each rather than one shared, so a scope disposed by a failing sweep cannot
                // take the other one's services with it.
                using IServiceScope scope = this.serviceScopeFactory.CreateScope();

                // The foundation services rather than the brokers directly, so a failed sweep
                // is logged and reported in this application's exception types rather than the
                // library's.
                int purgedCount = await sweep(scope);

                this.logger.LogInformation(
                    "AuditAndMetricPurgeWorker purged {PurgedCount} {SweepName}(s).",
                    purgedCount,
                    sweepName);

                return false;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return true;
            }
            catch (Exception exception)
            {
                // A failed sweep must not stop the worker: the next one will pick up
                // everything this one would have deleted plus whatever has since expired.
                this.logger.LogError(
                    exception,
                    "AuditAndMetricPurgeWorker encountered an error during the {SweepName} retention sweep.",
                    sweepName);

                return false;
            }
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
