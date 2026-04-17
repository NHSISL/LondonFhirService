// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Orchestrations.Comparisons;
using LondonFhirService.Core.Models.Processings.ListEntryComparisons;
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
            string randomSource1Json = GetRandomJson();
            string inputSource1Json = randomSource1Json;
            string randomSource2Json = GetRandomJson();
            string inputSource2Json = randomSource2Json;

            var expectedComparisonResult = new ComparisonResult
            {
                CorrelationId = inputCorrelationId,
                DiffCount = 0,
                Diffs = new List<DiffItem>()
            };

            // when
            ComparisonResult actualComparisonResult =
                await this.comparisonOrchestrationService.CompareAsync(
                    correlationId: inputCorrelationId,
                    source1Json: inputSource1Json,
                    source2Json: inputSource2Json);

            // then
            actualComparisonResult.Should().BeEquivalentTo(expectedComparisonResult);

            this.resourceMatcherProcessingServiceMock.VerifyNoOtherCalls();
            this.listEntryComparisonProcessingServiceMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
