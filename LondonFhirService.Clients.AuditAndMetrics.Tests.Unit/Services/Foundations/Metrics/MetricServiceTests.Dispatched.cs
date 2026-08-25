// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Models.Metrics;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using Moq;
using Xeptions;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Services.Foundations.Metrics
{
    /// <summary>
    /// A dispatched write measures work it must never break. The whole reason these verbs exist
    /// is that recording something costs the caller nothing - and failing is not nothing.
    /// </summary>
    public partial class MetricServiceTests
    {
        [Fact]
        public async Task ShouldNotThrowIntoTheCallerWhenADispatchedMetricIsInvalidAsync()
        {
            // given
            // Missing everything the validator requires.
            IMetric invalidMetric = new TestMetric();

            this.dateTimeBrokerMock.Setup(broker => broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(GetRandomDateTimeOffset());

            // when
            Func<Task> logMetric = async () =>
                await this.metricService.LogMetricAsync(
                    invalidMetric, TestContext.Current.CancellationToken);

            // then
            // This span is recorded from inside the patient request it measures. A bad span must
            // cost that request nothing - it is telemetry, not the work the caller came for.
            await logMetric.Should().NotThrowAsync();

            // Swallowed, not ignored - the localisation still logged it exactly once.
            // Without this the test would also pass if the validation never ran at all.
            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Xeption>()),
                    Times.Once);
        }

        [Fact]
        public async Task ShouldNotThrowIntoTheCallerWhenADispatchedMetricWriteFailsAsync()
        {
            // given
            IMetric randomMetric = CreateRandomMetric();
            var storageException = new Exception("storage is down");

            this.dateTimeBrokerMock.Setup(broker => broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(randomMetric.CreatedDate);

            // CancellationToken.None, because that is the token the dispatcher hands the deferred
            // work - a deferred write must not carry the request's token, or it would be
            // cancelled the moment the response is sent. Keying this setup on the ambient test
            // token instead would never match, the write would never fail, and the test would
            // pass while proving nothing.
            this.metricBrokerMock.Setup(broker =>
                broker.InsertMetricAsync(randomMetric, CancellationToken.None))
                    .ThrowsAsync(storageException);

            // when
            Func<Task> logMetric = async () =>
                await this.metricService.LogMetricAsync(
                    randomMetric, TestContext.Current.CancellationToken);

            // then
            // Storage being down loses the span. It must not also lose the request.
            await logMetric.Should().NotThrowAsync();

            // One line, not two: the localisation logs, the swallow does not log again.
            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Xeption>()),
                    Times.Once);
        }
    }
}
