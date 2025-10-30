// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Foundations.Patients.Exceptions;
using LondonFhirService.Core.Services.Foundations.Patients;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Patients
{
    public partial class PatientServiceTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnEverythingIfServiceErrorOccursAndLogItAsync()
        {
            // given
            List<string> randomProviderNames = new List<string>
            {
                "DDS"
            };

            List<string> inputProviderNames = randomProviderNames.DeepClone();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;

            var serviceException = new Exception();

            var failedPatientServiceException =
                new FailedPatientServiceException(
                    message: "Failed patient service error occurred, please contact support.",
                    innerException: serviceException);

            var expectedPatientServiceException =
                new PatientServiceException(
                    message: "Patient service error occurred, contact support.",
                    innerException: failedPatientServiceException);

            var patientServiceMock = new Mock<PatientService>(
                this.fhirBroker,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.ExecuteWithTimeoutAsync(
                    ddsFhirProviderMock.Object.Patients,
                    default,
                    inputNhsNumber,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ThrowsAsync(serviceException);

            PatientService patientService = patientServiceMock.Object;

            // when
            ValueTask<List<Bundle>> everythingTask =
                patientService.Everything(
                    providerNames: inputProviderNames,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            PatientServiceException actualPatientServiceException =
                await Assert.ThrowsAsync<PatientServiceException>(
                    testCode: everythingTask.AsTask);

            // then
            actualPatientServiceException.Should()
                .BeEquivalentTo(expectedPatientServiceException);

            patientServiceMock.Verify(service =>
                service.ExecuteWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object.Patients,
                    default,
                    inputNhsNumber,
                    null,
                    null,
                    null,
                    null,
                    null),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }
    }
}
