// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Orchestrations.Comparisons;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Consumers
{
    public partial class ComparisonTests
    {
        [Fact]
        public async Task ShouldCompareJsonResultsAsync()
        {
            // given
            string correlationId = Guid.NewGuid().ToString();
            string json1 = await File.ReadAllTextAsync("Comparison/json1.txt");
            string json2 = await File.ReadAllTextAsync("Comparison/json2.txt");

            // when
            ComparisonResult comparisonResult = await this.comparisonOrchestrationService.CompareAsync(
                      correlationId: correlationId,
                      source1Json: json1,
                      source2Json: json2);

            // then
            comparisonResult.Should().NotBeNull();
            //comparisonResult.CorrelationId.Should().Be(correlationId);
            //this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
