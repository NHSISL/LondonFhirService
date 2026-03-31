// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Patients.Exceptions;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Patients;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Patients
{
    public partial class PatientMatcherServiceTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnMatchIfServiceErrorOccursAndLogItAsync()
        {
            // given
            List<JsonElement> invalidSource1Resources = new();
            List<JsonElement> invalidSource2Resources = new();
            Dictionary<string, JsonElement> invalidSource1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> invalidSource2ResourceIndex = CreateResourceIndex();
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
                service.ValidateOnMatchArguments(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex))
                        .Throws(serviceException);

            // when
            ValueTask<ResourceMatch> matchTask =
                patientMatcherServiceMock.Object.MatchAsync(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex);

            PatientMatcherServiceException actualPatientMatcherServiceException =
                await Assert.ThrowsAsync<PatientMatcherServiceException>(
                    matchTask.AsTask);

            // then
            actualPatientMatcherServiceException.Should()
                .BeEquivalentTo(expectedPatientMatcherServiceException);

            patientMatcherServiceMock.Verify(service =>
                service.ValidateOnMatchArguments(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex),
                        Times.Once);

            patientMatcherServiceMock.Verify(service =>
                service.MatchAsync(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex),
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