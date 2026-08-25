// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using Moq;

namespace LondonFhirService.Clients.AuditAndMetrics.Tests.Unit.Services.Foundations.Metrics
{
    public partial class MetricServiceTests
    {
        [Fact]
        public async Task ShouldStampCurrentUserOnAddMetricAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            IMetric randomMetric = CreateRandomMetric(randomDateTimeOffset);
            randomMetric.UserId = null;
            IMetric inputMetric = randomMetric;
            string randomUserId = GetRandomString();
            string randomDisplayName = GetRandomString();

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.auditUserBrokerMock.Setup(broker =>
                broker.GetCurrentUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.auditUserBrokerMock.Setup(broker =>
                broker.GetCurrentUserDisplayNameAsync())
                    .ReturnsAsync(randomDisplayName);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertMetricAsync(inputMetric, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(inputMetric);

            // when
            await this.metricService.AddMetricAsync(inputMetric, TestContext.Current.CancellationToken);

            // then
            inputMetric.UserId.Should().Be(randomUserId);
            inputMetric.Consumer.Should().Be(randomDisplayName);

            VerifyCurrentUserResolvedOnce();
        }

        [Fact]
        public async Task ShouldResolveTheUserOnceForTheWholeBatchOnAddMetricsAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            List<IMetric> randomMetrics = CreateRandomMetrics(randomDateTimeOffset);

            foreach (IMetric metric in randomMetrics)
            {
                metric.UserId = null;
            }

            List<IMetric> inputMetrics = randomMetrics;
            string randomUserId = GetRandomString();
            string randomDisplayName = GetRandomString();

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.auditUserBrokerMock.Setup(broker =>
                broker.GetCurrentUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.auditUserBrokerMock.Setup(broker =>
                broker.GetCurrentUserDisplayNameAsync())
                    .ReturnsAsync(randomDisplayName);

            // when
            await this.metricService.AddMetricsAsync(inputMetrics, TestContext.Current.CancellationToken);

            // then
            inputMetrics.Should().OnlyContain(metric => metric.UserId == randomUserId);
            inputMetrics.Should().OnlyContain(metric => metric.Consumer == randomDisplayName);
    inputMetrics.Should().OnlyContain(metric => metric.Consumer == randomDisplayName);

            // The point of the exercise: one lookup for the batch, however many spans it holds.
            VerifyCurrentUserResolvedOnce();
        }

        [Fact]
        public async Task ShouldStampCurrentUserOnLogMetricAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            IMetric randomMetric = CreateRandomMetric(randomDateTimeOffset);
            randomMetric.UserId = string.Empty;
            IMetric inputMetric = randomMetric;
            string randomUserId = GetRandomString();
            string randomDisplayName = GetRandomString();

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.auditUserBrokerMock.Setup(broker =>
                broker.GetCurrentUserIdAsync())
                    .ReturnsAsync(randomUserId);

            this.auditUserBrokerMock.Setup(broker =>
                broker.GetCurrentUserDisplayNameAsync())
                    .ReturnsAsync(randomDisplayName);

            this.storageBrokerMock.Setup(broker =>
                broker.InsertMetricAsync(inputMetric, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(inputMetric);

            // when
            await this.metricService.LogMetricAsync(inputMetric, TestContext.Current.CancellationToken);

            // then
            inputMetric.UserId.Should().Be(randomUserId);
            inputMetric.Consumer.Should().Be(randomDisplayName);

            VerifyCurrentUserResolvedOnce();
        }
    }
}
