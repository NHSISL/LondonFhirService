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
using AbstractionExceptions = LondonFhirService.Core.Abstractions.Models.Metrics.Exceptions;
using ClientExceptions = LondonFhirService.Clients.AuditAndMetrics.Models.Metrics.Exceptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Metrics
{
    public partial class MetricServiceTests
    {
        [Theory]
        [MemberData(nameof(ClientExceptionMappings))]
        public async Task ShouldLocaliseClientExceptionOnAddMetricAndLogItAsync(
            Xeption clientException,
            Xeption expectedServiceException)
        {
            // given
            Metric randomMetric = CreateRandomMetric();

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.LogMetricAsync(It.IsAny<Metric>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(clientException);

            // when
            ValueTask addMetricTask =
                this.metricService.AddMetricAsync(randomMetric, TestContext.Current.CancellationToken);

            Xeption actualException =
                await Assert.ThrowsAsync(expectedServiceException.GetType(), addMetricTask.AsTask) as Xeption;

            // then
            actualException.Should().BeEquivalentTo(expectedServiceException);

            VerifyLoggedOnceAsCategory(expectedServiceException);
        }

        [Theory]
        [MemberData(nameof(ClientExceptionMappings))]
        public async Task ShouldLocaliseClientExceptionOnAddMetricsAndLogItAsync(
            Xeption clientException,
            Xeption expectedServiceException)
        {
            // given
            List<Metric> randomMetrics = CreateRandomMetrics();

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.LogMetricsAsync(It.IsAny<List<Metric>>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(clientException);

            // when
            ValueTask addMetricsTask =
                this.metricService.AddMetricsAsync(randomMetrics, TestContext.Current.CancellationToken);

            Xeption actualException =
                await Assert.ThrowsAsync(expectedServiceException.GetType(), addMetricsTask.AsTask) as Xeption;

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
        /// The metric client wraps dependency validation failures in the same
        /// MetricClientValidationException it uses for plain validation failures, unlike the audit
        /// client which keeps the two apart. If this service dispatched on the caught type alone,
        /// every duplicate, locked and bad-reference row would reach callers as a plain
        /// validation error.
        /// </summary>
        [Theory]
        [MemberData(nameof(DependencyValidationInnerExceptions))]
        public async Task ShouldSurfaceDependencyValidationWhenTheClientWrapsItAsValidationAsync(
            Xeption abstractionException,
            Type expectedCategorisedType)
        {
            // given
            Metric randomMetric = CreateRandomMetric();

            var dependencyValidationException =
                new ClientExceptions.MetricDependencyValidationException(
                    "Client dependency validation.", abstractionException);

            var clientException =
                new ClientExceptions.MetricClientValidationException(
                    "Client validation.", dependencyValidationException);

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.LogMetricAsync(It.IsAny<Metric>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(clientException);

            // when
            Func<Task> addMetric = async () =>
                await this.metricService.AddMetricAsync(
                    randomMetric, TestContext.Current.CancellationToken);

            // then
            var actualException =
                (await addMetric.Should().ThrowAsync<MetricServiceDependencyValidationException>()).Which;

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
            Func<Task> addMetric = async () =>
                await this.metricService.AddMetricAsync(
                    randomMetric, TestContext.Current.CancellationToken);

            // then
            // A caller that cancels gets the cancellation it asked for, not a service exception
            // it has to unwrap to find out what happened.
            (await addMetric.Should().ThrowAsync<OperationCanceledException>())
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
