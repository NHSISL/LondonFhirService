// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Models.Foundations.Providers;
using Moq;
using Xeptions;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Patients.STU3
{
    /// <summary>
    /// The orchestration slice of the span tree. No Orchestration span is recorded any more - it
    /// wrapped this layer end to end and carried nothing that could not be derived - so
    /// AccessCheck and ProviderRequests hang directly off the request root passed in from the
    /// coordination service.
    /// </summary>
    public partial class Stu3PatientOrchestrationServiceTests
    {
        [Fact]
        public async Task ShouldHangAccessCheckAndProviderRequestsOffTheRequestRootAsync()
        {
            // given
            string inputNhsNumber = GetRandomString();
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            Guid inputParentId = Guid.NewGuid();
            Guid accessCheckSpanId = Guid.NewGuid();
            Guid providerRequestsSpanId = Guid.NewGuid();
            Guid discoverySpanId = Guid.NewGuid();
            // The provider fixture is built around the same instant the clock is pinned to, since
            // the activation window is now evaluated against the injected clock.
            DateTimeOffset startedAt = GetRandomDateTimeOffset();
            string userId = GetRandomString();
            User randomUser = CreateRandomUser(userId);
            ConsumerAccess allowedAccess = CreateRandomConsumerAccess(isAccessAllowed: true);
            Provider primaryProvider = CreateRandomPrimaryProvider(now: startedAt);
            List<(string Provider, string Json)> randomBundles = CreateRandomBundles();
            var recordedMetrics = new List<Metric>();

            // Sequenced so the ids can be told apart: the access check is drawn first, then
            // provider requests, then discovery.
            this.identifierBrokerMock.SetupSequence(broker => broker.GetIdentifierAsync())
                .ReturnsAsync(accessCheckSpanId)
                .ReturnsAsync(providerRequestsSpanId)
                .ReturnsAsync(discoverySpanId);

            this.dateTimeBrokerMock.Setup(broker => broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(startedAt);

            this.securityBrokerMock.Setup(broker => broker.GetCurrentUserAsync())
                .ReturnsAsync(randomUser);

            this.consumerAccessServiceMock.Setup(service =>
                service.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(allowedAccess);

            this.providerServiceMock.Setup(service =>
                service.RetrieveAllProvidersAsListAsync())
                    .ReturnsAsync(new List<Provider> { primaryProvider });

            this.patientServiceMock.Setup(service =>
                service.GetStructuredRecordSerialisedAsync(
                    It.IsAny<List<Provider>>(),
                    correlationId,
                    inputNhsNumber,
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(randomBundles);

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.LogMetricAsync(It.IsAny<Metric>(), It.IsAny<CancellationToken>()))
                    .Callback<Metric, CancellationToken>((metric, _) => recordedMetrics.Add(metric));

            // when
            await this.patientOrchestrationService.GetStructuredRecordSerialisedAsync(
                correlationId,
                inputNhsNumber,
                parentId: inputParentId,
                cancellationToken: cancellationToken);

            // then
            recordedMetrics.Should().NotContain(metric => metric.Type == MetricType.Orchestration);

            Metric accessCheckSpan = recordedMetrics.Should().ContainSingle(metric =>
                metric.Type == MetricType.AccessCheck).Subject;

            Metric providerRequestsSpan = recordedMetrics.Should().ContainSingle(metric =>
                metric.Type == MetricType.ProviderRequests).Subject;

            Metric discoverySpan = recordedMetrics.Should().ContainSingle(metric =>
                metric.Type == MetricType.ProviderDiscovery).Subject;

            // Both hang off the request root now that the layer span between them is gone.
            accessCheckSpan.Id.Should().Be(accessCheckSpanId);
            accessCheckSpan.ParentId.Should().Be(inputParentId);

            providerRequestsSpan.Id.Should().Be(providerRequestsSpanId);
            providerRequestsSpan.ParentId.Should().Be(inputParentId);

            discoverySpan.Id.Should().Be(discoverySpanId);
            discoverySpan.ParentId.Should().Be(providerRequestsSpanId);

            recordedMetrics.Should().OnlyContain(metric => metric.CorrelationId == correlationId);
            recordedMetrics.Should().OnlyContain(metric => metric.Status == MetricStatus.Succeeded);

            // Completed is Started plus the duration by construction, never a second clock read.
            recordedMetrics.Should().OnlyContain(metric =>
                metric.Completed == metric.Started.AddMilliseconds(metric.DurationMs));

            // The layer below is told which span encloses it.
            this.patientServiceMock.Verify(service =>
                service.GetStructuredRecordSerialisedAsync(
                    It.IsAny<List<Provider>>(),
                    correlationId,
                    inputNhsNumber,
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    providerRequestsSpanId,
                    It.IsAny<CancellationToken>()),
                        Times.Once);
        }

        [Fact]
        public async Task ShouldRecordProviderRequestsAsFailedWhenTheRequestFailsAsync()
        {
            // given
            string inputNhsNumber = GetRandomString();
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            Guid inputParentId = Guid.NewGuid();
            Guid accessCheckSpanId = Guid.NewGuid();
            Guid providerRequestsSpanId = Guid.NewGuid();
            string userId = GetRandomString();
            User randomUser = CreateRandomUser(userId);
            ConsumerAccess allowedAccess = CreateRandomConsumerAccess(isAccessAllowed: true);
            Provider primaryProvider = CreateRandomPrimaryProvider();
            var recordedMetrics = new List<Metric>();
            var serviceException = new Exception(GetRandomString());

            this.identifierBrokerMock.SetupSequence(broker => broker.GetIdentifierAsync())
                .ReturnsAsync(accessCheckSpanId)
                .ReturnsAsync(providerRequestsSpanId)
                .ReturnsAsync(Guid.NewGuid())
                .ReturnsAsync(Guid.NewGuid());

            this.securityBrokerMock.Setup(broker => broker.GetCurrentUserAsync())
                .ReturnsAsync(randomUser);

            this.consumerAccessServiceMock.Setup(service =>
                service.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(allowedAccess);

            this.providerServiceMock.Setup(service =>
                service.RetrieveAllProvidersAsListAsync())
                    .ReturnsAsync(new List<Provider> { primaryProvider });

            this.patientServiceMock.Setup(service =>
                service.GetStructuredRecordSerialisedAsync(
                    It.IsAny<List<Provider>>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(serviceException);

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.LogMetricAsync(It.IsAny<Metric>(), It.IsAny<CancellationToken>()))
                    .Callback<Metric, CancellationToken>((metric, _) => recordedMetrics.Add(metric));

            // when
            Func<Task> getStructuredRecord = async () =>
                await this.patientOrchestrationService.GetStructuredRecordSerialisedAsync(
                    correlationId,
                    inputNhsNumber,
                    parentId: inputParentId,
                    cancellationToken: cancellationToken);

            await getStructuredRecord.Should().ThrowAsync<Xeption>();

            // then
            recordedMetrics.Should().NotContain(metric => metric.Type == MetricType.Orchestration);

            // ProviderRequests still has to be written on the way out, or the discovery span
            // already recorded points at a row that never got inserted.
            Metric providerRequestsSpan = recordedMetrics.Should().ContainSingle(metric =>
                metric.Type == MetricType.ProviderRequests).Subject;

            providerRequestsSpan.Id.Should().Be(providerRequestsSpanId);
            providerRequestsSpan.ParentId.Should().Be(inputParentId);
            providerRequestsSpan.Status.Should().Be(MetricStatus.Failed);
            providerRequestsSpan.ErrorCode.Should().Be(nameof(Exception));
        }
    }
}
