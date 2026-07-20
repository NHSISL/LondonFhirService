// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.ProcedureRequests
{
    public partial class ProcedureRequestMatcherServiceTests
    {
        [Fact]
        public async Task ShouldMatchProcedureRequestsWhenBothSourcesHaveResourcesWithSameKeyAsync()
        {
            // given
            string inputDdsIdentifierValue = "PRREQ-1";

            JsonElement source1Resource = CreateProcedureRequestResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: "procedure-request-1");

            JsonElement source2Resource = CreateProcedureRequestResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: "procedure-request-2");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Matched.Add(
                new MatchedResource(source1Resource, source2Resource, inputDdsIdentifierValue));

            // when
            ResourceMatch actualResourceMatch = await this.procedureRequestMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldAddToUnmatchedFromSource1WhenOnlySource1HasProcedureRequestAsync()
        {
            // given
            string inputDdsIdentifierValue = "PRREQ-1";

            JsonElement source1Resource = CreateProcedureRequestResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: "procedure-request-1");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement>();
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(source1Resource, "ProcedureRequest", inputDdsIdentifierValue, true));

            // when
            ResourceMatch actualResourceMatch = await this.procedureRequestMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldAddToUnmatchedFromSource2WhenOnlySource2HasProcedureRequestAsync()
        {
            // given
            string inputDdsIdentifierValue = "PRREQ-1";

            JsonElement source2Resource = CreateProcedureRequestResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: "procedure-request-1");

            var source1Resources = new List<JsonElement>();
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(source2Resource, "ProcedureRequest", inputDdsIdentifierValue, false));

            // when
            ResourceMatch actualResourceMatch = await this.procedureRequestMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldMatchComprehensiveProcedureRequestsWithMultipleIdentifierSystemsAsync()
        {
            // given
            string inputDdsIdentifierValue = "PRREQ-comprehensive-1";

            JsonElement source1Resource = CreateComprehensiveProcedureRequestResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: "procedure-request-comprehensive-1");

            JsonElement source2Resource = CreateComprehensiveProcedureRequestResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: "procedure-request-comprehensive-2");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Matched.Add(
                new MatchedResource(source1Resource, source2Resource, inputDdsIdentifierValue));

            // when
            ResourceMatch actualResourceMatch = await this.procedureRequestMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldExcludeProcedureRequestsWithNoDdsIdentifierFromMatchResultsAsync()
        {
            // given
            JsonElement source1Resource = CreateNonDdsProcedureRequestResource(id: "procedure-request-1");
            JsonElement source2Resource = CreateNonDdsProcedureRequestResource(id: "procedure-request-2");
            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            // when
            ResourceMatch actualResourceMatch = await this.procedureRequestMatcherService.MatchAsync(
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
