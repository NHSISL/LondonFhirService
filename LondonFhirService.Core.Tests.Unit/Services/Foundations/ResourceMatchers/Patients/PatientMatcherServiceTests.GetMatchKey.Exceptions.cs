// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Patients.Exceptions;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Patients;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Patients
{
    public partial class PatientMatcherServiceTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnGetMatchKeysIfServiceErrorOccursAndLogItAsync()
        {
            // given
            JsonElement resource = new();
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
            var serviceException = new Exception();

            var failedPatientMatcherServiceException =
                new FailedPatientMatcherServiceException(
                    message: "Failed patient matcher service occurred, please contact support",
                    innerException: serviceException);

            var expectedPatientMatcherServiceException =
                new PatientMatcherServiceException(
                    message: "Patient matcher service error occurred, contact support.",
                    innerException: failedPatientMatcherServiceException);

            var patientMatcherServiceMock = new Mock<PatientMatcherService>(loggingBrokerMock.Object)
                { CallBase = true };

            patientMatcherServiceMock.Setup(service =>
                service.ValidateOnGetMatchKeyArguments(
                    resource,
                    resourceIndex))
                        .Throws(serviceException);

            // when
            ValueTask<string> matchTask =
                patientMatcherServiceMock.Object.GetMatchKeyAsync(
                    resource,
                    resourceIndex);

            PatientMatcherServiceException actualPatientMatcherServiceException =
                await Assert.ThrowsAsync<PatientMatcherServiceException>(
                    matchTask.AsTask);

            // then
            actualPatientMatcherServiceException.Should()
                .BeEquivalentTo(expectedPatientMatcherServiceException);

            patientMatcherServiceMock.Verify(service =>
                service.ValidateOnGetMatchKeyArguments(
                    resource,
                    resourceIndex),
                        Times.Once);

            patientMatcherServiceMock.Verify(service =>
                service.GetMatchKeyAsync(
                    resource,
                    resourceIndex),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientMatcherServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            patientMatcherServiceMock.VerifyNoOtherCalls();
        }
    }
}    