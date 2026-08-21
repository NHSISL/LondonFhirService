// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Clients.AuditAndMetrics.Clients;
using LondonFhirService.Clients.AuditAndMetrics.Clients.Audits;
using LondonFhirService.Clients.AuditAndMetrics.Clients.Metrics;
using LondonFhirService.Core.Brokers.AuditAndMetrics;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.Audits;
using Moq;
using Tynamix.ObjectFiller;
using IAudit = LondonFhirService.Core.Abstractions.Models.Audits.IAudit;

namespace LondonFhirService.Core.Tests.Unit.Brokers.AuditAndMetrics
{
    /// <summary>
    /// The dispatched writes are the only ones whose failures nobody is waiting for, so what the
    /// broker does with an exception is the whole of their error handling.
    ///
    /// These tests wait on a signal rather than a delay: the work happens on a thread pool thread,
    /// and asserting immediately after the call would pass whether or not the code is correct.
    /// </summary>
    public class AuditAndMetricBrokerTests
    {
        private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(5);

        private readonly Mock<IAuditAndMetricsClient> auditAndMetricsClientMock;
        private readonly Mock<IAuditClient> auditClientMock;
        private readonly Mock<IMetricClient> metricClientMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IAuditAndMetricBroker auditAndMetricBroker;

        public AuditAndMetricBrokerTests()
        {
            this.auditClientMock = new Mock<IAuditClient>();
            this.metricClientMock = new Mock<IMetricClient>();
            this.auditAndMetricsClientMock = new Mock<IAuditAndMetricsClient>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.auditAndMetricsClientMock.SetupGet(client => client.AuditClient)
                .Returns(this.auditClientMock.Object);

            this.auditAndMetricsClientMock.SetupGet(client => client.MetricClient)
                .Returns(this.metricClientMock.Object);

            this.auditAndMetricBroker = new AuditAndMetricBroker(
                auditAndMetricsClient: this.auditAndMetricsClientMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        [Fact]
        public async Task ShouldLogAnUnexpectedFailureFromADispatchedWriteAsync()
        {
            // given
            Audit randomAudit = CreateRandomAudit();
            var storageException = new Exception(GetRandomString());
            var loggedException = new TaskCompletionSource<Exception>();

            this.auditClientMock.Setup(client =>
                client.LogAuditAsync(It.IsAny<IAudit>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(storageException);

            this.loggingBrokerMock.Setup(broker => broker.LogErrorAsync(It.IsAny<Exception>()))
                .Callback<Exception>(exception => loggedException.TrySetResult(exception));

            // when
            await this.auditAndMetricBroker.LogAuditAsync(randomAudit);

            // then
            // Nobody is awaiting the write, so the log line is the only trace a lost audit entry
            // leaves. If this stopped working, entries would vanish in silence.
            Task<Exception> completed = await Task.WhenAny(
                loggedException.Task, Task.Delay(WaitTimeout).ContinueWith<Exception>(_ => null));

            completed.Should().BeSameAs(loggedException.Task,
                because: "an unexpected failure in a dispatched write must be logged");

            (await loggedException.Task).Should().BeSameAs(storageException);
        }

        [Fact]
        public async Task ShouldNotLogCancellationFromADispatchedWriteAsync()
        {
            // given
            Audit randomAudit = CreateRandomAudit();
            var cancelledWrite = new TaskCompletionSource();
            var loggedAnything = new TaskCompletionSource();

            this.auditClientMock.Setup(client =>
                client.LogAuditAsync(It.IsAny<IAudit>(), It.IsAny<CancellationToken>()))
                    .Callback(() => cancelledWrite.TrySetResult())
                    .ThrowsAsync(new OperationCanceledException());

            this.loggingBrokerMock.Setup(broker => broker.LogErrorAsync(It.IsAny<Exception>()))
                .Callback<Exception>(_ => loggedAnything.TrySetResult());

            this.loggingBrokerMock.Setup(broker => broker.LogCriticalAsync(It.IsAny<Exception>()))
                .Callback<Exception>(_ => loggedAnything.TrySetResult());

            // when
            await this.auditAndMetricBroker.LogAuditAsync(randomAudit);
            await cancelledWrite.Task.WaitAsync(WaitTimeout);

            // then
            // A cancelled request or a host shutting down cancels the token these writes carry.
            // Reporting that as an error puts a line in the log every time a client disconnects,
            // which buries the failures that do matter.
            Task first = await Task.WhenAny(loggedAnything.Task, Task.Delay(WaitTimeout));

            first.Should().NotBeSameAs(loggedAnything.Task,
                because: "cancellation is expected and must not be logged as a failure");
        }

        private static Audit CreateRandomAudit() =>
            new Audit
            {
                Id = Guid.NewGuid(),
                AuditType = GetRandomString(),
                CorrelationId = GetRandomString()
            };

        private static string GetRandomString() =>
            new MnemonicString(wordCount: new IntRange(min: 2, max: 10).GetValue()).GetValue();
    }
}
