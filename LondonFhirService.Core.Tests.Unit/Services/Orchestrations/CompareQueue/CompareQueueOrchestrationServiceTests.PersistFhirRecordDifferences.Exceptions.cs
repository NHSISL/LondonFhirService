// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences;
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
        [MemberData(nameof(FhirRecordDifferenceDependencyValidationExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionOnPersistFhirRecordDifferencesAndLogItAsync(
            Xeption dependencyValidationException)
        {
            // given
            CompareQueueItem randomCompareQueueItem = CreateRandomCompareQueueItem();
            CompareQueueItem inputCompareQueueItem = randomCompareQueueItem;

            var expectedCompareQueueOrchestrationDependencyValidationException =
                new CompareQueueOrchestrationDependencyValidationException(
                    message: "Compare queue orchestration dependency validation error occurred, " +
                        "fix errors and try again.",
                    innerException: dependencyValidationException.InnerException as Xeption);

            this.fhirRecordDifferenceServiceMock.Setup(service =>
                service.AddFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()))
                    .ThrowsAsync(dependencyValidationException);

            // when
            ValueTask persistFhirRecordDifferencesTask =
                this.compareQueueOrchestrationService
                    .PersistFhirRecordDifferencesAsync(inputCompareQueueItem);

            CompareQueueOrchestrationDependencyValidationException
                actualCompareQueueOrchestrationDependencyValidationException =
                    await Assert.ThrowsAsync<CompareQueueOrchestrationDependencyValidationException>(
                        testCode: persistFhirRecordDifferencesTask.AsTask);

            // then
            actualCompareQueueOrchestrationDependencyValidationException
                .Should().BeEquivalentTo(expectedCompareQueueOrchestrationDependencyValidationException);

            this.fhirRecordDifferenceServiceMock.Verify(service =>
                service.AddFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
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
        [MemberData(nameof(FhirRecordDifferenceDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnPersistFhirRecordDifferencesAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            CompareQueueItem randomCompareQueueItem = CreateRandomCompareQueueItem();
            CompareQueueItem inputCompareQueueItem = randomCompareQueueItem;

            var expectedCompareQueueOrchestrationDependencyException =
                new CompareQueueOrchestrationDependencyException(
                    message: "Compare queue orchestration dependency error occurred, please contact support.",
                    innerException: dependencyException.InnerException as Xeption);

            this.fhirRecordDifferenceServiceMock.Setup(service =>
                service.AddFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()))
                    .ThrowsAsync(dependencyException);

            // when
            ValueTask persistFhirRecordDifferencesTask =
                this.compareQueueOrchestrationService
                    .PersistFhirRecordDifferencesAsync(inputCompareQueueItem);

            CompareQueueOrchestrationDependencyException
                actualCompareQueueOrchestrationDependencyException =
                    await Assert.ThrowsAsync<CompareQueueOrchestrationDependencyException>(
                        testCode: persistFhirRecordDifferencesTask.AsTask);

            // then
            actualCompareQueueOrchestrationDependencyException
                .Should().BeEquivalentTo(expectedCompareQueueOrchestrationDependencyException);

            this.fhirRecordDifferenceServiceMock.Verify(service =>
                service.AddFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
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
        public async Task ShouldThrowServiceExceptionOnPersistFhirRecordDifferencesAndLogItAsync()
        {
            // given
            CompareQueueItem randomCompareQueueItem = CreateRandomCompareQueueItem();
            CompareQueueItem inputCompareQueueItem = randomCompareQueueItem;
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

            this.fhirRecordDifferenceServiceMock.Setup(service =>
                service.AddFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask persistFhirRecordDifferencesTask =
                this.compareQueueOrchestrationService
                    .PersistFhirRecordDifferencesAsync(inputCompareQueueItem);

            CompareQueueOrchestrationServiceException
                actualCompareQueueOrchestrationServiceException =
                    await Assert.ThrowsAsync<CompareQueueOrchestrationServiceException>(
                        testCode: persistFhirRecordDifferencesTask.AsTask);

            // then
            actualCompareQueueOrchestrationServiceException
                .Should().BeEquivalentTo(expectedCompareQueueOrchestrationServiceException);

            this.fhirRecordDifferenceServiceMock.Verify(service =>
                service.AddFhirRecordDifferenceAsync(It.IsAny<FhirRecordDifference>()),
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
