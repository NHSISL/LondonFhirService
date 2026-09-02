// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Clients.AuditAndMetrics.Brokers.Dispatchers;
using LondonFhirService.Clients.AuditAndMetrics.Brokers.Loggings;
using LondonFhirService.Core.Abstractions.Brokers;
using Moq;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Brokers
{
    /// <summary>
    /// The fallback dispatcher. Nobody awaits the work it runs, so what it does with an exception
    /// is the whole of the error handling for a deferred write.
    ///
    /// These wait on a signal rather than a delay: the work runs on a thread pool thread, and
    /// asserting straight after the call would pass whether or not the code is right.
    /// </summary>
    public class ThreadPoolDispatcherTests
    {
        private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(5);

        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IAuditAndMetricsDispatcher dispatcher;

        public ThreadPoolDispatcherTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();
            this.dispatcher = new ThreadPoolDispatcher(this.loggingBrokerMock.Object);
        }

        [Fact]
        public async Task ShouldLogAnUnexpectedFailureFromADispatchedWriteAsync()
        {
            // given
            var storageException = new Exception("storage unavailable");
            var loggedException = new TaskCompletionSource<Exception>();

            this.loggingBrokerMock.Setup(broker => broker.LogErrorAsync(It.IsAny<Exception>()))
                .Callback<Exception>(exception => loggedException.TrySetResult(exception));

            // when
            bool accepted = this.dispatcher.TryDispatch(_ => throw storageException);

            // then
            // Nobody is awaiting the write, so this log line is the only trace a lost audit entry
            // leaves. Unhandled it would surface as an unobserved task exception instead.
            accepted.Should().BeTrue();

            // WaitAsync rather than a delay raced in Task.WhenAny: a cancelled run then ends as
            // cancelled instead of completing the delay first and reading as a timeout.
            Exception actualLoggedException =
                await loggedException.Task.WaitAsync(WaitTimeout, TestContext.Current.CancellationToken);

            actualLoggedException.Should().BeSameAs(storageException,
                because: "an unexpected failure in a dispatched write must be logged");
        }

        [Fact]
        public async Task ShouldNotLogCancellationFromADispatchedWriteAsync()
        {
            // given
            var ranWork = new TaskCompletionSource();
            var loggedAnything = new TaskCompletionSource();

            this.loggingBrokerMock.Setup(broker => broker.LogErrorAsync(It.IsAny<Exception>()))
                .Callback<Exception>(_ => loggedAnything.TrySetResult());

            // when
            this.dispatcher.TryDispatch(_ =>
            {
                ranWork.TrySetResult();

                throw new OperationCanceledException();
            });

            await ranWork.Task.WaitAsync(WaitTimeout, TestContext.Current.CancellationToken);

            // then
            // A cancelled request or a host shutting down cancels these writes. Reporting that as
            // an error puts a line in the log every time a client disconnects, which buries the
            // failures that do matter.
            // The wait itself, not a delay raced against the signal: raced, a cancelled run
            // completes the delay early and the assertion below passes without ever waiting.
            await Task.Delay(WaitTimeout, TestContext.Current.CancellationToken);

            loggedAnything.Task.IsCompleted.Should().BeFalse(
                because: "cancellation is expected and must not be logged as a failure");
        }

        [Fact]
        public void ShouldNeverRefuseWork()
        {
            // given, when
            bool accepted = this.dispatcher.TryDispatch(_ => ValueTask.CompletedTask);

            // then
            // The thread pool has no bound to hit, which is exactly why a host should supply a
            // dispatcher that does.
            accepted.Should().BeTrue();
        }
    }
}
