// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using LondonFhirService.Core.Abstractions.Brokers;
using Microsoft.Extensions.Options;

namespace LondonFhirService.Api.Dispatchers
{
    /// <summary>
    /// A bounded queue for deferred audit and metric writes, drained by
    /// <see cref="AuditAndMetricsDispatchWorker"/>.
    ///
    /// Replaces a thread pool work item per write. That was fine at low volume and wrong under
    /// load: a request records a span per provider call plus the tracing around it, so a burst of
    /// requests produced a burst of tasks with nothing throttling them.
    ///
    /// Full means refused, not blocked. These writes are observability - making a patient request
    /// wait on a telemetry queue, or fail because of one, would be a worse outcome than losing
    /// the entry. Refusals are counted so the loss is visible rather than silent.
    /// </summary>
    public class AuditAndMetricsDispatcher : IAuditAndMetricsDispatcher
    {
        private readonly Channel<Func<CancellationToken, ValueTask>> channel;
        private long droppedCount;
        private long refusedAfterCloseCount;
        private bool isCompleted;

        public AuditAndMetricsDispatcher(IOptions<AuditAndMetricsDispatcherSettings> settings)
        {
            var options = new BoundedChannelOptions(settings.Value.Capacity)
            {
                // Wait, not DropWrite. TryWrite under DropWrite discards the item and still
                // returns true, so the dispatcher would report success on a write it had just
                // thrown away and the drop would never be counted. Under Wait, TryWrite returns
                // false when full - it still does not block, because we never call WriteAsync.
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            };

            this.channel = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(options);
        }

        public ChannelReader<Func<CancellationToken, ValueTask>> Reader => this.channel.Reader;

        public long DroppedCount => Interlocked.Read(ref this.droppedCount);

        /// <summary>
        /// Refusals after shutdown has closed the queue, counted apart from capacity refusals.
        /// Both look identical to TryWrite, but only one of them means the queue is too small -
        /// conflating them made the loss signal untrustworthy during a deploy, which is exactly
        /// when it gets read.
        /// </summary>
        public long RefusedAfterCloseCount => Interlocked.Read(ref this.refusedAfterCloseCount);

        public bool TryDispatch(Func<CancellationToken, ValueTask> work)
        {
            if (this.channel.Writer.TryWrite(work))
            {
                return true;
            }

            if (Volatile.Read(ref this.isCompleted))
            {
                Interlocked.Increment(ref this.refusedAfterCloseCount);

                return false;
            }

            Interlocked.Increment(ref this.droppedCount);

            return false;
        }

        /// <summary>Stops accepting work so the worker can drain what is left and finish.</summary>
        public void Complete()
        {
            Volatile.Write(ref this.isCompleted, true);
            this.channel.Writer.TryComplete();
        }
    }
}
