// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LondonFhirService.Api.Dispatchers
{
    /// <summary>
    /// Drains the deferred audit and metric writes.
    ///
    /// On shutdown it stops accepting new work and drains what is already queued, so entries
    /// recorded moments before a deployment are not simply lost - which is what happened when
    /// each write was its own detached thread pool item.
    /// </summary>
    public class AuditAndMetricsDispatchWorker : BackgroundService
    {
        private readonly AuditAndMetricsDispatcher dispatcher;
        private readonly ILogger<AuditAndMetricsDispatchWorker> logger;
        private readonly IOptions<AuditAndMetricsDispatcherSettings> settings;

        public AuditAndMetricsDispatchWorker(
            AuditAndMetricsDispatcher dispatcher,
            ILogger<AuditAndMetricsDispatchWorker> logger,
            IOptions<AuditAndMetricsDispatcherSettings> settings)
        {
            this.dispatcher = dispatcher;
            this.logger = logger;
            this.settings = settings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int concurrency = Math.Max(1, this.settings.Value.DrainConcurrency);

            this.logger.LogInformation(
                "AuditAndMetricsDispatchWorker draining with {Concurrency} reader(s).", concurrency);

            IEnumerable<Task> readers = Enumerable
                .Range(0, concurrency)
                .Select(_ => DrainAsync(stoppingToken));

            await Task.WhenAll(readers);

            this.logger.LogInformation(
                "AuditAndMetricsDispatchWorker stopped. {DroppedCount} write(s) were dropped "
                    + "because the queue was full, {RefusedAfterCloseCount} because it had closed.",
                this.dispatcher.DroppedCount,
                this.dispatcher.RefusedAfterCloseCount);
        }

        private async Task DrainAsync(CancellationToken stoppingToken)
        {
            await foreach (Func<CancellationToken, ValueTask> work in
                this.dispatcher.Reader.ReadAllAsync(CancellationToken.None))
            {
                try
                {
                    // CancellationToken.None deliberately: a write that has been queued should
                    // finish even as the host stops. Passing the stopping token would discard
                    // exactly the entries recorded just before a shutdown.
                    await work(CancellationToken.None);
                }
                catch (OperationCanceledException)
                {
                    // Expected; not a failure worth a log line.
                }
                catch (Exception exception)
                {
                    // Nobody awaited this, so this line is the only trace a lost entry leaves.
                    this.logger.LogError(exception, "A deferred audit or metric write failed.");
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            // Not immediately. Hosted services stop in reverse registration order and the web
            // host registers first, so Kestrel is still draining in-flight requests when this
            // worker is asked to stop - and those requests are still recording spans. Completing
            // the channel here would refuse precisely the writes the drain exists to save.
            //
            // The wait is bounded by the host's own shutdown token, so a slow drain cannot hold a
            // deployment open beyond the timeout the host already enforces.
            try
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(Math.Max(0, this.settings.Value.ShutdownGraceSeconds)),
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // The host is out of patience. Fall through and drain what is already queued.
            }

            // Now stop accepting, so the readers see the channel complete and finish the
            // remainder rather than being cut off mid-queue.
            this.dispatcher.Complete();

            await base.StopAsync(cancellationToken);
        }
    }
}
