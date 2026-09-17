// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue.Exceptions;
using Moq;
using Xunit;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.CompareQueue
{
    public partial class CompareQueueOrchestrationServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnFinalizeIfSecondaryIdIsInvalidAndLogItAsync()
        {
            // given
            var inputCompareQueueItem = new CompareQueueItem
            {
                SecondaryFhirRecord = new FhirRecord { Id = Guid.Empty },
                ClaimedAt = GetRandomDateTimeOffset()
            };

            var invalidCompareQueueOrchestrationException =
                new InvalidCompareQueueOrchestrationException(
                    message: "Invalid argument(s), please correct the errors and try again.");

            invalidCompareQueueOrchestrationException.AddData(
                key: "Id",
                values: "Id is invalid");

            var expectedCompareQueueOrchestrationValidationException =
                new CompareQueueOrchestrationValidationException(
                    message: "Compare queue orchestration validation error occurred, fix errors and try again.",
                    innerException: invalidCompareQueueOrchestrationException);

            // when
            ValueTask<bool> finalizeTask = this.compareQueueOrchestrationService
                .TryFinalizeClaimedFhirRecordAsync(inputCompareQueueItem, StatusType.Failed);

            CompareQueueOrchestrationValidationException
                actualCompareQueueOrchestrationValidationException =
                    await Assert.ThrowsAsync<CompareQueueOrchestrationValidationException>(
                        testCode: finalizeTask.AsTask);

            // then
            // Validated before the clock is read and before anything reaches storage, so an empty
            // id never becomes a statement whose only remaining guard is the status.
            actualCompareQueueOrchestrationValidationException
                .Should().BeEquivalentTo(expectedCompareQueueOrchestrationValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCompareQueueOrchestrationValidationException))),
                        Times.Once);

            // The clock read is the first thing past the guard, so proving it never happened is
            // what makes "before the clock is read" an assertion rather than a comment. The settle
            // itself cannot run without the value that read returns.
            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(StatusType.Pending)]
        [InlineData(StatusType.Processing)]
        public async Task ShouldThrowValidationExceptionOnFinalizeIfStatusIsNotTerminalAndLogItAsync(
            StatusType nonTerminalStatus)
        {
            // given
            var inputCompareQueueItem = new CompareQueueItem
            {
                SecondaryFhirRecord = CreateRandomFhirRecord(),
                ClaimedAt = GetRandomDateTimeOffset()
            };

            var invalidCompareQueueOrchestrationException =
                new InvalidCompareQueueOrchestrationException(
                    message: "Invalid argument(s), please correct the errors and try again.");

            invalidCompareQueueOrchestrationException.AddData(
                key: "terminalStatus",
                values: "Status is not terminal");

            var expectedCompareQueueOrchestrationValidationException =
                new CompareQueueOrchestrationValidationException(
                    message: "Compare queue orchestration validation error occurred, fix errors and try again.",
                    innerException: invalidCompareQueueOrchestrationException);

            // when
            ValueTask<bool> finalizeTask = this.compareQueueOrchestrationService
                .TryFinalizeClaimedFhirRecordAsync(inputCompareQueueItem, nonTerminalStatus);

            CompareQueueOrchestrationValidationException
                actualCompareQueueOrchestrationValidationException =
                    await Assert.ThrowsAsync<CompareQueueOrchestrationValidationException>(
                        testCode: finalizeTask.AsTask);

            // then
            // This method hardcodes IsProcessed to true, which is only right for a status nothing
            // comes back from. Allowing Processing here would turn the fenced write into a lease
            // renewal that also marks the row processed, leaving it both in flight and reported
            // as done - so the guard is a validation rule rather than a comment.
            actualCompareQueueOrchestrationValidationException
                .Should().BeEquivalentTo(expectedCompareQueueOrchestrationValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCompareQueueOrchestrationValidationException))),
                        Times.Once);

            // A non-terminal status is refused before the clock is read, so the fenced write that
            // would have marked the row processed never reaches storage.
            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnFinalizeIfCompareQueueItemIsNullAndLogItAsync()
        {
            // given
            var nullCompareQueueItemException =
                new NullCompareQueueItemException(
                    message: "Compare queue item is null, fix errors and try again.");

            var expectedCompareQueueOrchestrationValidationException =
                new CompareQueueOrchestrationValidationException(
                    message: "Compare queue orchestration validation error occurred, fix errors and try again.",
                    innerException: nullCompareQueueItemException);

            // when
            ValueTask<bool> finalizeTask = this.compareQueueOrchestrationService
                .TryFinalizeClaimedFhirRecordAsync(null, StatusType.Failed);

            CompareQueueOrchestrationValidationException
                actualCompareQueueOrchestrationValidationException =
                    await Assert.ThrowsAsync<CompareQueueOrchestrationValidationException>(
                        testCode: finalizeTask.AsTask);

            // then
            actualCompareQueueOrchestrationValidationException
                .Should().BeEquivalentTo(expectedCompareQueueOrchestrationValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCompareQueueOrchestrationValidationException))),
                        Times.Once);

            // The null guard is structural and circuit-breaking: nothing downstream is reached,
            // so the clock is never read either.
            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Never);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
