// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Patients
{
    public partial class PatientMatcherServiceTests
    {
        [Fact]
        public async Task ShouldReturnEmptyMatchWhenBothSourcesAreEmptyAsync()
        {
            // given
            var source1Resources = new List<JsonElement>();
            var source2Resources = new List<JsonElement>();
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            // when
            ResourceMatch actualResourceMatch =
                await this.patientMatcherService.MatchAsync(
                    source1Resources,
                    source2Resources,
                    source1ResourceIndex,
                    source2ResourceIndex);

            // then
            actualResourceMatch.Matched.Should().BeEmpty();
            actualResourceMatch.Unmatched.Should().BeEmpty();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnMatchedResourceWhenBothSourcesHaveSameNhsNumberAsync()
        {
            // given
            string sharedNhsNumber = "9000000009";
            JsonElement source1PatientResource = CreatePatientWithNhsNumber(sharedNhsNumber, id: "patient-1");
            JsonElement source2PatientResource = CreatePatientWithNhsNumber(sharedNhsNumber, id: "patient-2");
            var source1Resources = new List<JsonElement> { source1PatientResource };
            var source2Resources = new List<JsonElement> { source2PatientResource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            // when
            ResourceMatch actualResourceMatch =
                await this.patientMatcherService.MatchAsync(
                    source1Resources,
                    source2Resources,
                    source1ResourceIndex,
                    source2ResourceIndex);

            // then
            actualResourceMatch.Matched.Should().HaveCount(1);
            actualResourceMatch.Matched[0].MatchKey.Should().Be(sharedNhsNumber);
            actualResourceMatch.Unmatched.Should().BeEmpty();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnUnmatchedResourceFromSource1WhenOnlySource1HasPatientAsync()
        {
            // given
            string nhsNumber = "9000000009";
            JsonElement source1PatientResource = CreatePatientWithNhsNumber(nhsNumber);
            var source1Resources = new List<JsonElement> { source1PatientResource };
            var source2Resources = new List<JsonElement>();
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            // when
            ResourceMatch actualResourceMatch =
                await this.patientMatcherService.MatchAsync(
                    source1Resources,
                    source2Resources,
                    source1ResourceIndex,
                    source2ResourceIndex);

            // then
            actualResourceMatch.Matched.Should().BeEmpty();
            actualResourceMatch.Unmatched.Should().HaveCount(1);
            actualResourceMatch.Unmatched[0].Identifier.Should().Be(nhsNumber);
            actualResourceMatch.Unmatched[0].IsFromSource1.Should().BeTrue();
            actualResourceMatch.Unmatched[0].ResourceType.Should().Be("Patient");
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnUnmatchedResourceFromSource2WhenOnlySource2HasPatientAsync()
        {
            // given
            string nhsNumber = "9000000009";
            JsonElement source2PatientResource = CreatePatientWithNhsNumber(nhsNumber);
            var source1Resources = new List<JsonElement>();
            var source2Resources = new List<JsonElement> { source2PatientResource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            // when
            ResourceMatch actualResourceMatch =
                await this.patientMatcherService.MatchAsync(
                    source1Resources,
                    source2Resources,
                    source1ResourceIndex,
                    source2ResourceIndex);

            // then
            actualResourceMatch.Matched.Should().BeEmpty();
            actualResourceMatch.Unmatched.Should().HaveCount(1);
            actualResourceMatch.Unmatched[0].Identifier.Should().Be(nhsNumber);
            actualResourceMatch.Unmatched[0].IsFromSource1.Should().BeFalse();
            actualResourceMatch.Unmatched[0].ResourceType.Should().Be("Patient");
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldExcludePatientWithoutNhsNumberFromMatchResultsAsync()
        {
            // given
            JsonElement source1PatientResource = CreatePatientWithoutIdentifier(id: "patient-1");
            JsonElement source2PatientResource = CreatePatientWithoutIdentifier(id: "patient-2");
            var source1Resources = new List<JsonElement> { source1PatientResource };
            var source2Resources = new List<JsonElement> { source2PatientResource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            // when
            ResourceMatch actualResourceMatch =
                await this.patientMatcherService.MatchAsync(
                    source1Resources,
                    source2Resources,
                    source1ResourceIndex,
                    source2ResourceIndex);

            // then
            actualResourceMatch.Matched.Should().BeEmpty();
            actualResourceMatch.Unmatched.Should().BeEmpty();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnMixedMatchedAndUnmatchedResourcesAsync()
        {
            // given
            string sharedNhsNumber = "9000000009";
            string source1OnlyNhsNumber = "9000000018";
            string source2OnlyNhsNumber = "9000000027";

            JsonElement source1MatchedPatient = CreatePatientWithNhsNumber(sharedNhsNumber, id: "patient-1a");
            JsonElement source1UnmatchedPatient = CreatePatientWithNhsNumber(source1OnlyNhsNumber, id: "patient-1b");
            JsonElement source2MatchedPatient = CreatePatientWithNhsNumber(sharedNhsNumber, id: "patient-2a");
            JsonElement source2UnmatchedPatient = CreatePatientWithNhsNumber(source2OnlyNhsNumber, id: "patient-2b");

            var source1Resources = new List<JsonElement>
            {
                source1MatchedPatient,
                source1UnmatchedPatient
            };

            var source2Resources = new List<JsonElement>
            {
                source2MatchedPatient,
                source2UnmatchedPatient
            };

            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            // when
            ResourceMatch actualResourceMatch =
                await this.patientMatcherService.MatchAsync(
                    source1Resources,
                    source2Resources,
                    source1ResourceIndex,
                    source2ResourceIndex);

            // then
            actualResourceMatch.Matched.Should().HaveCount(1);
            actualResourceMatch.Matched[0].MatchKey.Should().Be(sharedNhsNumber);
            actualResourceMatch.Unmatched.Should().HaveCount(2);

            actualResourceMatch.Unmatched.Should().Contain(unmatchedResource =>
                unmatchedResource.Identifier == source1OnlyNhsNumber
                && unmatchedResource.IsFromSource1 == true);

            actualResourceMatch.Unmatched.Should().Contain(unmatchedResource =>
                unmatchedResource.Identifier == source2OnlyNhsNumber
                && unmatchedResource.IsFromSource1 == false);

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldMatchComprehensivePatientsWithMultipleIdentifierSystemsAsync()
        {
            // given
            string nhsNumber = "9660979622";

            JsonElement source1Resource = CreateComprehensivePatientResource(
                nhsNumber: nhsNumber,
                id: "patient-comprehensive-1");

            JsonElement source2Resource = CreateComprehensivePatientResource(
                nhsNumber: nhsNumber,
                id: "patient-comprehensive-2");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();
            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Matched.Add(
                new MatchedResource(source1Resource, source2Resource, nhsNumber));

            // when
            ResourceMatch actualResourceMatch = await this.patientMatcherService.MatchAsync(
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
