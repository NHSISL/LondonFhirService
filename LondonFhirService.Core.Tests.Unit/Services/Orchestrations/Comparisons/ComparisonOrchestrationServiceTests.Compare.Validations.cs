// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Orchestrations.Comparisons;
using LondonFhirService.Core.Models.Orchestrations.Comparisons.Exceptions;
using Moq;
using Xunit;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Comparisons
{
    public partial class ComparisonOrchestrationServiceTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ShouldThrowValidationExceptionOnCompareIfCorrelationIdIsInvalidAndLogItAsync(
            string invalidCorrelationId)
        {
            // given
            string randomSource1Json = GetRandomJson();
            string randomSource2Json = GetRandomJson();

            var invalidComparisonOrchestrationException =
                new InvalidComparisonOrchestrationException(
                    message: "Invalid comparison orchestration argument(s), " +
                        "fix the errors and try again.");

            invalidComparisonOrchestrationException.UpsertDataList(
                key: "correlationId",
                value: "Text is invalid");

            var expectedComparisonOrchestrationValidationException =
                new ComparisonOrchestrationValidationException(
                    message: "Comparison orchestration validation error occurred, " +
                        "fix the errors and try again.",
                    innerException: invalidComparisonOrchestrationException);

            // when
            ValueTask<ComparisonResult> compareTask =
                this.comparisonOrchestrationService.CompareAsync(
                    correlationId: invalidCorrelationId,
                    source1Json: randomSource1Json,
                    source2Json: randomSource2Json);

            ComparisonOrchestrationValidationException actualComparisonOrchestrationValidationException =
                await Assert.ThrowsAsync<ComparisonOrchestrationValidationException>(
                    testCode: compareTask.AsTask);

            // then
            actualComparisonOrchestrationValidationException
                .Should().BeEquivalentTo(expectedComparisonOrchestrationValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedComparisonOrchestrationValidationException))),
                        Times.Once);

            this.resourceMatcherProcessingServiceMock.VerifyNoOtherCalls();
            this.listEntryComparisonProcessingServiceMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ShouldThrowValidationExceptionOnCompareIfSource1JsonIsInvalidAndLogItAsync(
            string invalidSource1Json)
        {
            // given
            string randomCorrelationId = GetRandomString();
            string randomSource2Json = GetRandomJson();

            var invalidComparisonOrchestrationException =
                new InvalidComparisonOrchestrationException(
                    message: "Invalid comparison orchestration argument(s), " +
                        "fix the errors and try again.");

            invalidComparisonOrchestrationException.UpsertDataList(
                key: "source1Json",
                value: "Text is invalid");

            var expectedComparisonOrchestrationValidationException =
                new ComparisonOrchestrationValidationException(
                    message: "Comparison orchestration validation error occurred, " +
                        "fix the errors and try again.",
                    innerException: invalidComparisonOrchestrationException);

            // when
            ValueTask<ComparisonResult> compareTask =
                this.comparisonOrchestrationService.CompareAsync(
                    correlationId: randomCorrelationId,
                    source1Json: invalidSource1Json,
                    source2Json: randomSource2Json);

            ComparisonOrchestrationValidationException actualComparisonOrchestrationValidationException =
                await Assert.ThrowsAsync<ComparisonOrchestrationValidationException>(
                    testCode: compareTask.AsTask);

            // then
            actualComparisonOrchestrationValidationException
                .Should().BeEquivalentTo(expectedComparisonOrchestrationValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedComparisonOrchestrationValidationException))),
                        Times.Once);

            this.resourceMatcherProcessingServiceMock.VerifyNoOtherCalls();
            this.listEntryComparisonProcessingServiceMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ShouldThrowValidationExceptionOnCompareIfSource2JsonIsInvalidAndLogItAsync(
            string invalidSource2Json)
        {
            // given
            string randomCorrelationId = GetRandomString();
            string randomSource1Json = GetRandomJson();

            var invalidComparisonOrchestrationException =
                new InvalidComparisonOrchestrationException(
                    message: "Invalid comparison orchestration argument(s), " +
                        "fix the errors and try again.");

            invalidComparisonOrchestrationException.UpsertDataList(
                key: "source2Json",
                value: "Text is invalid");

            var expectedComparisonOrchestrationValidationException =
                new ComparisonOrchestrationValidationException(
                    message: "Comparison orchestration validation error occurred, " +
                        "fix the errors and try again.",
                    innerException: invalidComparisonOrchestrationException);

            // when
            ValueTask<ComparisonResult> compareTask =
                this.comparisonOrchestrationService.CompareAsync(
                    correlationId: randomCorrelationId,
                    source1Json: randomSource1Json,
                    source2Json: invalidSource2Json);

            ComparisonOrchestrationValidationException actualComparisonOrchestrationValidationException =
                await Assert.ThrowsAsync<ComparisonOrchestrationValidationException>(
                    testCode: compareTask.AsTask);

            // then
            actualComparisonOrchestrationValidationException
                .Should().BeEquivalentTo(expectedComparisonOrchestrationValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedComparisonOrchestrationValidationException))),
                        Times.Once);

            this.resourceMatcherProcessingServiceMock.VerifyNoOtherCalls();
            this.listEntryComparisonProcessingServiceMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
