// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Core.Models.Foundations.Metrics;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Models.Orchestrations.Patients;
using Moq;
using Xeptions;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Coordinations.Patients.STU3
{
    /// <summary>
    /// The span tree, which the behavioural tests deliberately do not assert on. What matters
    /// here is the shape: the root span carries no parent, everything below it points at the
    /// span that encloses it, and the whole tree shares one correlation id. Without that, the
    /// rows are a pile of durations rather than a trace.
    /// </summary>
    public partial class Stu3PatientCoordinationServiceTests
    {
        [Fact]
        public async Task ShouldRecordTheRequestSpanAsTheRootOfTheTreeAsync()
        {
            // given
            string inputNhsNumber = GetRandomString();
            CancellationToken cancellationToken = CancellationToken.None;
            Bundle randomBundle = CreateRandomBundle();
            string expectedBundle = SerializeBundle(randomBundle.DeepClone());
            Guid correlationId = Guid.NewGuid();
            Guid requestSpanId = Guid.NewGuid();
            Guid consolidationSpanId = Guid.NewGuid();
            DateTimeOffset startedAt = GetRandomDateTimeOffset();
            List<(string Provider, string Json)> randomBundles = CreateRandomBundles();
            Provider randomPrimaryProvider = CreateRandomProvider();
            var recordedMetrics = new List<Metric>();

            // Sequenced so the ids can be told apart: the correlation id is drawn first, then
            // the root span, then the consolidation span.
            this.identifierBrokerMock.SetupSequence(broker => broker.GetIdentifierAsync())
                .ReturnsAsync(correlationId)
                .ReturnsAsync(requestSpanId)
                .ReturnsAsync(consolidationSpanId);

            this.dateTimeBrokerMock.Setup(broker => broker.GetCurrentDateTimeOffsetAsync())
                .ReturnsAsync(startedAt);

            this.patientOrchestrationServiceMock.Setup(service =>
                service.GetStructuredRecordSerialisedAsync(
                    correlationId,
                    inputNhsNumber,
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Guid?>(),
                    cancellationToken))
                        .ReturnsAsync(new StructuredRecordsResponse
                        {
                            PrimaryProvider = randomPrimaryProvider,
                            Bundles = randomBundles
                        });

            this.fhirReconciliationServiceMock.Setup(service =>
                service.ReconcileSerialisedAsync(
                    It.IsAny<List<(string Provider, string Json)>>(),
                    inputNhsNumber,
                    randomPrimaryProvider,
                    correlationId))
                        .ReturnsAsync(expectedBundle);

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.LogMetricAsync(It.IsAny<Metric>(), It.IsAny<CancellationToken>()))
                    .Callback<Metric, CancellationToken>((metric, _) => recordedMetrics.Add(metric));

            // when
            await this.patientCoordinationService.GetStructuredRecordSerialisedAsync(
                inputNhsNumber,
                cancellationToken: cancellationToken);

            // then
            Metric requestSpan = recordedMetrics.Should().ContainSingle(metric =>
                metric.Type == MetricType.Request).Subject;

            Metric consolidationSpan = recordedMetrics.Should().ContainSingle(metric =>
                metric.Type == MetricType.Consolidation).Subject;

            // The root is the only span with no parent. Anything else without one is an orphan
            // that would never appear under its request in a trace.
            requestSpan.Id.Should().Be(requestSpanId);
            requestSpan.ParentId.Should().BeNull();

            consolidationSpan.Id.Should().Be(consolidationSpanId);
            consolidationSpan.ParentId.Should().Be(requestSpanId);

            recordedMetrics.Should().OnlyContain(metric => metric.CorrelationId == correlationId);
            recordedMetrics.Should().OnlyContain(metric => metric.Status == MetricStatus.Succeeded);

            // Completed is Started plus the duration by construction, never a second clock read.
            recordedMetrics.Should().OnlyContain(metric =>
                metric.Completed == metric.Started.AddMilliseconds(metric.DurationMs));

            requestSpan.Method.Should().Be("STU3-Patient-GetStructuredRecordSerialised");
            requestSpan.PayloadBytes.Should().Be(expectedBundle.Length);
        }

        [Fact]
        public async Task ShouldRecordTheRequestSpanWhenTheRequestFailsAsync()
        {
            // given
            string inputNhsNumber = GetRandomString();
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            Guid requestSpanId = Guid.NewGuid();
            var recordedMetrics = new List<Metric>();
            var serviceException = new Exception(GetRandomString());

            this.identifierBrokerMock.SetupSequence(broker => broker.GetIdentifierAsync())
                .ReturnsAsync(correlationId)
                .ReturnsAsync(requestSpanId);

            this.patientOrchestrationServiceMock.Setup(service =>
                service.GetStructuredRecordSerialisedAsync(
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
                await this.patientCoordinationService.GetStructuredRecordSerialisedAsync(
                    inputNhsNumber,
                    cancellationToken: cancellationToken);

            await getStructuredRecord.Should().ThrowAsync<Xeption>();

            // then
            // The root has to be written on the way out too. Children are recorded as they
            // complete, so without this every span already written points at a row that never
            // got inserted - and a failed request is the one worth walking the tree for.
            Metric requestSpan = recordedMetrics.Should().ContainSingle(metric =>
                metric.Type == MetricType.Request).Subject;

            requestSpan.Id.Should().Be(requestSpanId);
            requestSpan.ParentId.Should().BeNull();
            requestSpan.Status.Should().Be(MetricStatus.Failed);
            requestSpan.ErrorCode.Should().Be(nameof(Exception));
        }

        [Fact]
        public async Task ShouldPassTheRequestSpanDownAsTheParentAsync()
        {
            // given
            string inputNhsNumber = GetRandomString();
            CancellationToken cancellationToken = CancellationToken.None;
            Bundle randomBundle = CreateRandomBundle();
            string expectedBundle = SerializeBundle(randomBundle.DeepClone());
            Guid correlationId = Guid.NewGuid();
            Guid requestSpanId = Guid.NewGuid();
            List<(string Provider, string Json)> randomBundles = CreateRandomBundles();
            Provider randomPrimaryProvider = CreateRandomProvider();

            this.identifierBrokerMock.SetupSequence(broker => broker.GetIdentifierAsync())
                .ReturnsAsync(correlationId)
                .ReturnsAsync(requestSpanId)
                .ReturnsAsync(Guid.NewGuid());

            this.patientOrchestrationServiceMock.Setup(service =>
                service.GetStructuredRecordSerialisedAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new StructuredRecordsResponse
                        {
                            PrimaryProvider = randomPrimaryProvider,
                            Bundles = randomBundles
                        });

            this.fhirReconciliationServiceMock.Setup(service =>
                service.ReconcileSerialisedAsync(
                    It.IsAny<List<(string Provider, string Json)>>(),
                    It.IsAny<string>(),
                    It.IsAny<Provider>(),
                    It.IsAny<Guid>()))
                        .ReturnsAsync(expectedBundle);

            // when
            await this.patientCoordinationService.GetStructuredRecordSerialisedAsync(
                inputNhsNumber,
                cancellationToken: cancellationToken);

            // then
            // Every layer below has to be told which span encloses it, or the tree flattens and
            // the nesting the whole design depends on is lost.
            this.patientOrchestrationServiceMock.Verify(service =>
                service.GetStructuredRecordSerialisedAsync(
                    correlationId,
                    inputNhsNumber,
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    requestSpanId,
                    cancellationToken),
                        Times.Once);
        }
    }
}
