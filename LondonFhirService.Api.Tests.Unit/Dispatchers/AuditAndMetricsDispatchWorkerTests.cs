// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Api.Dispatchers;
using LondonFhirService.Core.Abstractions.Brokers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace LondonFhirService.Api.Tests.Unit.Dispatchers
{
    /// <summary>
    /// The worker end of the queue. The dispatcher tests cover the bound; these cover the half
    /// that actually performs the writes, and the shutdown behaviour the design claims - that a
    /// write recorded moments before a deployment still lands.
    /// </summary>
    public class AuditAndMetricsDispatchWorkerTests
    {
        private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(5);

        private static (AuditAndMetricsDispatcher Dispatcher, AuditAndMetricsDispatchWorker Worker)
            Create(int capacity = 100, int concurrency = 2)
        {
            IOptions<AuditAndMetricsDispatcherSettings> settings = Options.Create(
                new AuditAndMetricsDispatcherSettings
                {
                    Capacity = capacity,
                    DrainConcurrency = concurrency,

                    // No grace window in most tests: the point being asserted is the drain, and
                    // waiting the production five seconds for each would only slow the suite.
                    ShutdownGraceSeconds = 0
                });

            var dispatcher = new AuditAndMetricsDispatcher(settings);

            var worker = new AuditAndMetricsDispatchWorker(
                dispatcher,
                Mock.Of<ILogger<AuditAndMetricsDispatchWorker>>(),
                settings);

            return (dispatcher, worker);
        }

        [Fact]
        public async Task ShouldRunQueuedWorkAsync()
        {
            // given
            (AuditAndMetricsDispatcher dispatcher, AuditAndMetricsDispatchWorker worker) = Create();
            var ran = new TaskCompletionSource();
            await worker.StartAsync(CancellationToken.None);

            // when
            dispatcher.TryDispatch(_ =>
            {
                ran.TrySetResult();

                return ValueTask.CompletedTask;
            });

            // then
            await ran.Task.WaitAsync(WaitTimeout);
            ran.Task.IsCompletedSuccessfully.Should().BeTrue();

            await worker.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task ShouldDrainWhatIsQueuedWhenTheHostStopsAsync()
        {
            // given
            (AuditAndMetricsDispatcher dispatcher, AuditAndMetricsDispatchWorker worker) =
                Create(concurrency: 1);

            var firstPickedUp = new TaskCompletionSource();
            var gate = new TaskCompletionSource();
            var completed = new ConcurrentQueue<int>();
            await worker.StartAsync(CancellationToken.None);

            // The first item blocks the single reader. Waiting for it to actually be picked up
            // before queueing the rest is what makes this deterministic: otherwise the reader
            // might drain everything before the stop even begins, and the test would be asserting
            // nothing.
            dispatcher.TryDispatch(async _ =>
            {
                firstPickedUp.TrySetResult();
                await gate.Task;
                completed.Enqueue(0);
            });

            await firstPickedUp.Task.WaitAsync(WaitTimeout);

            for (int index = 1; index <= 5; index++)
            {
                int captured = index;
                dispatcher.TryDispatch(_ =>
                {
                    completed.Enqueue(captured);

                    return ValueTask.CompletedTask;
                });
            }

            // when
            Task stopping = worker.StopAsync(CancellationToken.None);
            gate.TrySetResult();
            await stopping.WaitAsync(WaitTimeout);

            // then
            // All six, not just the one that was already running. Entries recorded moments before
            // a deployment are exactly the ones a detached thread pool item used to lose.
            completed.Should().HaveCount(6);
        }

        [Fact]
        public async Task ShouldKeepDrainingAfterAWriteThrowsAsync()
        {
            // given
            (AuditAndMetricsDispatcher dispatcher, AuditAndMetricsDispatchWorker worker) =
                Create(concurrency: 1);

            var ranAfterFailure = new TaskCompletionSource();
            await worker.StartAsync(CancellationToken.None);

            // when
            dispatcher.TryDispatch(_ => throw new InvalidOperationException("storage is down"));

            dispatcher.TryDispatch(_ =>
            {
                ranAfterFailure.TrySetResult();

                return ValueTask.CompletedTask;
            });

            // then
            // One failed write must not kill the reader. If it did, drain concurrency would decay
            // to zero over time and the queue would fill silently.
            await ranAfterFailure.Task.WaitAsync(WaitTimeout);
            ranAfterFailure.Task.IsCompletedSuccessfully.Should().BeTrue();

            await worker.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task ShouldKeepDrainingAfterAWriteIsCancelledAsync()
        {
            // given
            (AuditAndMetricsDispatcher dispatcher, AuditAndMetricsDispatchWorker worker) =
                Create(concurrency: 1);

            var ranAfterCancellation = new TaskCompletionSource();
            await worker.StartAsync(CancellationToken.None);

            // when
            dispatcher.TryDispatch(_ => throw new OperationCanceledException());

            dispatcher.TryDispatch(_ =>
            {
                ranAfterCancellation.TrySetResult();

                return ValueTask.CompletedTask;
            });

            // then
            await ranAfterCancellation.Task.WaitAsync(WaitTimeout);
            ranAfterCancellation.Task.IsCompletedSuccessfully.Should().BeTrue();

            await worker.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task ShouldKeepAcceptingWritesDuringTheShutdownGraceWindowAsync()
        {
            // given
            IOptions<AuditAndMetricsDispatcherSettings> settings = Options.Create(
                new AuditAndMetricsDispatcherSettings
                {
                    Capacity = 10,
                    DrainConcurrency = 1,
                    ShutdownGraceSeconds = 2
                });

            var dispatcher = new AuditAndMetricsDispatcher(settings);

            var worker = new AuditAndMetricsDispatchWorker(
                dispatcher,
                Mock.Of<ILogger<AuditAndMetricsDispatchWorker>>(),
                settings);

            var lateWriteRan = new TaskCompletionSource();
            await worker.StartAsync(CancellationToken.None);

            // when
            Task stopping = worker.StopAsync(CancellationToken.None);
            await Task.Delay(200);

            // A request still draining under Kestrel records a span after this worker was told
            // to stop. Hosted services stop before the web host finishes, so this is the normal
            // case on every deployment, not an edge one.
            bool accepted = dispatcher.TryDispatch(_ =>
            {
                lateWriteRan.TrySetResult();

                return ValueTask.CompletedTask;
            });

            await stopping.WaitAsync(WaitTimeout);

            // then
            accepted.Should().BeTrue(
                because: "the queue must stay open while in-flight requests are still recording");

            lateWriteRan.Task.IsCompletedSuccessfully.Should().BeTrue();
            dispatcher.DroppedCount.Should().Be(0);
        }

        [Fact]
        public void ShouldGiveTheWorkerAndTheWritersTheSameQueue()
        {
            // given
            var services = new ServiceCollection();
            services.Configure<AuditAndMetricsDispatcherSettings>(_ => { });
            services.AddSingleton<AuditAndMetricsDispatcher>();

            services.AddSingleton<IAuditAndMetricsDispatcher>(provider =>
                provider.GetRequiredService<AuditAndMetricsDispatcher>());

            using ServiceProvider provider = services.BuildServiceProvider();

            // when
            var concrete = provider.GetRequiredService<AuditAndMetricsDispatcher>();
            var viaPort = provider.GetRequiredService<IAuditAndMetricsDispatcher>();

            // then
            // Registering the concrete type and the interface separately rather than forwarding
            // would give the worker a different queue from the writers, and nothing would ever
            // drain - with no error anywhere to say so.
            viaPort.Should().BeSameAs(concrete);
        }
    }
}
