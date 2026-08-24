// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Api.Dispatchers;
using Microsoft.Extensions.Options;

namespace LondonFhirService.Api.Tests.Unit.Dispatchers
{
    /// <summary>
    /// The bound is the whole point: a thread pool item per write was fine at low volume and
    /// wrong under load, because a request records a span per provider call plus the tracing
    /// around it. These pin that the queue actually refuses rather than growing, and that a
    /// refusal is counted rather than silent.
    /// </summary>
    public partial class AuditAndMetricsDispatcherTests
    {
        private static AuditAndMetricsDispatcher CreateDispatcher(int capacity) =>
            new AuditAndMetricsDispatcher(
                Options.Create(new AuditAndMetricsDispatcherSettings { Capacity = capacity }));

        [Fact]
        public void ShouldRefuseWorkOnceTheQueueIsFull()
        {
            // given
            AuditAndMetricsDispatcher dispatcher = CreateDispatcher(capacity: 2);

            // when
            bool first = dispatcher.TryDispatch(_ => ValueTask.CompletedTask);
            bool second = dispatcher.TryDispatch(_ => ValueTask.CompletedTask);
            bool third = dispatcher.TryDispatch(_ => ValueTask.CompletedTask);

            // then
            // Nothing is draining, so the third has nowhere to go. Refusing is the point - an
            // unbounded queue under load is the defect this replaced.
            first.Should().BeTrue();
            second.Should().BeTrue();
            third.Should().BeFalse();
        }

        [Fact]
        public void ShouldCountWhatItDropped()
        {
            // given
            AuditAndMetricsDispatcher dispatcher = CreateDispatcher(capacity: 1);
            dispatcher.TryDispatch(_ => ValueTask.CompletedTask);

            // when
            dispatcher.TryDispatch(_ => ValueTask.CompletedTask);
            dispatcher.TryDispatch(_ => ValueTask.CompletedTask);

            // then
            // A dropped entry is lost either way; the count is what stops it being lost silently.
            dispatcher.DroppedCount.Should().Be(2);
        }

        [Fact]
        public async Task ShouldHandQueuedWorkToAReaderAsync()
        {
            // given
            AuditAndMetricsDispatcher dispatcher = CreateDispatcher(capacity: 4);
            var ran = new TaskCompletionSource();

            // when
            bool accepted = dispatcher.TryDispatch(_ =>
            {
                ran.TrySetResult();

                return ValueTask.CompletedTask;
            });

            await foreach (Func<CancellationToken, ValueTask> work in
                dispatcher.Reader.ReadAllAsync(CancellationToken.None))
            {
                await work(CancellationToken.None);

                break;
            }

            // then
            accepted.Should().BeTrue();
            ran.Task.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldLetTheReaderFinishOnceCompletedAsync()
        {
            // given
            AuditAndMetricsDispatcher dispatcher = CreateDispatcher(capacity: 4);
            dispatcher.TryDispatch(_ => ValueTask.CompletedTask);
            dispatcher.TryDispatch(_ => ValueTask.CompletedTask);

            // when
            dispatcher.Complete();
            int drained = 0;

            await foreach (Func<CancellationToken, ValueTask> work in
                dispatcher.Reader.ReadAllAsync(CancellationToken.None))
            {
                await work(CancellationToken.None);
                drained++;
            }

            // then
            // Completing stops new work but leaves what is queued readable, which is what lets
            // the worker drain entries recorded moments before a shutdown instead of losing them.
            drained.Should().Be(2);
        }

        [Fact]
        public void ShouldRefuseWorkOnceCompleted()
        {
            // given
            AuditAndMetricsDispatcher dispatcher = CreateDispatcher(capacity: 4);
            dispatcher.Complete();

            // when
            bool accepted = dispatcher.TryDispatch(_ => ValueTask.CompletedTask);

            // then
            // Counted as a shutdown refusal, not a capacity drop. Only one of the two means the
            // queue is too small, and conflating them made the warning misleading during a deploy.
            accepted.Should().BeFalse();
            dispatcher.RefusedAfterCloseCount.Should().Be(1);
            dispatcher.DroppedCount.Should().Be(0);
        }
    }
}
