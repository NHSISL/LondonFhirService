// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.ReferralRequests
{
    public partial class ReferralRequestMatcherServiceTests
    {
        [Fact]
        public async Task ShouldMatchReferralRequestsWhenBothSourcesHaveResourcesWithSameKeyAsync()
        {
            // given
            string inputDdsIdentifierValue = "RR-1";

            JsonElement source1Resource = CreateReferralRequestResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: "referral-request-1");

            JsonElement source2Resource = CreateReferralRequestResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: "referral-request-2");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Matched.Add(
                new MatchedResource(source1Resource, source2Resource, inputDdsIdentifierValue));

            // when
            ResourceMatch actualResourceMatch = await this.referralRequestMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Should().BeEquivalentTo(expectedResourceMatch);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldAddToUnmatchedFromSource1WhenOnlySource1HasReferralRequestAsync()
        {
            // given
            string inputDdsIdentifierValue = "RR-1";

            JsonElement source1Resource = CreateReferralRequestResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: "referral-request-1");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement>();
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Unmatched.Add(
                new UnmatchedResource(source1Resource, "ReferralRequest", inputDdsIdentifierValue, true));

            // when
            ResourceMatch actualResourceMatch = await this.referralRequestMatcherService.MatchAsync(
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
