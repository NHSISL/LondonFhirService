// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Medications.Exceptions;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Medications;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Medications
{
    public partial class MedicationMatcherServiceTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnGetMatchKeyIfServiceErrorOccursAndLogItAsync()
        {
            // given
            JsonElement resource = new();
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
            var serviceException = new Exception();

            var failedMedicationMatcherServiceException =
                new FailedMedicationMatcherServiceException(
                    message: "Failed medication matcher service occurred, please contact support",
                    innerException: serviceException);

            var expectedMedicationMatcherServiceException =
                new MedicationMatcherServiceException(
                    message: "Medication matcher service error occurred, contact support.",
                    innerException: failedMedicationMatcherServiceException);

            var medicationMatcherServiceMock = new Mock<MedicationMatcherService>(loggingBrokerMock.Object)
                { CallBase = true };

            medicationMatcherServiceMock.Setup(service =>
                service.ValidateOnGetMatchKeyArguments(
                    resource,
                    resourceIndex))
                        .Throws(serviceException);

            // when
            ValueTask<string> matchTask =
                medicationMatcherServiceMock.Object.GetMatchKeyAsync(
                    resource,
                    resourceIndex);

            MedicationMatcherServiceException actualMedicationMatcherServiceException =
                await Assert.ThrowsAsync<MedicationMatcherServiceException>(
                    matchTask.AsTask);

            // then
            actualMedicationMatcherServiceException.Should()
                .BeEquivalentTo(expectedMedicationMatcherServiceException);

            medicationMatcherServiceMock.Verify(service =>
                service.ValidateOnGetMatchKeyArguments(
                    resource,
                    resourceIndex),
                        Times.Once);

            medicationMatcherServiceMock.Verify(service =>
                service.GetMatchKeyAsync(
                    resource,
                    resourceIndex),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMedicationMatcherServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            medicationMatcherServiceMock.VerifyNoOtherCalls();
        }
    }
}