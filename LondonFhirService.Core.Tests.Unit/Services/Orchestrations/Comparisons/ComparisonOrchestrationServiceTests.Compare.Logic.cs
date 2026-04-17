// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Orchestrations.Comparisons;
using Moq;
using Xunit;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Comparisons
{
    public partial class ComparisonOrchestrationServiceTests
    {
        [Fact]
        public async Task ShouldCompareAsync()
        {
            // given
            string randomCorrelationId = GetRandomString();
            string inputCorrelationId = randomCorrelationId;
            string inputSource1Json = GetRandomJson();
            string inputSource2Json = GetRandomJson();

            this.resourceMatcherProcessingServiceMock
                .Setup(service => service.GetMatcherAsync(It.IsAny<string>()))
                    .ReturnsAsync((null as object) as
                        LondonFhirService.Core.Services.Foundations.ResourceMatchers.IResourceMatcherService);

            var expectedComparisonResult = new ComparisonResult
            {
                CorrelationId = inputCorrelationId,
                DiffCount = 0,
                Diffs = new List<Models.Processings.ListEntryComparisons.DiffItem>()
            };

            // when
            ComparisonResult actualComparisonResult =
                await this.comparisonOrchestrationService.CompareAsync(
                    correlationId: inputCorrelationId,
                    source1Json: inputSource1Json,
                    source2Json: inputSource2Json);

            // then
            actualComparisonResult.CorrelationId.Should().Be(expectedComparisonResult.CorrelationId);

            this.resourceMatcherProcessingServiceMock.Verify(service =>
                service.GetMatcherAsync(It.IsAny<string>()),
                    Times.Never);

            this.listEntryComparisonProcessingServiceMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
