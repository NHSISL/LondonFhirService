// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Procedures
{
    public partial class ProcedureMatcherServiceTests
    {
        [Fact]
        public async Task ShouldMatchProceduresWhenBothSourcesHaveResourcesWithSameKeyAsync()
        {
            // given
            string inputDdsIdentifierValue = "PROC-1";

            JsonElement source1Resource = CreateProcedureResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: "procedure-1");

            JsonElement source2Resource = CreateProcedureResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: "procedure-2");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Matched.Add(
                new MatchedResource(source1Resource, source2Resource, inputDdsIdentifierValue));

            // when
            ResourceMatch actualResourceMatch = await this.procedureMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldAddToUnmatchedFromSource1WhenOnlySource1HasProcedureAsync()
        {
            // given
            string inputDdsIdentifierValue = "PROC-1";

            JsonElement source1Resource = CreateProcedureResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: "procedure-1");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement>();
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(source1Resource, "Procedure", inputDdsIdentifierValue, true));

            // when
            ResourceMatch actualResourceMatch = await this.procedureMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldAddToUnmatchedFromSource2WhenOnlySource2HasProcedureAsync()
        {
            // given
            string inputDdsIdentifierValue = "PROC-1";

            JsonElement source2Resource = CreateProcedureResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: "procedure-1");

            var source1Resources = new List<JsonElement>();
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(source2Resource, "Procedure", inputDdsIdentifierValue, false));

            // when
            ResourceMatch actualResourceMatch = await this.procedureMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
