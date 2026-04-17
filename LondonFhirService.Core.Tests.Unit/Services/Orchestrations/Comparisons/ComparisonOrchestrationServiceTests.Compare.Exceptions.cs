// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Orchestrations.Comparisons;
using LondonFhirService.Core.Models.Orchestrations.Comparisons.Exceptions;
using Moq;
using Xeptions;
using Xunit;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Comparisons
{
    public partial class ComparisonOrchestrationServiceTests
    {
        [Theory]
        [MemberData(nameof(DependencyValidationExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionOnCompareAndLogItAsync(
            Xeption dependencyValidationException)
        {
            // given
            string randomCorrelationId = GetRandomString();
            string randomSource1Json = GetRandomJsonWithResources();
            string randomSource2Json = GetRandomJsonWithResources();

            this.resourceMatcherProcessingServiceMock
                .Setup(service => service.GetMatcherAsync(It.IsAny<string>()))
                    .ThrowsAsync(dependencyValidationException);

            var expectedComparisonOrchestrationDependencyValidationException =
                new ComparisonOrchestrationDependencyValidationException(
                    message: "Comparison orchestration dependency validation error occurred, " +
                        "fix the errors and try again.",
                    innerException: dependencyValidationException.InnerException as Xeption);

            // when
            ValueTask<ComparisonResult> compareTask =
                this.comparisonOrchestrationService.CompareAsync(
                    correlationId: randomCorrelationId,
                    source1Json: randomSource1Json,
                    source2Json: randomSource2Json);

            ComparisonOrchestrationDependencyValidationException
                actualComparisonOrchestrationDependencyValidationException =
                    await Assert.ThrowsAsync<ComparisonOrchestrationDependencyValidationException>(
                        testCode: compareTask.AsTask);

            // then
            actualComparisonOrchestrationDependencyValidationException
                .Should().BeEquivalentTo(expectedComparisonOrchestrationDependencyValidationException);

            this.resourceMatcherProcessingServiceMock.Verify(service =>
                service.GetMatcherAsync(It.IsAny<string>()),
                    Times.AtLeastOnce);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedComparisonOrchestrationDependencyValidationException))),
                        Times.Once);

            this.listEntryComparisonProcessingServiceMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(DependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnCompareAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            string randomCorrelationId = GetRandomString();
            string randomSource1Json = GetRandomJsonWithResources();
            string randomSource2Json = GetRandomJsonWithResources();

            this.resourceMatcherProcessingServiceMock
                .Setup(service => service.GetMatcherAsync(It.IsAny<string>()))
                    .ThrowsAsync(dependencyException);

            var expectedComparisonOrchestrationDependencyException =
                new ComparisonOrchestrationDependencyException(
                    message: "Comparison orchestration dependency error occurred, " +
                        "fix the errors and try again.",
                    innerException: dependencyException.InnerException as Xeption);

            // when
            ValueTask<ComparisonResult> compareTask =
                this.comparisonOrchestrationService.CompareAsync(
                    correlationId: randomCorrelationId,
                    source1Json: randomSource1Json,
                    source2Json: randomSource2Json);

            ComparisonOrchestrationDependencyException actualComparisonOrchestrationDependencyException =
                await Assert.ThrowsAsync<ComparisonOrchestrationDependencyException>(
                    testCode: compareTask.AsTask);

            // then
            actualComparisonOrchestrationDependencyException
                .Should().BeEquivalentTo(expectedComparisonOrchestrationDependencyException);

            this.resourceMatcherProcessingServiceMock.Verify(service =>
                service.GetMatcherAsync(It.IsAny<string>()),
                    Times.AtLeastOnce);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedComparisonOrchestrationDependencyException))),
                        Times.Once);

            this.listEntryComparisonProcessingServiceMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnCompareAndLogItAsync()
        {
            // given
            string randomCorrelationId = GetRandomString();
            string randomSource1Json = GetRandomJsonWithResources();
            string randomSource2Json = GetRandomJsonWithResources();
            var serviceException = new Exception();

            this.resourceMatcherProcessingServiceMock
                .Setup(service => service.GetMatcherAsync(It.IsAny<string>()))
                    .ThrowsAsync(serviceException);

            var failedComparisonOrchestrationServiceException =
                new FailedComparisonOrchestrationServiceException(
                    message: "Failed comparison orchestration service error occurred, please contact support.",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedComparisonOrchestrationServiceException =
                new ComparisonOrchestrationServiceException(
                    message: "Comparison orchestration service error occurred, please contact support.",
                    innerException: failedComparisonOrchestrationServiceException);

            // when
            ValueTask<ComparisonResult> compareTask =
                this.comparisonOrchestrationService.CompareAsync(
                    correlationId: randomCorrelationId,
                    source1Json: randomSource1Json,
                    source2Json: randomSource2Json);

            ComparisonOrchestrationServiceException actualComparisonOrchestrationServiceException =
                await Assert.ThrowsAsync<ComparisonOrchestrationServiceException>(
                    testCode: compareTask.AsTask);

            // then
            actualComparisonOrchestrationServiceException
                .Should().BeEquivalentTo(expectedComparisonOrchestrationServiceException);

            this.resourceMatcherProcessingServiceMock.Verify(service =>
                service.GetMatcherAsync(It.IsAny<string>()),
                    Times.AtLeastOnce);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedComparisonOrchestrationServiceException))),
                        Times.Once);

            this.listEntryComparisonProcessingServiceMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
