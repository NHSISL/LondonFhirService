// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
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
        public async Task ShouldThrowDependencyValidationExceptionOnChangeFhirRecordStatusAndLogItAsync(
            Xeption dependencyValidationException)
        {
            // given
            Guid randomFhirRecordId = Guid.NewGuid();
            Guid inputFhirRecordId = randomFhirRecordId;
            StatusType inputStatus = StatusType.Processing;

            var expectedCompareQueueOrchestrationDependencyValidationException =
                new CompareQueueOrchestrationDependencyValidationException(
                    message: "Compare queue orchestration dependency validation error occurred, " +
                        "fix errors and try again.",
                    innerException: dependencyValidationException.InnerException as Xeption);

            this.fhirRecordServiceMock.Setup(service =>
                service.RetrieveFhirRecordByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(dependencyValidationException);

            // when
            ValueTask changeFhirRecordStatusTask =
                this.compareQueueOrchestrationService
                    .ChangeFhirRecordStatusAsync(inputFhirRecordId, inputStatus);

            CompareQueueOrchestrationDependencyValidationException
                actualCompareQueueOrchestrationDependencyValidationException =
                    await Assert.ThrowsAsync<CompareQueueOrchestrationDependencyValidationException>(
                        testCode: changeFhirRecordStatusTask.AsTask);

            // then
            actualCompareQueueOrchestrationDependencyValidationException
                .Should().BeEquivalentTo(expectedCompareQueueOrchestrationDependencyValidationException);

            this.fhirRecordServiceMock.Verify(service =>
                service.RetrieveFhirRecordByIdAsync(It.IsAny<Guid>()),
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
        public async Task ShouldThrowDependencyExceptionOnChangeFhirRecordStatusAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            Guid randomFhirRecordId = Guid.NewGuid();
            Guid inputFhirRecordId = randomFhirRecordId;
            StatusType inputStatus = StatusType.Processing;

            var expectedCompareQueueOrchestrationDependencyException =
                new CompareQueueOrchestrationDependencyException(
                    message: "Compare queue orchestration dependency error occurred, please contact support.",
                    innerException: dependencyException.InnerException as Xeption);

            this.fhirRecordServiceMock.Setup(service =>
                service.RetrieveFhirRecordByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(dependencyException);

            // when
            ValueTask changeFhirRecordStatusTask =
                this.compareQueueOrchestrationService
                    .ChangeFhirRecordStatusAsync(inputFhirRecordId, inputStatus);

            CompareQueueOrchestrationDependencyException
                actualCompareQueueOrchestrationDependencyException =
                    await Assert.ThrowsAsync<CompareQueueOrchestrationDependencyException>(
                        testCode: changeFhirRecordStatusTask.AsTask);

            // then
            actualCompareQueueOrchestrationDependencyException
                .Should().BeEquivalentTo(expectedCompareQueueOrchestrationDependencyException);

            this.fhirRecordServiceMock.Verify(service =>
                service.RetrieveFhirRecordByIdAsync(It.IsAny<Guid>()),
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
        public async Task ShouldThrowServiceExceptionOnChangeFhirRecordStatusAndLogItAsync()
        {
            // given
            Guid randomFhirRecordId = Guid.NewGuid();
            Guid inputFhirRecordId = randomFhirRecordId;
            StatusType inputStatus = StatusType.Processing;
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

            this.fhirRecordServiceMock.Setup(service =>
                service.RetrieveFhirRecordByIdAsync(It.IsAny<Guid>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask changeFhirRecordStatusTask =
                this.compareQueueOrchestrationService
                    .ChangeFhirRecordStatusAsync(inputFhirRecordId, inputStatus);

            CompareQueueOrchestrationServiceException
                actualCompareQueueOrchestrationServiceException =
                    await Assert.ThrowsAsync<CompareQueueOrchestrationServiceException>(
                        testCode: changeFhirRecordStatusTask.AsTask);

            // then
            actualCompareQueueOrchestrationServiceException
                .Should().BeEquivalentTo(expectedCompareQueueOrchestrationServiceException);

            this.fhirRecordServiceMock.Verify(service =>
                service.RetrieveFhirRecordByIdAsync(It.IsAny<Guid>()),
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
