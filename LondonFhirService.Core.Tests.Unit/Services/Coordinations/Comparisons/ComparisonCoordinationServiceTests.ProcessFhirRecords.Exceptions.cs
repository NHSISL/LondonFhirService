// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Coordinations.Comparisons.Exceptions;
using Moq;
using Xeptions;
using Xunit;

namespace LondonFhirService.Core.Tests.Unit.Services.Coordinations.Comparisons
{
    public partial class ComparisonCoordinationServiceTests
    {
        [Theory]
        [MemberData(nameof(DependencyValidationExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionOnProcessFhirRecordsIfErrorOccursAndLogItAsync(
            Xeption dependencyValidationException)
        {
            // given
            var expectedComparisonCoordinationDependencyValidationException =
                new ComparisonCoordinationDependencyValidationException(
                    message: "Comparison coordination dependency validation error occurred, please try again.",
                    innerException: dependencyValidationException.InnerException as Xeption);

            this.compareQueueOrchestrationServiceMock.Setup(service =>
                service.GetUnprocessedRecordAsync())
                    .ThrowsAsync(dependencyValidationException);

            // when
            ValueTask processFhirRecordsTask =
                this.comparisonCoordinationService.ProcessFhirRecordsAsync();

            ComparisonCoordinationDependencyValidationException
                actualComparisonCoordinationDependencyValidationException =
                    await Assert.ThrowsAsync<ComparisonCoordinationDependencyValidationException>(
                        processFhirRecordsTask.AsTask);

            // then
            actualComparisonCoordinationDependencyValidationException.Should()
                .BeEquivalentTo(expectedComparisonCoordinationDependencyValidationException);

            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.GetUnprocessedRecordAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedComparisonCoordinationDependencyValidationException))),
                        Times.Once);

            this.compareQueueOrchestrationServiceMock.VerifyNoOtherCalls();
            this.comparisonOrchestrationServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(DependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnProcessFhirRecordsIfErrorOccursAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            var expectedComparisonCoordinationDependencyException =
                new ComparisonCoordinationDependencyException(
                    message: "Comparison coordination dependency error occurred, fix the errors and try again.",
                    innerException: dependencyException.InnerException as Xeption);

            this.compareQueueOrchestrationServiceMock.Setup(service =>
                service.GetUnprocessedRecordAsync())
                    .ThrowsAsync(dependencyException);

            // when
            ValueTask processFhirRecordsTask =
                this.comparisonCoordinationService.ProcessFhirRecordsAsync();

            ComparisonCoordinationDependencyException actualComparisonCoordinationDependencyException =
                await Assert.ThrowsAsync<ComparisonCoordinationDependencyException>(
                    processFhirRecordsTask.AsTask);

            // then
            actualComparisonCoordinationDependencyException.Should()
                .BeEquivalentTo(expectedComparisonCoordinationDependencyException);

            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.GetUnprocessedRecordAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedComparisonCoordinationDependencyException))),
                        Times.Once);

            this.compareQueueOrchestrationServiceMock.VerifyNoOtherCalls();
            this.comparisonOrchestrationServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnProcessFhirRecordsIfErrorOccursAndLogItAsync()
        {
            // given
            string randomExceptionMessage = GetRandomString();
            Exception serviceException = new Exception(randomExceptionMessage);

            var failedComparisonCoordinationException =
                new FailedComparisonCoordinationException(
                    message: "Failed comparison coordination service error occurred, please contact support.",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedComparisonCoordinationServiceException =
                new ComparisonCoordinationServiceException(
                    message: "Comparison coordination service error occurred, please contact support.",
                    innerException: failedComparisonCoordinationException);

            this.compareQueueOrchestrationServiceMock.Setup(service =>
                service.GetUnprocessedRecordAsync())
                    .ThrowsAsync(serviceException);

            // when
            ValueTask processFhirRecordsTask =
                this.comparisonCoordinationService.ProcessFhirRecordsAsync();

            ComparisonCoordinationServiceException actualComparisonCoordinationServiceException =
                await Assert.ThrowsAsync<ComparisonCoordinationServiceException>(
                    processFhirRecordsTask.AsTask);

            // then
            actualComparisonCoordinationServiceException.Should()
                .BeEquivalentTo(expectedComparisonCoordinationServiceException);

            this.compareQueueOrchestrationServiceMock.Verify(service =>
                service.GetUnprocessedRecordAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedComparisonCoordinationServiceException))),
                        Times.Once);

            this.compareQueueOrchestrationServiceMock.VerifyNoOtherCalls();
            this.comparisonOrchestrationServiceMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
