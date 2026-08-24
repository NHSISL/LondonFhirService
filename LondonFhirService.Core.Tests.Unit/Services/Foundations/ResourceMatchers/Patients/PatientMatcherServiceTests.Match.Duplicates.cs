// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Patients
{
    public partial class PatientMatcherServiceTests
    {
        [Fact]
        public async Task ShouldNotThrowWhenSourceBundleRepeatsTheSameNhsNumberAsync()
        {
            // given
            string duplicatedNhsNumber = "9000000009";

            JsonElement source1FirstPatientResource =
                CreatePatientWithNhsNumber(duplicatedNhsNumber, id: "patient-1-first");

            JsonElement source1DuplicatePatientResource =
                CreatePatientWithNhsNumber(duplicatedNhsNumber, id: "patient-1-duplicate");

            JsonElement source2FirstPatientResource =
                CreatePatientWithNhsNumber(duplicatedNhsNumber, id: "patient-2-first");

            JsonElement source2DuplicatePatientResource =
                CreatePatientWithNhsNumber(duplicatedNhsNumber, id: "patient-2-duplicate");

            var source1Resources = new List<JsonElement>
            {
                source1FirstPatientResource,
                source1DuplicatePatientResource
            };

            var source2Resources = new List<JsonElement>
            {
                source2FirstPatientResource,
                source2DuplicatePatientResource
            };

            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            Func<Task> matchAction = async () =>
            {
                await this.patientMatcherService.MatchAsync(
                    source1Resources,
                    source2Resources,
                    source1ResourceIndex,
                    source2ResourceIndex);
            };

            // when
            // then
            await matchAction.Should().NotThrowAsync();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldStillReturnMatchWhenSource1BundleRepeatsTheSameNhsNumberAsync()
        {
            // given
            string duplicatedNhsNumber = "9000000009";

            JsonElement source1FirstPatientResource =
                CreatePatientWithNhsNumber(duplicatedNhsNumber, id: "patient-1-first");

            JsonElement source1DuplicatePatientResource =
                CreatePatientWithNhsNumber(duplicatedNhsNumber, id: "patient-1-duplicate");

            JsonElement source2PatientResource =
                CreatePatientWithNhsNumber(duplicatedNhsNumber, id: "patient-2");

            var source1Resources = new List<JsonElement>
            {
                source1FirstPatientResource,
                source1DuplicatePatientResource
            };

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
            actualResourceMatch.Matched[0].MatchKey.Should().Be(duplicatedNhsNumber);
            // The dropped duplicate is reported rather than discarded, so a comparison
            // cannot read as clean while a resource is missing from it.
            actualResourceMatch.Unmatched.Should().ContainSingle(unmatched =>
                unmatched.Identifier == duplicatedNhsNumber);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldCarryFirstSource1OccurrenceIntoMatchWhenNhsNumberIsDuplicatedAsync()
        {
            // given
            string duplicatedNhsNumber = "9000000009";
            string expectedSource1Id = "patient-1-first";
            string droppedSource1Id = "patient-1-duplicate";

            JsonElement source1FirstPatientResource =
                CreatePatientWithNhsNumber(duplicatedNhsNumber, id: expectedSource1Id);

            JsonElement source1DuplicatePatientResource =
                CreatePatientWithNhsNumber(duplicatedNhsNumber, id: droppedSource1Id);

            JsonElement source2PatientResource =
                CreatePatientWithNhsNumber(duplicatedNhsNumber, id: "patient-2");

            var source1Resources = new List<JsonElement>
            {
                source1FirstPatientResource,
                source1DuplicatePatientResource
            };

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
            MatchedResource actualMatchedResource = actualResourceMatch.Matched.Single();
            actualMatchedResource.MatchKey.Should().Be(duplicatedNhsNumber);
            actualMatchedResource.Source1.GetProperty("id").GetString().Should().Be(expectedSource1Id);
            actualMatchedResource.Source1.GetProperty("id").GetString().Should().NotBe(droppedSource1Id);
            actualMatchedResource.Source2.GetProperty("id").GetString().Should().Be("patient-2");
            // The dropped duplicate is reported rather than discarded, so a comparison
            // cannot read as clean while a resource is missing from it.
            actualResourceMatch.Unmatched.Should().ContainSingle(unmatched =>
                unmatched.Identifier == duplicatedNhsNumber);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldCarryFirstSource2OccurrenceIntoMatchWhenNhsNumberIsDuplicatedAsync()
        {
            // given
            string duplicatedNhsNumber = "9000000009";
            string expectedSource2Id = "patient-2-first";
            string droppedSource2Id = "patient-2-duplicate";

            JsonElement source1PatientResource =
                CreatePatientWithNhsNumber(duplicatedNhsNumber, id: "patient-1");

            JsonElement source2FirstPatientResource =
                CreatePatientWithNhsNumber(duplicatedNhsNumber, id: expectedSource2Id);

            JsonElement source2DuplicatePatientResource =
                CreatePatientWithNhsNumber(duplicatedNhsNumber, id: droppedSource2Id);

            var source1Resources = new List<JsonElement> { source1PatientResource };

            var source2Resources = new List<JsonElement>
            {
                source2FirstPatientResource,
                source2DuplicatePatientResource
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
            MatchedResource actualMatchedResource = actualResourceMatch.Matched.Single();
            actualMatchedResource.MatchKey.Should().Be(duplicatedNhsNumber);
            actualMatchedResource.Source2.GetProperty("id").GetString().Should().Be(expectedSource2Id);
            actualMatchedResource.Source2.GetProperty("id").GetString().Should().NotBe(droppedSource2Id);
            // The dropped duplicate is reported rather than discarded, so a comparison
            // cannot read as clean while a resource is missing from it.
            actualResourceMatch.Unmatched.Should().ContainSingle(unmatched =>
                unmatched.Identifier == duplicatedNhsNumber);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldStillCompareRemainingPatientsWhenBundleRepeatsAnNhsNumberAsync()
        {
            // given
            string duplicatedNhsNumber = "9000000009";
            string otherSharedNhsNumber = "9000000018";
            string source2OnlyNhsNumber = "9000000027";

            JsonElement source1FirstDuplicatePatient =
                CreatePatientWithNhsNumber(duplicatedNhsNumber, id: "patient-1-first");

            JsonElement source1SecondDuplicatePatient =
                CreatePatientWithNhsNumber(duplicatedNhsNumber, id: "patient-1-duplicate");

            JsonElement source1OtherPatient =
                CreatePatientWithNhsNumber(otherSharedNhsNumber, id: "patient-1-other");

            JsonElement source2DuplicatedKeyPatient =
                CreatePatientWithNhsNumber(duplicatedNhsNumber, id: "patient-2-a");

            JsonElement source2OtherPatient =
                CreatePatientWithNhsNumber(otherSharedNhsNumber, id: "patient-2-b");

            JsonElement source2UnmatchedPatient =
                CreatePatientWithNhsNumber(source2OnlyNhsNumber, id: "patient-2-c");

            var source1Resources = new List<JsonElement>
            {
                source1FirstDuplicatePatient,
                source1SecondDuplicatePatient,
                source1OtherPatient
            };

            var source2Resources = new List<JsonElement>
            {
                source2DuplicatedKeyPatient,
                source2OtherPatient,
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
            actualResourceMatch.Matched.Should().HaveCount(2);

            actualResourceMatch.Matched.Should().Contain(matchedResource =>
                matchedResource.MatchKey == duplicatedNhsNumber);

            actualResourceMatch.Matched.Should().Contain(matchedResource =>
                matchedResource.MatchKey == otherSharedNhsNumber);

            // Two unmatched now: the genuinely unpaired source2 patient, and the source1
            // duplicate that lost the key. Reporting the second is the point - it used to
            // disappear from the comparison entirely.
            actualResourceMatch.Unmatched.Should().HaveCount(2);

            actualResourceMatch.Unmatched.Should().Contain(unmatchedResource =>
                unmatchedResource.Identifier == source2OnlyNhsNumber
                && unmatchedResource.IsFromSource1 == false);

            actualResourceMatch.Unmatched.Should().Contain(unmatchedResource =>
                unmatchedResource.Identifier == duplicatedNhsNumber
                && unmatchedResource.IsFromSource1
                && unmatchedResource.Resource.GetProperty("id").GetString() == "patient-1-duplicate");

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReportOneUnmatchedResourceWhenDuplicatedNhsNumberHasNoCounterpartAsync()
        {
            // given
            string duplicatedNhsNumber = "9000000009";
            string expectedSource1Id = "patient-1-first";
            string droppedSource1Id = "patient-1-duplicate";

            JsonElement source1FirstPatientResource =
                CreatePatientWithNhsNumber(duplicatedNhsNumber, id: expectedSource1Id);

            JsonElement source1DuplicatePatientResource =
                CreatePatientWithNhsNumber(duplicatedNhsNumber, id: droppedSource1Id);

            var source1Resources = new List<JsonElement>
            {
                source1FirstPatientResource,
                source1DuplicatePatientResource
            };

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

            // Both source1 patients are reported: the one that held the key and found no
            // counterpart, and the duplicate that was dropped from the lookup. Neither may
            // vanish from the comparison.
            actualResourceMatch.Unmatched.Should().HaveCount(2);

            actualResourceMatch.Unmatched.Should().OnlyContain(unmatchedResource =>
                unmatchedResource.Identifier == duplicatedNhsNumber
                    && unmatchedResource.ResourceType == "Patient"
                    && unmatchedResource.IsFromSource1);

            actualResourceMatch.Unmatched.Should().Contain(unmatchedResource =>
                unmatchedResource.Resource.GetProperty("id").GetString() == expectedSource1Id);

            actualResourceMatch.Unmatched.Should().Contain(unmatchedResource =>
                unmatchedResource.Resource.GetProperty("id").GetString() == droppedSource1Id);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
