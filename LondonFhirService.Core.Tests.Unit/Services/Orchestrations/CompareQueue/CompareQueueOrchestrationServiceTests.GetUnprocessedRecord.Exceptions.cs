// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
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
        public async Task ShouldThrowDependencyValidationExceptionOnGetUnprocessedRecordAndLogItAsync(
            Xeption dependencyValidationException)
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();

            var expectedCompareQueueOrchestrationDependencyValidationException =
                new CompareQueueOrchestrationDependencyValidationException(
                    message: "Compare queue orchestration dependency validation error occurred, " +
                        "fix errors and try again.",
                    innerException: dependencyValidationException.InnerException as Xeption);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.fhirRecordServiceMock.Setup(service =>
                service.RetrieveAllFhirRecordsAsync())
                    .ThrowsAsync(dependencyValidationException);

            // when
            ValueTask<CompareQueueItem> getUnprocessedRecordTask =
                this.compareQueueOrchestrationService.GetUnprocessedRecordAsync();

            CompareQueueOrchestrationDependencyValidationException
                actualCompareQueueOrchestrationDependencyValidationException =
                    await Assert.ThrowsAsync<CompareQueueOrchestrationDependencyValidationException>(
                        testCode: getUnprocessedRecordTask.AsTask);

            // then
            actualCompareQueueOrchestrationDependencyValidationException
                .Should().BeEquivalentTo(expectedCompareQueueOrchestrationDependencyValidationException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.fhirRecordServiceMock.Verify(service =>
                service.RetrieveAllFhirRecordsAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCompareQueueOrchestrationDependencyValidationException))),
                        Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(FhirRecordDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnGetUnprocessedRecordAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();

            var expectedCompareQueueOrchestrationDependencyException =
                new CompareQueueOrchestrationDependencyException(
                    message: "Compare queue orchestration dependency error occurred, please contact support.",
                    innerException: dependencyException.InnerException as Xeption);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.fhirRecordServiceMock.Setup(service =>
                service.RetrieveAllFhirRecordsAsync())
                    .ThrowsAsync(dependencyException);

            // when
            ValueTask<CompareQueueItem> getUnprocessedRecordTask =
                this.compareQueueOrchestrationService.GetUnprocessedRecordAsync();

            CompareQueueOrchestrationDependencyException
                actualCompareQueueOrchestrationDependencyException =
                    await Assert.ThrowsAsync<CompareQueueOrchestrationDependencyException>(
                        testCode: getUnprocessedRecordTask.AsTask);

            // then
            actualCompareQueueOrchestrationDependencyException
                .Should().BeEquivalentTo(expectedCompareQueueOrchestrationDependencyException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.fhirRecordServiceMock.Verify(service =>
                service.RetrieveAllFhirRecordsAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCompareQueueOrchestrationDependencyException))),
                        Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnGetUnprocessedRecordAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
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
                    .ReturnsAsync(randomDateTimeOffset);

            this.fhirRecordServiceMock.Setup(service =>
                service.RetrieveAllFhirRecordsAsync())
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<CompareQueueItem> getUnprocessedRecordTask =
                this.compareQueueOrchestrationService.GetUnprocessedRecordAsync();

            CompareQueueOrchestrationServiceException
                actualCompareQueueOrchestrationServiceException =
                    await Assert.ThrowsAsync<CompareQueueOrchestrationServiceException>(
                        testCode: getUnprocessedRecordTask.AsTask);

            // then
            actualCompareQueueOrchestrationServiceException
                .Should().BeEquivalentTo(expectedCompareQueueOrchestrationServiceException);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.fhirRecordServiceMock.Verify(service =>
                service.RetrieveAllFhirRecordsAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCompareQueueOrchestrationServiceException))),
                        Times.Once);

            this.fhirRecordServiceMock.VerifyNoOtherCalls();
            this.fhirRecordDifferenceServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
