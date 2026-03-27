// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Orchestrations.Accesses;
using LondonFhirService.Core.Services.Coordinations.Patients.STU3;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Coordinations.Patients.STU3
{
    public partial class Stu3PatientCoordinationServiceTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldCallGetStructuredRecordAsync(bool checkAccessPermissions)
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
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{inputDateOfBirth}\", " +
                $"demographicsOnly = \"{inputDemographicsOnly}\", " +
                $"includeInactivePatients = \"{inputActivePatientsOnly}\" }}";

            this.identifierBrokerMock.Setup(broker =>
                broker.GetIdentifierAsync())
                    .ReturnsAsync(correlationId);

            this.patientOrchestrationServiceMock.Setup(service =>
                service.GetStructuredRecordSerialisedAsync(
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly,
                    cancellationToken))
                        .ReturnsAsync(expectedBundle);

            var accessConfig = new AccessConfigurations
            {
                UseHashedNhsNumber = true,
                HashPepper = GetRandomStringWithLength(100),
                CheckAccessPermissions = checkAccessPermissions
            };

            var patientCoordinationServiceMock = new Mock<Stu3PatientCoordinationService>(
                accessOrchestrationServiceMock.Object,
                patientOrchestrationServiceMock.Object,
                loggingBrokerMock.Object,
                auditBrokerMock.Object,
                identifierBrokerMock.Object,
                accessConfig)
            {
                CallBase = true
            };

            // when
            string actualJson = await patientCoordinationServiceMock.Object.GetStructuredRecordSerialisedAsync(
                inputNhsNumber,
                inputDateOfBirth,
                inputDemographicsOnly,
                inputActivePatientsOnly,
                cancellationToken);

            // then
            actualJson.Should().BeEquivalentTo(expectedBundle);

            this.identifierBrokerMock.Verify(broker =>
                broker.GetIdentifierAsync(),
                    Times.Once);

            this.patientOrchestrationServiceMock.Verify(service =>
                service.GetStructuredRecordSerialisedAsync(
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly,
                    cancellationToken),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Coordination Service Request Submitted",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            if (checkAccessPermissions)
            {
                this.accessOrchestrationServiceMock.Verify(service =>
                    service.ValidateAccess(inputNhsNumber, correlationId),
                        Times.Once);

                this.auditBrokerMock.Verify(broker =>
                    broker.LogInformationAsync(
                        auditType,
                        "Check Access Permissions",
                        message,
                        null,
                        correlationId.ToString()),
                            Times.Once);
            }

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Requesting Patient Info",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    It.Is<string>(s => s.StartsWith("Coordination Service Request Completed")),
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.accessOrchestrationServiceMock.VerifyNoOtherCalls();
            this.patientOrchestrationServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
        }
    }
}
