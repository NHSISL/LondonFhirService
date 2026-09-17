// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue.Exceptions;
using Moq;
using Xeptions;
using Xunit;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.CompareQueue
{
    public partial class CompareQueueOrchestrationServiceTests
    {
        [Theory]
        [MemberData(nameof(FhirRecordDependencyValidationExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionOnFinalizeAndLogItAsync(
            Xeption dependencyValidationException)
        {
            // given
            CompareQueueItem inputCompareQueueItem = CreateClaimedCompareQueueItem();

            var expectedCompareQueueOrchestrationDependencyValidationException =
                new CompareQueueOrchestrationDependencyValidationException(
                    message: "Compare queue orchestration dependency validation error occurred, " +
                        "fix errors and try again.",
                    innerException: dependencyValidationException.InnerException as Xeption);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(GetRandomDateTimeOffset());

            this.fhirRecordServiceMock.Setup(service =>
                service.TryClaimFhirRecordAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<bool>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(dependencyValidationException);

            // when
            ValueTask<bool> finalizeTask = this.compareQueueOrchestrationService
                .TryFinalizeClaimedFhirRecordAsync(inputCompareQueueItem, StatusType.Failed);

            CompareQueueOrchestrationDependencyValidationException
                actualCompareQueueOrchestrationDependencyValidationException =
                    await Assert.ThrowsAsync<CompareQueueOrchestrationDependencyValidationException>(
                        testCode: finalizeTask.AsTask);

            // then
            actualCompareQueueOrchestrationDependencyValidationException
                .Should().BeEquivalentTo(expectedCompareQueueOrchestrationDependencyValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCompareQueueOrchestrationDependencyValidationException))),
                        Times.Once);

            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(FhirRecordDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnFinalizeAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            CompareQueueItem inputCompareQueueItem = CreateClaimedCompareQueueItem();

            var expectedCompareQueueOrchestrationDependencyException =
                new CompareQueueOrchestrationDependencyException(
                    message: "Compare queue orchestration dependency error occurred, please contact support.",
                    innerException: dependencyException.InnerException as Xeption);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(GetRandomDateTimeOffset());

            this.fhirRecordServiceMock.Setup(service =>
                service.TryClaimFhirRecordAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<bool>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(dependencyException);

            // when
            ValueTask<bool> finalizeTask = this.compareQueueOrchestrationService
                .TryFinalizeClaimedFhirRecordAsync(inputCompareQueueItem, StatusType.Completed);

            CompareQueueOrchestrationDependencyException
                actualCompareQueueOrchestrationDependencyException =
                    await Assert.ThrowsAsync<CompareQueueOrchestrationDependencyException>(
                        testCode: finalizeTask.AsTask);

            // then
            actualCompareQueueOrchestrationDependencyException
                .Should().BeEquivalentTo(expectedCompareQueueOrchestrationDependencyException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCompareQueueOrchestrationDependencyException))),
                        Times.Once);

            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnFinalizeAndLogItAsync()
        {
            // given
            CompareQueueItem inputCompareQueueItem = CreateClaimedCompareQueueItem();
            var serviceException = new Exception();

            var failedCompareQueueOrchestrationServiceException =
                new FailedCompareQueueOrchestrationServiceException(
                    message: "Failed compare queue orchestration service error occurred, please contact support.",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedCompareQueueOrchestrationServiceException =
                new CompareQueueOrchestrationServiceException(
                    message: "Compare queue orchestration service error occurred, please contact support.",
                    innerException: failedCompareQueueOrchestrationServiceException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(GetRandomDateTimeOffset());

            this.fhirRecordServiceMock.Setup(service =>
                service.TryClaimFhirRecordAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<StatusType>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<bool>(),
                    It.IsAny<DateTimeOffset?>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(serviceException);

            // when
            ValueTask<bool> finalizeTask = this.compareQueueOrchestrationService
                .TryFinalizeClaimedFhirRecordAsync(inputCompareQueueItem, StatusType.Failed);

            CompareQueueOrchestrationServiceException
                actualCompareQueueOrchestrationServiceException =
                    await Assert.ThrowsAsync<CompareQueueOrchestrationServiceException>(
                        testCode: finalizeTask.AsTask);

            // then
            actualCompareQueueOrchestrationServiceException
                .Should().BeEquivalentTo(expectedCompareQueueOrchestrationServiceException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCompareQueueOrchestrationServiceException))),
                        Times.Once);

            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        private CompareQueueItem CreateClaimedCompareQueueItem() =>
            new CompareQueueItem
            {
                SecondaryFhirRecord = CreateRandomFhirRecord(),
                ClaimedAt = GetRandomDateTimeOffset()
            };
    }
}
