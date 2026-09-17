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
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Coordinations.Patients.STU3
{
    public partial class Stu3PatientCoordinationServiceTests
    {
        [Fact]
        public async Task ShouldCallGetStructuredRecordAsync()
        {
            // given
            string inputNhsNumber = GetRandomString();
            string inputDateOfBirth = DateTime.Now.ToString("yyyy-MM-dd");
            bool? inputDemographicsOnly = false;
            bool? inputActivePatientsOnly = true;
            CancellationToken cancellationToken = CancellationToken.None;
            Bundle randomBundle = CreateRandomBundle();
            string expectedBundle = SerializeBundle(randomBundle.DeepClone());
            Guid correlationId = Guid.NewGuid();
            Guid requestSpanId = Guid.NewGuid();
            Guid consolidationSpanId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";
            var recordedMetrics = new List<Metric>();
            List<(string Provider, string Json)> randomBundles = CreateRandomBundles();
            Provider randomPrimaryProvider = CreateRandomProvider();

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{inputDateOfBirth}\", " +
                $"demographicsOnly = \"{inputDemographicsOnly}\", " +
                $"includeInactivePatients = \"{inputActivePatientsOnly}\" }}";

            // Stubbed, not left to default. The correlation id is no longer drawn here, but the two
            // span ids still are - and with no setup Moq hands back Guid.Empty for both, so the
            // consolidation span came out self-parented and nothing in the test noticed.
            this.identifierBrokerMock.SetupSequence(broker =>
                broker.GetIdentifierAsync())
                    .ReturnsAsync(requestSpanId)
                    .ReturnsAsync(consolidationSpanId);

            // It.Is rather than It.IsAny: this is a Logic file, where test-106 bans the wildcard,
            // and the setup genuinely has to match every span the service emits - that is the
            // point of recording them. A predicate keeps the catch-all without the wildcard.
            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.LogMetricAsync(
                    It.Is<Metric>(metric => metric != null),
                    It.Is<CancellationToken>(cancellationToken => true)))
                        .Callback<Metric, CancellationToken>(
                            (metric, cancellationToken) => recordedMetrics.Add(metric));

            this.patientOrchestrationServiceMock.Setup(service =>
                service.GetStructuredRecordSerialisedAsync(
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly,
                    It.IsAny<Guid?>(),
                    cancellationToken))
                        .ReturnsAsync(new StructuredRecordsResponse
                        {
                            PrimaryProvider = randomPrimaryProvider,
                            Bundles = randomBundles
                        });

            this.fhirReconciliationServiceMock.Setup(service =>
                service.ReconcileSerialisedAsync(
                    randomBundles,
                    inputNhsNumber,
                    randomPrimaryProvider,
                    correlationId))
                        .ReturnsAsync(expectedBundle);

            // when
            string actualJson = await this.patientCoordinationService.GetStructuredRecordSerialisedAsync(
                correlationId,
                inputNhsNumber,
                inputDateOfBirth,
                inputDemographicsOnly,
                inputActivePatientsOnly,
                cancellationToken);

            // then
            actualJson.Should().BeEquivalentTo(expectedBundle);

            // Real span ids, and a tree rather than a pile: the root carries no parent and the
            // consolidation span hangs off it. Times.AtLeastOnce alone passed even when every id
            // was Guid.Empty.
            recordedMetrics.Should().HaveCount(2);

            Metric requestSpan = recordedMetrics.Should().ContainSingle(metric =>
                metric.Type == MetricType.Request).Subject;

            Metric consolidationSpan = recordedMetrics.Should().ContainSingle(metric =>
                metric.Type == MetricType.Consolidation).Subject;

            requestSpan.Id.Should().Be(requestSpanId);
            requestSpan.ParentId.Should().BeNull();
            consolidationSpan.Id.Should().Be(consolidationSpanId);
            consolidationSpan.ParentId.Should().Be(requestSpanId);

            recordedMetrics.Should().OnlyContain(metric => metric.CorrelationId == correlationId);

            this.identifierBrokerMock.Verify(broker =>
                broker.GetIdentifierAsync(),
                    Times.Exactly(2));

            this.patientOrchestrationServiceMock.Verify(service =>
                service.GetStructuredRecordSerialisedAsync(
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly,
                    It.IsAny<Guid?>(),
                    cancellationToken),
                        Times.Once);

            this.fhirReconciliationServiceMock.Verify(service =>
                service.ReconcileSerialisedAsync(
                    randomBundles,
                    inputNhsNumber,
                    randomPrimaryProvider,
                    correlationId),
                        Times.Once);

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Coordination Service Request Submitted",
                    message,
                    null,
                    correlationId.ToString("N")),
                        Times.Once);

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Requesting Patient Info",
                    message,
                    null,
                    correlationId.ToString("N")),
                        Times.Once);

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Reconcile bundles",
                    message,
                    null,
                    correlationId.ToString("N")),
                        Times.Once);

            this.auditAndMetricBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    It.Is<string>(title => title.StartsWith("Coordination Service Request Completed")),
                    message,
                    null,
                    correlationId.ToString("N")),
                        Times.Once);

            AcceptMetricSpans();
            this.patientOrchestrationServiceMock.VerifyNoOtherCalls();
            this.fhirReconciliationServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.auditAndMetricBrokerMock.VerifyNoOtherCalls();
        }
    }
}
