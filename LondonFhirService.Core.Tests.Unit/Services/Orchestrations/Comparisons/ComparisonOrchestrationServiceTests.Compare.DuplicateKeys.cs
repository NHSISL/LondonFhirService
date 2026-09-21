// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Orchestrations.Comparisons;
using LondonFhirService.Core.Services.Orchestrations.Comparisons;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Comparisons
{
    /// <summary>
    /// A repeated match key is only ambiguous when the repeat differs from the resource already
    /// kept. These hold both halves of that: an identical repeat is dropped, and one that differs
    /// in anything the comparison would have reported is still raised for review.
    /// </summary>
    public partial class ComparisonOrchestrationServiceTests
    {
        private static string CreatePractitionerBundle(
            string firstId,
            string firstFamilyName,
            string secondId,
            string secondFamilyName) =>
            "{\"resourceType\":\"Bundle\",\"entry\":[" +
            CreatePractitionerEntry(firstId, firstFamilyName) + "," +
            CreatePractitionerEntry(secondId, secondFamilyName) +
            "]}";

        private static string CreatePractitionerEntry(string id, string familyName) =>
            "{\"resource\":{" +
            "\"resourceType\":\"Practitioner\"," +
            $"\"id\":\"{id}\"," +
            "\"identifier\":[" +
            "{\"system\":\"https://fhir.nhs.uk/Id/sds-user-id\"}," +
            "{\"system\":\"https://fhir.hl7.org.uk/Id/dds\",\"value\":\"22528\"}]," +
            $"\"name\":[{{\"family\":\"{familyName}\",\"given\":[\"Kim\"]}}]" +
            "}}";

        /// <summary>
        /// The case that prompted this: one record carrying the same practitioner twice, differing
        /// only in the id each side minted for it. Nothing a reviewer could answer, so nothing is
        /// asked.
        /// </summary>
        [Fact]
        public async Task ShouldNotReportRepeatedKeyWhenRepeatIsIndistinguishableOnCompareAsync()
        {
            // given
            var loggingBrokerMock = new Mock<ILoggingBroker>();

            IComparisonOrchestrationService comparisonOrchestrationService =
                CreateServiceWithRealIgnoreRules(loggingBrokerMock);

            string source1Json = CreatePractitionerBundle(
                firstId: "92f740a2-88ad-4e76-b948-9583b85ecb15",
                firstFamilyName: "CROWE",
                secondId: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                secondFamilyName: "CROWE");

            string source2Json = CreatePractitionerBundle(
                firstId: "ffa73264-d1e2-4c3b-9a8f-1b2c3d4e5f60",
                firstFamilyName: "CROWE",
                secondId: "0badc0de-1111-2222-3333-444455556666",
                secondFamilyName: "CROWE");

            // when
            ComparisonResult actualComparisonResult =
                await comparisonOrchestrationService.CompareAsync(
                    GetRandomString(), source1Json, source2Json);

            // then
            actualComparisonResult.Diffs.Should().BeEmpty();
            actualComparisonResult.DiffCount.Should().Be(0);
        }

        /// <summary>
        /// Two genuinely different resources sharing a key is the case the manual-review report
        /// exists for - the engine cannot say which of them the other side's resource is, and
        /// guessing is what would put one practitioner's details against another's.
        /// </summary>
        [Fact]
        public async Task ShouldStillReportRepeatedKeyWhenRepeatDiffersOnCompareAsync()
        {
            // given
            var loggingBrokerMock = new Mock<ILoggingBroker>();

            IComparisonOrchestrationService comparisonOrchestrationService =
                CreateServiceWithRealIgnoreRules(loggingBrokerMock);

            string source1Json = CreatePractitionerBundle(
                firstId: "92f740a2-88ad-4e76-b948-9583b85ecb15",
                firstFamilyName: "CROWE",
                secondId: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                secondFamilyName: "ABBOTT");

            string source2Json = CreatePractitionerBundle(
                firstId: "ffa73264-d1e2-4c3b-9a8f-1b2c3d4e5f60",
                firstFamilyName: "CROWE",
                secondId: "0badc0de-1111-2222-3333-444455556666",
                secondFamilyName: "ABBOTT");

            // when
            ComparisonResult actualComparisonResult =
                await comparisonOrchestrationService.CompareAsync(
                    GetRandomString(), source1Json, source2Json);

            // then
            actualComparisonResult.Diffs
                .Where(diff => diff.Type == "manual-review-required")
                .Should().HaveCount(2);

            actualComparisonResult.Diffs
                .Should().OnlyContain(diff => diff.Identifier == "|22528");
        }

        /// <summary>
        /// A resource whose key has no counterpart on the other side is a real finding with
        /// nothing to be compared against, so suppression must never reach it.
        /// </summary>
        [Fact]
        public async Task ShouldStillReportUnmatchedResourceWithNoCounterpartOnCompareAsync()
        {
            // given
            var loggingBrokerMock = new Mock<ILoggingBroker>();

            IComparisonOrchestrationService comparisonOrchestrationService =
                CreateServiceWithRealIgnoreRules(loggingBrokerMock);

            string source1Json = CreatePractitionerBundle(
                firstId: "92f740a2-88ad-4e76-b948-9583b85ecb15",
                firstFamilyName: "CROWE",
                secondId: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                secondFamilyName: "CROWE");

            string source2Json = "{\"resourceType\":\"Bundle\",\"entry\":[]}";

            // when
            ComparisonResult actualComparisonResult =
                await comparisonOrchestrationService.CompareAsync(
                    GetRandomString(), source1Json, source2Json);

            // then
            actualComparisonResult.Diffs.Should().NotBeEmpty();

            actualComparisonResult.Diffs
                .Should().OnlyContain(diff => diff.Type == "manual-review-required");
        }
    }
}
