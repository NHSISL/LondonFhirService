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
        public async Task ShouldThrowDependencyValidationExceptionOnCompareIfErrorsAndLogItAsync(
            Xeption dependencyValidationException)
        {
            // given
            string randomCorrelationId = GetRandomString();
            string inputCorrelationId = randomCorrelationId;
            string randomSource1Json = GetRandomJsonWithResources();
            string inputSource1Json = randomSource1Json;
            string randomSource2Json = GetRandomJsonWithResources();
            string inputSource2Json = randomSource2Json;

            var expectedComparisonOrchestrationDependencyValidationException =
                new ComparisonOrchestrationDependencyValidationException(
                    message: "Comparison orchestration dependency validation error occurred, " +
                        "fix the errors and try again.",
                    innerException: dependencyValidationException.InnerException as Xeption);

            this.resourceMatcherProcessingServiceMock
                .Setup(service => service.GetMatcherAsync(It.IsAny<string>()))
                    .ThrowsAsync(dependencyValidationException);

            // when
            ValueTask<ComparisonResult> compareTask =
                this.comparisonOrchestrationService.CompareAsync(
                    correlationId: inputCorrelationId,
                    source1Json: inputSource1Json,
                    source2Json: inputSource2Json);

            ComparisonOrchestrationDependencyValidationException
                actualComparisonOrchestrationDependencyValidationException =
                    await Assert.ThrowsAsync<ComparisonOrchestrationDependencyValidationException>(
                        testCode: compareTask.AsTask);

            // then
            actualComparisonOrchestrationDependencyValidationException
                .Should().BeEquivalentTo(expectedComparisonOrchestrationDependencyValidationException);

            this.resourceMatcherProcessingServiceMock.Verify(service =>
                service.GetMatcherAsync(It.IsAny<string>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedComparisonOrchestrationDependencyValidationException))),
                        Times.Once);

            this.resourceMatcherProcessingServiceMock.VerifyNoOtherCalls();
            this.listEntryComparisonProcessingServiceMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(DependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnCompareIfErrorsAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            string randomCorrelationId = GetRandomString();
            string inputCorrelationId = randomCorrelationId;
            string randomSource1Json = GetRandomJsonWithResources();
            string inputSource1Json = randomSource1Json;
            string randomSource2Json = GetRandomJsonWithResources();
            string inputSource2Json = randomSource2Json;

            var expectedComparisonOrchestrationDependencyException =
                new ComparisonOrchestrationDependencyException(
                    message: "Comparison orchestration dependency error occurred, " +
                        "fix the errors and try again.",
                    innerException: dependencyException.InnerException as Xeption);

            this.resourceMatcherProcessingServiceMock
                .Setup(service => service.GetMatcherAsync(It.IsAny<string>()))
                    .ThrowsAsync(dependencyException);

            // when
            ValueTask<ComparisonResult> compareTask =
                this.comparisonOrchestrationService.CompareAsync(
                    correlationId: inputCorrelationId,
                    source1Json: inputSource1Json,
                    source2Json: inputSource2Json);

            ComparisonOrchestrationDependencyException actualComparisonOrchestrationDependencyException =
                await Assert.ThrowsAsync<ComparisonOrchestrationDependencyException>(
                    testCode: compareTask.AsTask);

            // then
            actualComparisonOrchestrationDependencyException
                .Should().BeEquivalentTo(expectedComparisonOrchestrationDependencyException);

            this.resourceMatcherProcessingServiceMock.Verify(service =>
                service.GetMatcherAsync(It.IsAny<string>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedComparisonOrchestrationDependencyException))),
                        Times.Once);

            this.resourceMatcherProcessingServiceMock.VerifyNoOtherCalls();
            this.listEntryComparisonProcessingServiceMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnCompareIfServiceErrorOccursAndLogItAsync()
        {
            // given
            string randomCorrelationId = GetRandomString();
            string inputCorrelationId = randomCorrelationId;
            string randomSource1Json = GetRandomJsonWithResources();
            string inputSource1Json = randomSource1Json;
            string randomSource2Json = GetRandomJsonWithResources();
            string inputSource2Json = randomSource2Json;
            var serviceException = new Exception();

            var failedComparisonOrchestrationServiceException =
                new FailedComparisonOrchestrationServiceException(
                    message: "Failed comparison orchestration service error occurred, please contact support.",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedComparisonOrchestrationServiceException =
                new ComparisonOrchestrationServiceException(
                    message: "Comparison orchestration service error occurred, please contact support.",
                    innerException: failedComparisonOrchestrationServiceException);

            this.resourceMatcherProcessingServiceMock
                .Setup(service => service.GetMatcherAsync(It.IsAny<string>()))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<ComparisonResult> compareTask =
                this.comparisonOrchestrationService.CompareAsync(
                    correlationId: inputCorrelationId,
                    source1Json: inputSource1Json,
                    source2Json: inputSource2Json);

            ComparisonOrchestrationServiceException actualComparisonOrchestrationServiceException =
                await Assert.ThrowsAsync<ComparisonOrchestrationServiceException>(
                    testCode: compareTask.AsTask);

            // then
            actualComparisonOrchestrationServiceException
                .Should().BeEquivalentTo(expectedComparisonOrchestrationServiceException);

            this.resourceMatcherProcessingServiceMock.Verify(service =>
                service.GetMatcherAsync(It.IsAny<string>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedComparisonOrchestrationServiceException))),
                        Times.Once);

            this.resourceMatcherProcessingServiceMock.VerifyNoOtherCalls();
            this.listEntryComparisonProcessingServiceMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
