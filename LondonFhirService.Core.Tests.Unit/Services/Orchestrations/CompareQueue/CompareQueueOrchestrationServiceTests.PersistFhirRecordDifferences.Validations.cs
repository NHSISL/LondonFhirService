// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue.Exceptions;
using Moq;
using Xunit;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.CompareQueue
{
    public partial class CompareQueueOrchestrationServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnPersistFhirRecordDifferencesIfNullAndLogItAsync()
        {
            // given
            CompareQueueItem nullCompareQueueItem = null;

            var nullCompareQueueItemException =
                new NullCompareQueueItemException(
                    message: "Compare queue item is null, fix errors and try again.");

            var expectedCompareQueueOrchestrationValidationException =
                new CompareQueueOrchestrationValidationException(
                    message: "Compare queue orchestration validation error occurred, fix errors and try again.",
                    innerException: nullCompareQueueItemException);

            // when
            ValueTask persistFhirRecordDifferencesTask =
                this.compareQueueOrchestrationService
                    .PersistFhirRecordDifferencesAsync(nullCompareQueueItem);

            CompareQueueOrchestrationValidationException
                actualCompareQueueOrchestrationValidationException =
                    await Assert.ThrowsAsync<CompareQueueOrchestrationValidationException>(
                        testCode: persistFhirRecordDifferencesTask.AsTask);

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
