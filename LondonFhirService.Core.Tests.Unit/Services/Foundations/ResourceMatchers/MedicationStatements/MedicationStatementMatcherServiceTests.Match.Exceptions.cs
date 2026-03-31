// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.MedicationStatements.Exceptions;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.MedicationStatements;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.MedicationStatements
{
    public partial class MedicationStatementMatcherServiceTests
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
                service.ValidateOnMatchArguments(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex))
                        .Throws(serviceException);

            // when
            ValueTask<ResourceMatch> matchTask =
                medicationStatementMatcherServiceMock.Object.MatchAsync(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex);

            MedicationStatementMatcherServiceException actualMedicationStatementMatcherServiceException =
                await Assert.ThrowsAsync<MedicationStatementMatcherServiceException>(
                    matchTask.AsTask);

            // then
            actualMedicationStatementMatcherServiceException.Should()
                .BeEquivalentTo(expectedMedicationStatementMatcherServiceException);

            medicationStatementMatcherServiceMock.Verify(service =>
                service.ValidateOnMatchArguments(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex),
                        Times.Once);

            medicationStatementMatcherServiceMock.Verify(service =>
                service.MatchAsync(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex),
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