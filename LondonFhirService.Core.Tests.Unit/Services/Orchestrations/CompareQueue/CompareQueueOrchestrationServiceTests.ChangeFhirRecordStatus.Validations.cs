// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue.Exceptions;
using Moq;
using Xunit;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.CompareQueue
{
    public partial class CompareQueueOrchestrationServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnChangeFhirRecordStatusIfFhirRecordIdIsInvalidAndLogItAsync()
        {
            // given
            Guid invalidFhirRecordId = Guid.Empty;
            StatusType randomStatus = StatusType.Processing;
            StatusType inputStatus = randomStatus;

            var invalidCompareQueueOrchestrationException =
                new InvalidCompareQueueOrchestrationException(
                    message: "Invalid argument(s), please correct the errors and try again.");

            invalidCompareQueueOrchestrationException.AddData(
                key: "fhirRecordId",
                values: "Id is invalid");

            var expectedCompareQueueOrchestrationValidationException =
                new CompareQueueOrchestrationValidationException(
                    message: "Compare queue orchestration validation error occurred, fix errors and try again.",
                    innerException: invalidCompareQueueOrchestrationException);

            // when
            ValueTask changeFhirRecordStatusTask =
                this.compareQueueOrchestrationService
                    .ChangeFhirRecordStatusAsync(invalidFhirRecordId, inputStatus);

            CompareQueueOrchestrationValidationException
                actualCompareQueueOrchestrationValidationException =
                    await Assert.ThrowsAsync<CompareQueueOrchestrationValidationException>(
                        testCode: changeFhirRecordStatusTask.AsTask);

            // then
            actualCompareQueueOrchestrationValidationException
                .Should().BeEquivalentTo(expectedCompareQueueOrchestrationValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCompareQueueOrchestrationValidationException))),
                        Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
