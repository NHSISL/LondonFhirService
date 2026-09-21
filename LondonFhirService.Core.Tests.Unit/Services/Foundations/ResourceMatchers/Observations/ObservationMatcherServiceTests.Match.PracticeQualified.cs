// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Observations
{
    /// <summary>
    /// A DDS identifier is unique within one practice's feed, not within a bundle. A patient
    /// registered at two practices carries two unrelated observations both claiming the same DDS
    /// id, and before the key was practice-qualified they collided: the first won and the rest
    /// were reported as manual-review-required.
    /// </summary>
    public partial class ObservationMatcherServiceTests
    {
        [Fact]
        public async Task ShouldMatchObservationsSharingDdsIdentifierAcrossPracticesAsync()
        {
            // given
            string sharedDdsIdentifier = "54";
            string firstPractice = "F84642";
            string secondPractice = "E84015";

            var source1Resources = new List<JsonElement>
            {
                CreateTaggedObservationResource(sharedDdsIdentifier, firstPractice, "obs-1"),
                CreateTaggedObservationResource(sharedDdsIdentifier, secondPractice, "obs-2")
            };

            var source2Resources = new List<JsonElement>
            {
                CreateTaggedObservationResource(sharedDdsIdentifier, firstPractice, "obs-3"),
                CreateTaggedObservationResource(sharedDdsIdentifier, secondPractice, "obs-4")
            };

            // when
            ResourceMatch actualResourceMatch = await this.observationMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                CreateResourceIndex(),
                CreateResourceIndex());

            // then
            actualResourceMatch.Unmatched.Should().BeEmpty();
            actualResourceMatch.Matched.Should().HaveCount(2);

            actualResourceMatch.Matched.Select(match => match.MatchKey)
                .Should().BeEquivalentTo(new[]
                {
                    ExpectedMatchKey(sharedDdsIdentifier, firstPractice),
                    ExpectedMatchKey(sharedDdsIdentifier, secondPractice)
                });

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Two observations from the SAME practice sharing an identifier is a genuine ambiguity in
        /// the source rather than a key that is too coarse, so it still goes to manual review -
        /// guessing which is which is what the practice qualifier exists to avoid, not to enable.
        /// </summary>
        [Fact]
        public async Task ShouldStillReportDuplicateWithinOnePracticeAsUnmatchedOnMatchAsync()
        {
            // given
            string sharedDdsIdentifier = "54";
            string practice = "F84642";

            var source1Resources = new List<JsonElement>
            {
                CreateTaggedObservationResource(sharedDdsIdentifier, practice, "obs-1"),
                CreateTaggedObservationResource(sharedDdsIdentifier, practice, "obs-2")
            };

            var source2Resources = new List<JsonElement>
            {
                CreateTaggedObservationResource(sharedDdsIdentifier, practice, "obs-3")
            };

            // when
            ResourceMatch actualResourceMatch = await this.observationMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                CreateResourceIndex(),
                CreateResourceIndex());

            // then
            actualResourceMatch.Matched.Should().ContainSingle();
            actualResourceMatch.Unmatched.Should().ContainSingle();

            actualResourceMatch.Unmatched.Single().Identifier
                .Should().Be(ExpectedMatchKey(sharedDdsIdentifier, practice));

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// An observation with no ODS tag keys on an empty practice rather than being dropped, so
        /// it groups exactly as it did before the qualifier existed.
        /// </summary>
        [Fact]
        public async Task ShouldMatchUntaggedObservationsOnBareIdentifierOnMatchAsync()
        {
            // given
            string ddsIdentifier = "54";

            JsonElement source1Resource = CreateObservationResource(
                ddsIdentifierValue: ddsIdentifier,
                id: "obs-1");

            JsonElement source2Resource = CreateObservationResource(
                ddsIdentifierValue: ddsIdentifier,
                id: "obs-2");

            // when
            ResourceMatch actualResourceMatch = await this.observationMatcherService.MatchAsync(
                new List<JsonElement> { source1Resource },
                new List<JsonElement> { source2Resource },
                CreateResourceIndex(),
                CreateResourceIndex());

            // then
            actualResourceMatch.Unmatched.Should().BeEmpty();
            actualResourceMatch.Matched.Should().ContainSingle();
            actualResourceMatch.Matched.Single().MatchKey.Should().Be(ExpectedMatchKey(ddsIdentifier));

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// A tagged resource and an untagged one carrying the same DDS id are not the same
        /// resource, and must not pair just because one side omitted its provenance.
        /// </summary>
        [Fact]
        public async Task ShouldNotMatchTaggedObservationAgainstUntaggedOneOnMatchAsync()
        {
            // given
            string ddsIdentifier = "54";

            JsonElement taggedResource =
                CreateTaggedObservationResource(ddsIdentifier, "F84642", "obs-1");

            JsonElement untaggedResource = CreateObservationResource(
                ddsIdentifierValue: ddsIdentifier,
                id: "obs-2");

            // when
            ResourceMatch actualResourceMatch = await this.observationMatcherService.MatchAsync(
                new List<JsonElement> { taggedResource },
                new List<JsonElement> { untaggedResource },
                CreateResourceIndex(),
                CreateResourceIndex());

            // then
            actualResourceMatch.Matched.Should().BeEmpty();
            actualResourceMatch.Unmatched.Should().HaveCount(2);

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        private static JsonElement CreateTaggedObservationResource(
            string ddsIdentifierValue,
            string odsCode,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "Observation",
                "id": "{{id}}",
                "meta": {
                  "tag": [
                    {
                      "system": "https://fhir.nhs.uk/Id/ODS-Code",
                      "code": "{{odsCode}}",
                      "display": "A Practice"
                    }
                  ]
                },
                "identifier": [
                  {
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "{{ddsIdentifierValue}}"
                  }
                ],
                "status": "final"
              }
              """;

            return ParseJsonElement(json);
        }
    }
}
