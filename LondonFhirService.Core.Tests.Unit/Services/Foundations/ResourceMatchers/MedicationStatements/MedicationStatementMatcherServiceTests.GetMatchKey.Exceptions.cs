// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.MedicationStatements.Exceptions;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.MedicationStatements;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.MedicationStatements
{
    public partial class MedicationStatementMatcherServiceTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnGetMatchKeyIfServiceErrorOccursAndLogItAsync()
        {
            // given
            JsonElement resource = new();
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
            var serviceException = new Exception();

            var failedMedicationStatementMatcherServiceException =
                new FailedMedicationStatementMatcherServiceException(
                    message: "Failed medication statement matcher service occurred, please contact support",
                    innerException: serviceException);

            var expectedMedicationStatementMatcherServiceException =
                new MedicationStatementMatcherServiceException(
                    message: "Medication statement matcher service error occurred, contact support.",
                    innerException: failedMedicationStatementMatcherServiceException);

            var medicationStatementMatcherServiceMock = new Mock<MedicationStatementMatcherService>(loggingBrokerMock.Object)
                { CallBase = true };

            medicationStatementMatcherServiceMock.Setup(service =>
                service.ValidateOnGetMatchKeyArguments(
                    resource,
                    resourceIndex))
                        .Throws(serviceException);

            // when
            ValueTask<string> matchTask =
                medicationStatementMatcherServiceMock.Object.GetMatchKeyAsync(
                    resource,
                    resourceIndex);

            MedicationStatementMatcherServiceException actualMedicationStatementMatcherServiceException =
                await Assert.ThrowsAsync<MedicationStatementMatcherServiceException>(
                    matchTask.AsTask);

            // then
            actualMedicationStatementMatcherServiceException.Should()
                .BeEquivalentTo(expectedMedicationStatementMatcherServiceException);

            medicationStatementMatcherServiceMock.Verify(service =>
                service.ValidateOnGetMatchKeyArguments(
                    resource,
                    resourceIndex),
                        Times.Once);

            medicationStatementMatcherServiceMock.Verify(service =>
                service.GetMatchKeyAsync(
                    resource,
                    resourceIndex),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedMedicationStatementMatcherServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            medicationStatementMatcherServiceMock.VerifyNoOtherCalls();
        }
    }
}