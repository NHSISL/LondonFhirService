// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
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
            DateTime? inputDateOfBirth = DateTime.Now;
            bool? inputDemographicsOnly = false;
            bool? inputActivePatientsOnly = true;
            CancellationToken cancellationToken = CancellationToken.None;
            Bundle randomBundle = CreateRandomBundle();
            Bundle expectedBundle = randomBundle.DeepClone();
            Guid correlationId = Guid.NewGuid();

            this.accessOrchestrationServiceMock.Setup(service =>
                service.ValidateAccess(inputNhsNumber, correlationId));

            this.patientOrchestrationServiceMock.Setup(service =>
                service.GetStructuredRecordAsync(
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly,
                    cancellationToken))
                        .ReturnsAsync(expectedBundle);

            // when
            Bundle actualBundle = await this.patientCoordinationService.GetStructuredRecordAsync(
                inputNhsNumber,
                inputDateOfBirth,
                inputDemographicsOnly,
                inputActivePatientsOnly,
                cancellationToken);

            // then
            actualBundle.Should().BeEquivalentTo(expectedBundle);

            this.accessOrchestrationServiceMock.Verify(service =>
                service.ValidateAccess(inputNhsNumber, correlationId),
                    Times.Once);

            this.patientOrchestrationServiceMock.Verify(service =>
                service.GetStructuredRecordAsync(
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly,
                    cancellationToken),
                        Times.Once);

            this.accessOrchestrationServiceMock.VerifyNoOtherCalls();
            this.patientOrchestrationServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
        }
    }
}
