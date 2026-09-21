// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Practitioners
{
    /// <summary>
    /// Feeds have been seen carrying the SDS identifier entry with its system and no value. Keyed
    /// on SDS alone those practitioners produced no key, and a resource with no key is dropped
    /// before matching - not paired, not reported as unmatched, simply absent. Every practitioner
    /// in a record could go uncompared and the result would still read clean, which is what these
    /// cover.
    /// </summary>
    public partial class PractitionerMatcherServiceTests
    {
        [Fact]
        public async Task ShouldFallBackToDdsIdentifierWhenSdsUserIdHasNoValueOnGetMatchKeyAsync()
        {
            // given
            JsonElement practitionerResource = CreateValuelessSdsPractitionerResource("22338", "p-1");

            // when
            string actualMatchKey = await this.practitionerMatcherService.GetMatchKeyAsync(
                practitionerResource,
                CreateResourceIndex());

            // then
            actualMatchKey.Should().Be("|22338");
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPreferSdsUserIdOverDdsIdentifierOnGetMatchKeyAsync()
        {
            // given
            JsonElement practitionerResource =
                CreateDualIdentifierPractitionerResource("G123456", "22338", "p-1");

            // when
            string actualMatchKey = await this.practitionerMatcherService.GetMatchKeyAsync(
                practitionerResource,
                CreateResourceIndex());

            // then
            actualMatchKey.Should().Be("G123456");
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// The behaviour that was missing: these practitioners used to vanish from the comparison
        /// entirely rather than pair with their counterpart.
        /// </summary>
        [Fact]
        public async Task ShouldMatchPractitionersOnDdsIdentifierWhenSdsUserIdIsEmptyOnMatchAsync()
        {
            // given
            var source1Resources = new List<JsonElement>
            {
                CreateValuelessSdsPractitionerResource("22338", "p-1"),
                CreateValuelessSdsPractitionerResource("22528", "p-2")
            };

            var source2Resources = new List<JsonElement>
            {
                CreateValuelessSdsPractitionerResource("22338", "p-3"),
                CreateValuelessSdsPractitionerResource("22528", "p-4")
            };

            // when
            ResourceMatch actualResourceMatch = await this.practitionerMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                CreateResourceIndex(),
                CreateResourceIndex());

            // then
            actualResourceMatch.Unmatched.Should().BeEmpty();
            actualResourceMatch.Matched.Should().HaveCount(2);

            actualResourceMatch.Matched.Select(match => match.MatchKey)
                .Should().BeEquivalentTo(new[] { "|22338", "|22528" });

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// A practitioner with neither identifier still has no key. That is unchanged, and it is
        /// the one case where a resource is still dropped silently.
        /// </summary>
        [Fact]
        public async Task ShouldReturnNullOnGetMatchKeyWhenNeitherIdentifierIsUsableAsync()
        {
            // given
            JsonElement practitionerResource =
                CreatePractitionerResourceWithoutIdentifierProperty(id: "p-1");

            // when
            string actualMatchKey = await this.practitionerMatcherService.GetMatchKeyAsync(
                practitionerResource,
                CreateResourceIndex());

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// An SDS id and a DDS identifier that happen to read the same must not be taken for the
        /// same practitioner - the DDS form carries its practice prefix, which an SDS id never has.
        /// </summary>
        [Fact]
        public async Task ShouldKeepSdsAndDdsKeySpacesApartOnMatchAsync()
        {
            // given
            JsonElement bySdsResource =
                CreateDualIdentifierPractitionerResource("22338", "99999", "p-1");

            JsonElement byDdsResource =
                CreateValuelessSdsPractitionerResource("22338", "p-2");

            // when
            ResourceMatch actualResourceMatch = await this.practitionerMatcherService.MatchAsync(
                new List<JsonElement> { bySdsResource },
                new List<JsonElement> { byDdsResource },
                CreateResourceIndex(),
                CreateResourceIndex());

            // then
            actualResourceMatch.Matched.Should().BeEmpty();
            actualResourceMatch.Unmatched.Should().HaveCount(2);

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        private static JsonElement CreateValuelessSdsPractitionerResource(
            string ddsIdentifierValue,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "Practitioner",
                "id": "{{id}}",
                "identifier": [
                  { "system": "https://fhir.nhs.uk/Id/sds-user-id" },
                  {
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "{{ddsIdentifierValue}}"
                  }
                ]
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateDualIdentifierPractitionerResource(
            string sdsUserIdValue,
            string ddsIdentifierValue,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "Practitioner",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "https://fhir.nhs.uk/Id/sds-user-id",
                    "value": "{{sdsUserIdValue}}"
                  },
                  {
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "{{ddsIdentifierValue}}"
                  }
                ]
              }
              """;

            return ParseJsonElement(json);
        }
    }
}
