// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Models.Foundations.Providers;
using Moq;
using Xeptions;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Patients.STU3
{
    /// <summary>
    /// The foundation slice of the span tree. The Foundation span carries the figure the
    /// "Foundation Service Request Completed in Nms" audit line used to carry; the fan out hangs
    /// off it, each provider hangs off the fan out, and the call and the deferred persist hang
    /// off their provider. Persist is the one span whose duration is deliberately outside its
    /// ancestors' durations.
    /// </summary>
    public partial class Stu3PatientServiceTests
    {
        [Fact]
        public async Task ShouldRecordTheFoundationSpanWithTheFanOutUnderItAsync()
        {
            // given
            string inputNhsNumber = GetRandomString();
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            Guid inputParentId = Guid.NewGuid();
            Guid foundationSpanId = Guid.NewGuid();
            Guid fanOutSpanId = Guid.NewGuid();
            Guid providerSpanId = Guid.NewGuid();
            Guid providerCallMetricId = Guid.NewGuid();
            Guid fhirRecordId = Guid.NewGuid();
            Guid persistMetricId = Guid.NewGuid();
            DateTimeOffset startedAt = GetRandomDateTimeOffset();
            Bundle randomBundle = CreateRandomBundle();
            string rawOutputJson = this.fhirJsonSerializer.SerializeToString(randomBundle);
            var recordedMetrics = new List<Metric>();

            List<Provider> activeProviders = new List<Provider>
            {
                new Provider { FriendlyName = "DDS Provider", FullyQualifiedName = "DDS", IsPrimary = true }
            };

            // Sequenced in the order the service draws them: the foundation span, the fan out,
            // then within the single provider task its span, the call metric, the record id, and
            // the persist metric.
            this.identifierBrokerMock.SetupSequence(broker => broker.GetIdentifierAsync())
                .ReturnsAsync(foundationSpanId)
                .ReturnsAsync(fanOutSpanId)
                .ReturnsAsync(providerSpanId)
                .ReturnsAsync(providerCallMetricId)
                .ReturnsAsync(fhirRecordId)
                .ReturnsAsync(persistMetricId);

            this.dateTimeBrokerMock.Setup(broker => broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(startedAt);

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<FhirRecord>()))
                    .ReturnsAsync((FhirRecord record) => record);

            this.ddsFhirProviderMock.Setup(provider => provider.Patients.GetStructuredRecordSerialisedAsync(
                inputNhsNumber,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
                    .ReturnsAsync(rawOutputJson);

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.LogMetricAsync(It.IsAny<Metric>(), It.IsAny<CancellationToken>()))
                    .Callback<Metric, CancellationToken>((metric, _) => recordedMetrics.Add(metric));

            // when
            await this.patientService.GetStructuredRecordSerialisedAsync(
                activeProviders,
                correlationId,
                inputNhsNumber,
                parentId: inputParentId,
                cancellationToken: cancellationToken);

            // then
            Metric foundationSpan = recordedMetrics.Should().ContainSingle(metric =>
                metric.Type == MetricType.Foundation).Subject;

            Metric fanOutSpan = recordedMetrics.Should().ContainSingle(metric =>
                metric.Type == MetricType.ProviderFanOut).Subject;

            Metric providerSpan = recordedMetrics.Should().ContainSingle(metric =>
                metric.Type == MetricType.Provider).Subject;

            Metric providerCallSpan = recordedMetrics.Should().ContainSingle(metric =>
                metric.Type == MetricType.ProviderCall).Subject;

            Metric persistSpan = recordedMetrics.Should().ContainSingle(metric =>
                metric.Type == MetricType.Persist).Subject;

            foundationSpan.Id.Should().Be(foundationSpanId);
            foundationSpan.ParentId.Should().Be(inputParentId);

            fanOutSpan.Id.Should().Be(fanOutSpanId);
            fanOutSpan.ParentId.Should().Be(foundationSpanId);

            providerSpan.Id.Should().Be(providerSpanId);
            providerSpan.ParentId.Should().Be(fanOutSpanId);

            providerCallSpan.Id.Should().Be(providerCallMetricId);
            providerCallSpan.ParentId.Should().Be(providerSpanId);

            // The persist is deferred, but it still belongs to the provider whose payload it
            // writes - the tree keeps it under that provider even though its duration is not
            // part of the provider's.
            persistSpan.Id.Should().Be(persistMetricId);
            persistSpan.ParentId.Should().Be(providerSpanId);

            recordedMetrics.Should().OnlyContain(metric => metric.CorrelationId == correlationId);
            recordedMetrics.Should().OnlyContain(metric => metric.Status == MetricStatus.Succeeded);

            // Completed is Started plus the duration by construction, never a second clock read.
            recordedMetrics.Should().OnlyContain(metric =>
                metric.Completed == metric.Started.AddMilliseconds(metric.DurationMs));
        }

        [Fact]
        public async Task ShouldRecordTheFoundationSpanWhenTheRequestFailsAsync()
        {
            // given
            string inputNhsNumber = GetRandomString();
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            Guid inputParentId = Guid.NewGuid();
            Guid foundationSpanId = Guid.NewGuid();
            var recordedMetrics = new List<Metric>();
            var brokerException = new Exception(GetRandomString());

            List<Provider> activeProviders = new List<Provider>
            {
                new Provider { FriendlyName = "DDS Provider", FullyQualifiedName = "DDS", IsPrimary = true }
            };

            this.identifierBrokerMock.SetupSequence(broker => broker.GetIdentifierAsync())
                .ReturnsAsync(foundationSpanId)
                .ReturnsAsync(Guid.NewGuid());

            // Thrown from inside the foundation span's try, after it exists - the span must
            // still be written on the way out.
            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.LogInformationAsync(
                    It.IsAny<string>(),
                    "Parallel Provider Execution Started",
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(brokerException);

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.LogMetricAsync(It.IsAny<Metric>(), It.IsAny<CancellationToken>()))
                    .Callback<Metric, CancellationToken>((metric, _) => recordedMetrics.Add(metric));

            // when
            Func<Task> getStructuredRecord = async () =>
                await this.patientService.GetStructuredRecordSerialisedAsync(
                    activeProviders,
                    correlationId,
                    inputNhsNumber,
                    parentId: inputParentId,
                    cancellationToken: cancellationToken);

            await getStructuredRecord.Should().ThrowAsync<Xeption>();

            // then
            Metric foundationSpan = recordedMetrics.Should().ContainSingle(metric =>
                metric.Type == MetricType.Foundation).Subject;

            foundationSpan.Id.Should().Be(foundationSpanId);
            foundationSpan.ParentId.Should().Be(inputParentId);
            foundationSpan.Status.Should().Be(MetricStatus.Failed);
            foundationSpan.ErrorCode.Should().Be(nameof(Exception));
        }
    }
}
