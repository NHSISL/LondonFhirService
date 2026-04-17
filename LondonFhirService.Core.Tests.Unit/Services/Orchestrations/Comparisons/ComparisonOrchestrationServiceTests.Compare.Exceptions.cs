// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Orchestrations.Comparisons;
using LondonFhirService.Core.Models.Orchestrations.Comparisons.Exceptions;
using LondonFhirService.Core.Services.Orchestrations.Comparisons;
using LondonFhirService.Core.Services.Processings.JsonIgnoreRules;
using Moq;
using Xeptions;

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

            var failedComparisonOrchestrationServiceException =
                new FailedComparisonOrchestrationServiceException(
                    message: "Issue comparing resource Patient",
                    innerException: dependencyValidationException.InnerException,
                    data: null);

            var loopAggregateException = new AggregateException(
                message: "1 errors occurred during comparison. See inner exceptions for details.",
                innerExceptions: failedComparisonOrchestrationServiceException);

            var expectedComparisonOrchestrationServiceException =
                new ComparisonOrchestrationServiceException(
                    message: "Comparison orchestration service error occurred, please contact support.",
                    innerException: loopAggregateException);

            this.resourceMatcherProcessingServiceMock
                .Setup(service => service.GetMatcherAsync(It.IsAny<string>()))
                    .ThrowsAsync(dependencyValidationException);

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

            var failedComparisonOrchestrationServiceException =
                new FailedComparisonOrchestrationServiceException(
                    message: "Issue comparing resource Patient",
                    innerException: dependencyException.InnerException,
                    data: null);

            var loopAggregateException = new AggregateException(
                message: "1 errors occurred during comparison. See inner exceptions for details.",
                innerExceptions: failedComparisonOrchestrationServiceException);

            var expectedComparisonOrchestrationServiceException =
                new ComparisonOrchestrationServiceException(
                    message: "Comparison orchestration service error occurred, please contact support.",
                    innerException: loopAggregateException);

            this.resourceMatcherProcessingServiceMock
                .Setup(service => service.GetMatcherAsync(It.IsAny<string>()))
                    .ThrowsAsync(dependencyException);

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


        [Fact]
        public async Task ShouldThrowAggregateExceptionOnCompareIfServiceErrorOccursAndLogItAsync()
        {
            // given
            string randomCorrelationId = GetRandomString();
            string inputCorrelationId = randomCorrelationId;
            string randomSource1Json = GetRandomJsonWithResources();
            string inputSource1Json = randomSource1Json;
            string randomSource2Json = GetRandomJsonWithResources();
            string inputSource2Json = randomSource2Json;

            var thrownException = new Exception(
                message: "Some inner exception");

            var failedComparisonOrchestrationServiceException =
                new FailedComparisonOrchestrationServiceException(
                    message: "Issue comparing resource Patient",
                    innerException: thrownException,
                    data: null);

            var loopAggregateException = new AggregateException(
                message: "1 errors occurred during comparison. See inner exceptions for details.",
                innerExceptions: failedComparisonOrchestrationServiceException);

            var expectedComparisonOrchestrationServiceException =
                new ComparisonOrchestrationServiceException(
                    message: "Comparison orchestration service error occurred, please contact support.",
                    innerException: loopAggregateException);

            this.resourceMatcherProcessingServiceMock
                .Setup(service => service.GetMatcherAsync(It.IsAny<string>()))
                    .ThrowsAsync(thrownException);

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

            Mock<ComparisonOrchestrationService> comparisonOrchestrationServiceMock =
                new Mock<ComparisonOrchestrationService>(
                    new List<IJsonIgnoreProcessingRule>(),
                    this.resourceMatcherProcessingServiceMock.Object,
                    this.listEntryComparisonProcessingServiceMock.Object,
                    this.jsonElementServiceMock.Object,
                    this.loggingBrokerMock.Object)
                { CallBase = true };

            comparisonOrchestrationServiceMock
                .Setup(service =>
                    service.ValidateCompareArguments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                        .Throws(serviceException);

            // when
            ValueTask<ComparisonResult> compareTask =
                comparisonOrchestrationServiceMock.Object.CompareAsync(
                    correlationId: inputCorrelationId,
                    source1Json: inputSource1Json,
                    source2Json: inputSource2Json);

            ComparisonOrchestrationServiceException actualComparisonOrchestrationServiceException =
                await Assert.ThrowsAsync<ComparisonOrchestrationServiceException>(
                    testCode: compareTask.AsTask);

            // then
            actualComparisonOrchestrationServiceException
                .Should().BeEquivalentTo(expectedComparisonOrchestrationServiceException);

            comparisonOrchestrationServiceMock.Verify(service =>
                service.ValidateCompareArguments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedComparisonOrchestrationServiceException))),
                        Times.Once);

            this.resourceMatcherProcessingServiceMock.VerifyNoOtherCalls();
            this.listEntryComparisonProcessingServiceMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            comparisonOrchestrationServiceMock.VerifyNoOtherCalls();
        }
    }
}
