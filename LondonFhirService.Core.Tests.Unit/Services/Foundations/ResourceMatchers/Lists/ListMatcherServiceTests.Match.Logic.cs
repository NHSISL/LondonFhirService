// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Lists
{
    public partial class ListMatcherServiceTests
    {
        [Fact]
        public async Task ShouldMatchComprehensiveListsByTitleWithMultipleIdentifiersAsync()
        {
            // given
            string title = "Active Problem List";

            JsonElement source1Resource = CreateComprehensiveListResource(
                title: title,
                id: "list-comprehensive-1");

            JsonElement source2Resource = CreateComprehensiveListResource(
                title: title,
                id: "list-comprehensive-2");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            // when
            ResourceMatch actualResourceMatch = await this.listMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                source1ResourceIndex,
                source2ResourceIndex);

            // then
            actualResourceMatch.Unmatched.Should().BeEmpty();
            actualResourceMatch.Matched.Should().ContainSingle();
            actualResourceMatch.Matched.Single().MatchKey.Should().Be(title);

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// The case the grouping exists for: one provider files a heading as two lists, another as
        /// one. Matching resource to resource made that a difference; matching groupings does not.
        /// </summary>
        [Fact]
        public async Task ShouldMatchSplitListAgainstSingleListOfSameTitleAsync()
        {
            // given
            string title = "Active Allergies";
            string snomedCode = "886921000000105";

            JsonElement source1First = CreateTitledListResource(
                title: title,
                snomedCode: snomedCode,
                date: "2026-09-18T10:08:13+00:00",
                entryReferences: new[] { "AllergyIntolerance/one", "AllergyIntolerance/two" });

            JsonElement source1Second = CreateTitledListResource(
                title: title,
                snomedCode: snomedCode,
                date: "2026-09-18T10:08:39+00:00",
                entryReferences: new[] { "AllergyIntolerance/three" });

            JsonElement source2Single = CreateTitledListResource(
                title: title,
                snomedCode: snomedCode,
                date: "2026-09-18T11:00:00+00:00",
                entryReferences: new[]
                {
                    "AllergyIntolerance/one",
                    "AllergyIntolerance/two",
                    "AllergyIntolerance/three"
                });

            var source1Resources = new List<JsonElement> { source1First, source1Second };
            var source2Resources = new List<JsonElement> { source2Single };

            // when
            ResourceMatch actualResourceMatch = await this.listMatcherService.MatchAsync(
                source1Resources,
                source2Resources,
                CreateResourceIndex(),
                CreateResourceIndex());

            // then
            actualResourceMatch.Unmatched.Should().BeEmpty();
            actualResourceMatch.Matched.Should().ContainSingle();

            MatchedResource matched = actualResourceMatch.Matched.Single();
            matched.MatchKey.Should().Be(title);

            GetEntryReferences(matched.Source1).Should().BeEquivalentTo(
                GetEntryReferences(matched.Source2));

            GetEntryReferences(matched.Source1).Should().HaveCount(3);

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// date is the timestamp of when a source assembled its list. A merged grouping has no
        /// honest one to carry, and keeping an arbitrary member's would differ between any two
        /// independent retrievals and report as a difference every time.
        /// </summary>
        [Fact]
        public async Task ShouldDropDateFromMergedGroupingOnMatchAsync()
        {
            // given
            string title = "Active Allergies";

            JsonElement source1Resource = CreateTitledListResource(
                title: title,
                snomedCode: "886921000000105",
                date: "2026-09-18T10:08:13+00:00",
                entryReferences: new[] { "AllergyIntolerance/one" });

            JsonElement source2Resource = CreateTitledListResource(
                title: title,
                snomedCode: "886921000000105",
                date: "2026-09-18T11:00:00+00:00",
                entryReferences: new[] { "AllergyIntolerance/one" });

            // when
            ResourceMatch actualResourceMatch = await this.listMatcherService.MatchAsync(
                new List<JsonElement> { source1Resource },
                new List<JsonElement> { source2Resource },
                CreateResourceIndex(),
                CreateResourceIndex());

            // then
            MatchedResource matched = actualResourceMatch.Matched.Single();
            matched.Source1.TryGetProperty("date", out _).Should().BeFalse();
            matched.Source2.TryGetProperty("date", out _).Should().BeFalse();
            matched.Source1.TryGetProperty("title", out _).Should().BeTrue();

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Same title, different SNOMED code - an active list and a resolved one a provider has
        /// titled alike. Folding those together would show a resolved allergy as current, so the
        /// grouping key keeps them apart.
        /// </summary>
        [Fact]
        public async Task ShouldNotGroupListsSharingTitleWithDifferentCodesOnMatchAsync()
        {
            // given
            string title = "Allergies";

            JsonElement activeList = CreateTitledListResource(
                title: title,
                snomedCode: "886921000000105",
                date: "2026-09-18T10:08:13+00:00",
                entryReferences: new[] { "AllergyIntolerance/one" });

            JsonElement resolvedList = CreateTitledListResource(
                title: title,
                snomedCode: "1103671000000101",
                date: "2026-09-18T10:08:39+00:00",
                entryReferences: new[] { "AllergyIntolerance/two" });

            var resources = new List<JsonElement> { activeList, resolvedList };

            // when
            ResourceMatch actualResourceMatch = await this.listMatcherService.MatchAsync(
                resources,
                resources,
                CreateResourceIndex(),
                CreateResourceIndex());

            // then
            actualResourceMatch.Unmatched.Should().BeEmpty();
            actualResourceMatch.Matched.Should().HaveCount(2);

            actualResourceMatch.Matched
                .SelectMany(match => GetEntryReferences(match.Source1))
                .Should().BeEquivalentTo(new[] { "AllergyIntolerance/one", "AllergyIntolerance/two" });

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Membership is what is left to disagree about once the grouping absorbs how many list
        /// resources a provider used, so it still has to be visible.
        /// </summary>
        [Fact]
        public async Task ShouldKeepMembershipDifferenceVisibleAcrossGroupingOnMatchAsync()
        {
            // given
            string title = "Active Allergies";

            JsonElement source1Resource = CreateTitledListResource(
                title: title,
                snomedCode: "886921000000105",
                date: "2026-09-18T10:08:13+00:00",
                entryReferences: new[] { "AllergyIntolerance/one", "AllergyIntolerance/two" });

            JsonElement source2Resource = CreateTitledListResource(
                title: title,
                snomedCode: "886921000000105",
                date: "2026-09-18T10:08:13+00:00",
                entryReferences: new[] { "AllergyIntolerance/one" });

            // when
            ResourceMatch actualResourceMatch = await this.listMatcherService.MatchAsync(
                new List<JsonElement> { source1Resource },
                new List<JsonElement> { source2Resource },
                CreateResourceIndex(),
                CreateResourceIndex());

            // then
            MatchedResource matched = actualResourceMatch.Matched.Single();
            GetEntryReferences(matched.Source1).Should().HaveCount(2);
            GetEntryReferences(matched.Source2).Should().HaveCount(1);

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// A heading only one side carries is still unmatched, and still reported once rather than
        /// once per list resource the side happened to split it across.
        /// </summary>
        [Fact]
        public async Task ShouldReportGroupingPresentOnOneSideOnlyAsUnmatchedOnceOnMatchAsync()
        {
            // given
            JsonElement first = CreateTitledListResource(
                title: "Problems",
                snomedCode: "717711000000103",
                date: "2026-09-18T10:08:13+00:00",
                entryReferences: new[] { "Condition/one" });

            JsonElement second = CreateTitledListResource(
                title: "Problems",
                snomedCode: "717711000000103",
                date: "2026-09-18T10:08:39+00:00",
                entryReferences: new[] { "Condition/two" });

            // when
            ResourceMatch actualResourceMatch = await this.listMatcherService.MatchAsync(
                new List<JsonElement> { first, second },
                new List<JsonElement>(),
                CreateResourceIndex(),
                CreateResourceIndex());

            // then
            actualResourceMatch.Matched.Should().BeEmpty();
            actualResourceMatch.Unmatched.Should().ContainSingle();
            actualResourceMatch.Unmatched.Single().Identifier.Should().Be("Problems");
            actualResourceMatch.Unmatched.Single().IsFromSource1.Should().BeTrue();
            GetEntryReferences(actualResourceMatch.Unmatched.Single().Resource).Should().HaveCount(2);

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// A grouping whose lists carry no entries at all is still shaped like one that does, so
        /// the absent array cannot read as a difference in its own right.
        /// </summary>
        [Fact]
        public async Task ShouldWriteEmptyEntryArrayWhenNoListInGroupingHasEntriesOnMatchAsync()
        {
            // given
            JsonElement withoutEntries = ParseJsonElement("""
              {
                "resourceType": "List",
                "id": "list-1",
                "title": "Medication List",
                "status": "current"
              }
              """);

            // when
            ResourceMatch actualResourceMatch = await this.listMatcherService.MatchAsync(
                new List<JsonElement> { withoutEntries },
                new List<JsonElement> { withoutEntries },
                CreateResourceIndex(),
                CreateResourceIndex());

            // then
            MatchedResource matched = actualResourceMatch.Matched.Single();
            matched.Source1.TryGetProperty("entry", out JsonElement entry).Should().BeTrue();
            entry.ValueKind.Should().Be(JsonValueKind.Array);
            entry.GetArrayLength().Should().Be(0);

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        private static List<string> GetEntryReferences(JsonElement list) =>
            list.TryGetProperty("entry", out JsonElement entry) && entry.ValueKind == JsonValueKind.Array
                ? entry.EnumerateArray()
                    .Select(item => item.GetProperty("item").GetProperty("reference").GetString())
                    .ToList()
                : new List<string>();

        private static JsonElement CreateTitledListResource(
            string title,
            string snomedCode,
            string date,
            string[] entryReferences)
        {
            string entries = string.Join(",", entryReferences
                .Select(reference => "{\"item\":{\"reference\":\"" + reference + "\"}}"));

            string json = $$"""
              {
                "resourceType": "List",
                "id": "list-{{title.GetHashCode():X}}-{{date.GetHashCode():X}}",
                "status": "current",
                "mode": "snapshot",
                "title": "{{title}}",
                "code": {
                  "coding": [
                    {
                      "system": "http://snomed.info/sct",
                      "code": "{{snomedCode}}"
                    }
                  ]
                },
                "date": "{{date}}",
                "entry": [{{entries}}]
              }
              """;

            return ParseJsonElement(json);
        }
    }
}
