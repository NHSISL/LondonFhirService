// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Models.Metrics;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Services.Foundations.Metrics
{
    /// <summary>
    /// A dispatched write measures work it must never break. The whole reason these verbs exist
    /// is that recording something should cost the caller nothing - and failing is not nothing.
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
            // This span is recorded from inside the patient request it is measuring. A bad span
            // must cost that request nothing - it is telemetry, not the work the caller came for.
            await logMetric.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ShouldNotThrowIntoTheCallerWhenADispatchedMetricWriteFailsAsync()
        {
            // given
            IMetric randomMetric = CreateRandomMetric();

            this.dateTimeBrokerMock.Setup(broker => broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(randomMetric.CreatedDate);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertMetricAsync(randomMetric, TestContext.Current.CancellationToken))
                    .ThrowsAsync(new Exception("storage is down"));

            // when
            Func<Task> logMetric = async () =>
                await this.metricService.LogMetricAsync(
                    randomMetric, TestContext.Current.CancellationToken);

            // then
            // Storage being down loses the span. It must not also lose the request.
            await logMetric.Should().NotThrowAsync();
        }
    }
}
