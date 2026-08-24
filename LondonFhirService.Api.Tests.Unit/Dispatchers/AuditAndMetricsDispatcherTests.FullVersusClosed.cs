// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Api.Dispatchers;

namespace LondonFhirService.Api.Tests.Unit.Dispatchers
{
    /// <summary>
    /// A full queue and a closed one both make TryDispatch return false, but they mean opposite
    /// things: one says the queue is too small, the other says the host is shutting down. They
    /// used to share a counter and a warning, so every deploy produced capacity warnings for a
    /// queue that was merely closed - making the loss signal untrustworthy exactly when it is
    /// being read.
    /// </summary>
    public partial class AuditAndMetricsDispatcherTests
    {
        [Fact]
        public void ShouldCountACapacityRefusalAsDroppedAndNotAsClosed()
        {
            // given
            AuditAndMetricsDispatcher dispatcher = CreateDispatcher(capacity: 1);
            dispatcher.TryDispatch(_ => ValueTask.CompletedTask);

            // when
            bool accepted = dispatcher.TryDispatch(_ => ValueTask.CompletedTask);

            // then
            accepted.Should().BeFalse();
            dispatcher.DroppedCount.Should().Be(1);
            dispatcher.RefusedAfterCloseCount.Should().Be(0);
        }

        [Fact]
        public void ShouldCountAShutdownRefusalAsClosedAndNotAsDropped()
        {
            // given
            // Capacity is deliberately ample, so nothing here can be mistaken for a full queue.
            AuditAndMetricsDispatcher dispatcher = CreateDispatcher(capacity: 100);
            dispatcher.Complete();

            // when
            bool accepted = dispatcher.TryDispatch(_ => ValueTask.CompletedTask);

            // then
            accepted.Should().BeFalse();
            dispatcher.RefusedAfterCloseCount.Should().Be(1);
            dispatcher.DroppedCount.Should().Be(0);
        }

        [Fact]
        public void ShouldKeepTheTwoRefusalCountsSeparateAcrossAShutdown()
        {
            // given
            // One genuine capacity refusal first, then the host stops accepting.
            AuditAndMetricsDispatcher dispatcher = CreateDispatcher(capacity: 1);
            dispatcher.TryDispatch(_ => ValueTask.CompletedTask);
            dispatcher.TryDispatch(_ => ValueTask.CompletedTask);

            // when
            dispatcher.Complete();
            dispatcher.TryDispatch(_ => ValueTask.CompletedTask);
            dispatcher.TryDispatch(_ => ValueTask.CompletedTask);

            // then
            // The capacity count must not keep climbing once the queue is closed, or a deploy
            // reads as a capacity problem that needs a bigger queue.
            dispatcher.DroppedCount.Should().Be(1);
            dispatcher.RefusedAfterCloseCount.Should().Be(2);
        }
    }
}
