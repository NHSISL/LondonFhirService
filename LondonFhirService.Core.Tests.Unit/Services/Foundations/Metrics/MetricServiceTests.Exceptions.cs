// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Models.Foundations.Metrics.Exceptions;
using Moq;
using Xeptions;
using AbstractionExceptions = NHSOneLondon.AuditAndMetrics.Abstractions.Models.Metrics.Exceptions;
using ClientExceptions = NHSOneLondon.AuditAndMetrics.Clients.Models.Metrics.Exceptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Metrics
{
    public partial class MetricServiceTests
    {
        [Theory]
        [MemberData(nameof(ClientExceptionMappings))]
        public async Task ShouldLocaliseClientExceptionOnLogMetricAndLogItAsync(
            Xeption clientException,
            Xeption expectedServiceException)
        {
            // given
            Metric randomMetric = CreateRandomMetric();

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.LogMetricAsync(It.IsAny<Metric>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(clientException);

            // when
            ValueTask logMetricTask =
                this.metricService.LogMetricAsync(randomMetric, TestContext.Current.CancellationToken);

            Xeption actualException =
                await Assert.ThrowsAsync(expectedServiceException.GetType(), logMetricTask.AsTask) as Xeption;

            // then
            actualException.Should().BeEquivalentTo(expectedServiceException);

            VerifyLoggedOnceAsCategory(expectedServiceException);
        }

        [Theory]
        [MemberData(nameof(ClientExceptionMappings))]
        public async Task ShouldLocaliseClientExceptionOnLogMetricsAndLogItAsync(
            Xeption clientException,
            Xeption expectedServiceException)
        {
            // given
            List<Metric> randomMetrics = CreateRandomMetrics();

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.LogMetricsAsync(It.IsAny<List<Metric>>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(clientException);

            // when
            ValueTask logMetricsTask =
                this.metricService.LogMetricsAsync(randomMetrics, TestContext.Current.CancellationToken);

            Xeption actualException =
                await Assert.ThrowsAsync(expectedServiceException.GetType(), logMetricsTask.AsTask) as Xeption;

            // then
            actualException.Should().BeEquivalentTo(expectedServiceException);

            VerifyLoggedOnceAsCategory(expectedServiceException);
        }

        [Theory]
        [MemberData(nameof(ClientExceptionMappings))]
        public async Task ShouldLocaliseClientExceptionOnPurgeAndLogItAsync(
            Xeption clientException,
            Xeption expectedServiceException)
        {
            // given
            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.PurgeMetricsOlderThanRetentionPeriodAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(clientException);

            // when
            ValueTask<int> purgeTask = this.metricService
                .PurgeMetricsOlderThanRetentionPeriodAsync(TestContext.Current.CancellationToken);

            Xeption actualException =
                await Assert.ThrowsAsync(expectedServiceException.GetType(), purgeTask.AsTask) as Xeption;

            // then
            actualException.Should().BeEquivalentTo(expectedServiceException);

            VerifyLoggedOnceAsCategory(expectedServiceException);
        }

        /// <summary>
        /// The metric client reports a duplicate, locked or bad-reference row as its own
        /// MetricClientDependencyValidationException, carrying the storage exception the host's
        /// broker raised. Each has to reach callers as this service's dependency validation
        /// category with the matching categorised inner exception, because the controllers choose
        /// a status code from it - a duplicate is not the same answer as a malformed span.
        /// </summary>
        [Theory]
        [MemberData(nameof(DependencyValidationInnerExceptions))]
        public async Task ShouldLocaliseClientDependencyValidationIntoItsCategoryAsync(
            Xeption abstractionException,
            Type expectedCategorisedType)
        {
            // given
            Metric randomMetric = CreateRandomMetric();

            var clientException =
                new ClientExceptions.MetricClientDependencyValidationException(
                    "Client dependency validation.", abstractionException);

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.LogMetricAsync(It.IsAny<Metric>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(clientException);

            // when
            Func<Task> logMetric = async () =>
                await this.metricService.LogMetricAsync(
                    randomMetric, TestContext.Current.CancellationToken);

            // then
            var actualException =
                (await logMetric.Should().ThrowAsync<MetricServiceDependencyValidationException>()).Which;

            actualException.InnerException.Should().BeOfType(expectedCategorisedType);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Xeption>()),
                    Times.Once);
        }

        public static TheoryData<Xeption, Type> DependencyValidationInnerExceptions()
        {
            var innerException = new Exception("Inner.");

            return new TheoryData<Xeption, Type>
            {
                {
                    new AbstractionExceptions.AlreadyExistsMetricException(
                        "Already exists.", innerException, innerException.Data),
                    typeof(AlreadyExistsMetricServiceException)
                },
                {
                    new AbstractionExceptions.InvalidReferenceMetricException(
                        "Invalid reference.", innerException, innerException.Data),
                    typeof(InvalidReferenceMetricServiceException)
                },
                {
                    new AbstractionExceptions.LockedMetricException(
                        "Locked.", innerException, innerException.Data),
                    typeof(LockedMetricServiceException)
                }
            };
        }

        [Fact]
        public async Task ShouldNotTranslateCancellationAsync()
        {
            // given
            Metric randomMetric = CreateRandomMetric();
            var operationCanceledException = new OperationCanceledException();

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.LogMetricAsync(It.IsAny<Metric>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(operationCanceledException);

            // when
            Func<Task> logMetric = async () =>
                await this.metricService.LogMetricAsync(
                    randomMetric, TestContext.Current.CancellationToken);

            // then
            // A caller that cancels gets the cancellation it asked for, not a service exception
            // it has to unwrap to find out what happened.
            (await logMetric.Should().ThrowAsync<OperationCanceledException>())
                .Which.Should().BeSameAs(operationCanceledException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Xeption>()),
                    Times.Never);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.IsAny<Xeption>()),
                    Times.Never);
        }

        private void VerifyLoggedOnceAsCategory(Xeption expectedServiceException)
        {
            if (expectedServiceException is MetricServiceDependencyException)
            {
                this.loggingBrokerMock.Verify(broker =>
                    broker.LogCriticalAsync(It.Is(SameExceptionAs(expectedServiceException))),
                        Times.Once);

                return;
            }

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedServiceException))),
                    Times.Once);
        }
    }
}
