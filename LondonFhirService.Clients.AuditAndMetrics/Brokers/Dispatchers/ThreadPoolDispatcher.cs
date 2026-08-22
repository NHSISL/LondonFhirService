// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Clients.AuditAndMetrics.Brokers.Loggings;
using LondonFhirService.Core.Abstractions.Brokers;

namespace LondonFhirService.Clients.AuditAndMetrics.Brokers.Dispatchers
{
    /// <summary>
    /// The fallback used when the hosting application supplies no dispatcher of its own: one
    /// thread pool work item per write.
    ///
    /// Deliberately unbounded, and that is exactly why a host should supply its own. Under load
    /// this queues a work item per recorded span with nothing throttling it, and nothing drains
    /// it on shutdown. It exists so a consumer without a host lifecycle still works, not because
    /// it is the right way to run this in production.
    /// </summary>
    internal class ThreadPoolDispatcher : IAuditAndMetricsDispatcher
    {
        private readonly ILoggingBroker loggingBroker;

        public ThreadPoolDispatcher(ILoggingBroker loggingBroker) =>
            this.loggingBroker = loggingBroker;

        public bool TryDispatch(Func<CancellationToken, ValueTask> work)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await work(CancellationToken.None);
                }
                catch (OperationCanceledException)
                {
                    // Expected, not a failure. A cancelled request or a host shutting down
                    // cancels these writes, and logging that would put an error in the log every
                    // time a client disconnects.
                }
                catch (Exception exception)
                {
                    // Nobody is awaiting this, so the log line is the only trace a lost entry
                    // leaves. Left unhandled it would surface as an unobserved task exception,
                    // detached from the request that caused it.
                    await this.loggingBroker.LogErrorAsync(exception);
                }
            });

            // Never refused: the thread pool has no bound to hit. A host that wants back
            // pressure supplies a dispatcher that does.
            return true;
        }
    }
}
