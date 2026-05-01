// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Appointments
{
    public partial class AppointmentMatcherServiceTests
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
                await this.appointmentMatcherService.MatchAsync(
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
        public async Task ShouldReturnMatchedResourceWhenBothSourcesHaveSameDdsIdentifierAsync()
        {
            // given
            string sharedDdsIdentifierValue = GetRandomDdsIdentifierValue();

            JsonElement source1AppointmentResource =
                CreateAppointmentWithDdsIdentifier(sharedDdsIdentifierValue, id: "appointment-1");

            JsonElement source2AppointmentResource =
                CreateAppointmentWithDdsIdentifier(sharedDdsIdentifierValue, id: "appointment-2");

            var source1Resources = new List<JsonElement> { source1AppointmentResource };
            var source2Resources = new List<JsonElement> { source2AppointmentResource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            // when
            ResourceMatch actualResourceMatch =
                await this.appointmentMatcherService.MatchAsync(
                    source1Resources,
                    source2Resources,
                    source1ResourceIndex,
                    source2ResourceIndex);

            // then
            actualResourceMatch.Matched.Should().HaveCount(1);
            actualResourceMatch.Matched[0].MatchKey.Should().Be(sharedDdsIdentifierValue);
            actualResourceMatch.Unmatched.Should().BeEmpty();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnUnmatchedResourceFromSource1WhenOnlySource1HasAppointmentAsync()
        {
            // given
            string ddsIdentifierValue = GetRandomDdsIdentifierValue();

            JsonElement source1AppointmentResource =
                CreateAppointmentWithDdsIdentifier(ddsIdentifierValue);

            var source1Resources = new List<JsonElement> { source1AppointmentResource };
            var source2Resources = new List<JsonElement>();
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            // when
            ResourceMatch actualResourceMatch =
                await this.appointmentMatcherService.MatchAsync(
                    source1Resources,
                    source2Resources,
                    source1ResourceIndex,
                    source2ResourceIndex);

            // then
            actualResourceMatch.Matched.Should().BeEmpty();
            actualResourceMatch.Unmatched.Should().HaveCount(1);
            actualResourceMatch.Unmatched[0].Identifier.Should().Be(ddsIdentifierValue);
            actualResourceMatch.Unmatched[0].IsFromSource1.Should().BeTrue();
            actualResourceMatch.Unmatched[0].ResourceType.Should().Be("Appointment");
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnUnmatchedResourceFromSource2WhenOnlySource2HasAppointmentAsync()
        {
            // given
            string ddsIdentifierValue = GetRandomDdsIdentifierValue();

            JsonElement source2AppointmentResource =
                CreateAppointmentWithDdsIdentifier(ddsIdentifierValue);

            var source1Resources = new List<JsonElement>();
            var source2Resources = new List<JsonElement> { source2AppointmentResource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            // when
            ResourceMatch actualResourceMatch =
                await this.appointmentMatcherService.MatchAsync(
                    source1Resources,
                    source2Resources,
                    source1ResourceIndex,
                    source2ResourceIndex);

            // then
            actualResourceMatch.Matched.Should().BeEmpty();
            actualResourceMatch.Unmatched.Should().HaveCount(1);
            actualResourceMatch.Unmatched[0].Identifier.Should().Be(ddsIdentifierValue);
            actualResourceMatch.Unmatched[0].IsFromSource1.Should().BeFalse();
            actualResourceMatch.Unmatched[0].ResourceType.Should().Be("Appointment");
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldExcludeAppointmentWithoutDdsIdentifierFromMatchResultsAsync()
        {
            // given
            JsonElement source1AppointmentResource = CreateAppointmentWithoutIdentifier(id: "appointment-1");
            JsonElement source2AppointmentResource = CreateAppointmentWithoutIdentifier(id: "appointment-2");
            var source1Resources = new List<JsonElement> { source1AppointmentResource };
            var source2Resources = new List<JsonElement> { source2AppointmentResource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            // when
            ResourceMatch actualResourceMatch =
                await this.appointmentMatcherService.MatchAsync(
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
        public async Task ShouldReturnMixedMatchedAndUnmatchedAppointmentsAsync()
        {
            // given
            string sharedDdsIdentifierValue = GetRandomDdsIdentifierValue();
            string source1OnlyDdsIdentifierValue = GetRandomDdsIdentifierValue();
            string source2OnlyDdsIdentifierValue = GetRandomDdsIdentifierValue();

            JsonElement source1MatchedAppointment =
                CreateAppointmentWithDdsIdentifier(sharedDdsIdentifierValue, id: "appointment-1a");

            JsonElement source1UnmatchedAppointment =
                CreateAppointmentWithDdsIdentifier(source1OnlyDdsIdentifierValue, id: "appointment-1b");

            JsonElement source2MatchedAppointment =
                CreateAppointmentWithDdsIdentifier(sharedDdsIdentifierValue, id: "appointment-2a");

            JsonElement source2UnmatchedAppointment =
                CreateAppointmentWithDdsIdentifier(source2OnlyDdsIdentifierValue, id: "appointment-2b");

            var source1Resources = new List<JsonElement>
            {
                source1MatchedAppointment,
                source1UnmatchedAppointment
            };

            var source2Resources = new List<JsonElement>
            {
                source2MatchedAppointment,
                source2UnmatchedAppointment
            };

            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            // when
            ResourceMatch actualResourceMatch =
                await this.appointmentMatcherService.MatchAsync(
                    source1Resources,
                    source2Resources,
                    source1ResourceIndex,
                    source2ResourceIndex);

            // then
            actualResourceMatch.Matched.Should().HaveCount(1);
            actualResourceMatch.Matched[0].MatchKey.Should().Be(sharedDdsIdentifierValue);
            actualResourceMatch.Unmatched.Should().HaveCount(2);

            actualResourceMatch.Unmatched.Should().Contain(unmatchedResource =>
                unmatchedResource.Identifier == source1OnlyDdsIdentifierValue
                && unmatchedResource.IsFromSource1 == true);

            actualResourceMatch.Unmatched.Should().Contain(unmatchedResource =>
                unmatchedResource.Identifier == source2OnlyDdsIdentifierValue
                && unmatchedResource.IsFromSource1 == false);

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldMatchComprehensiveAppointmentsWithMultipleIdentifierSystemsAsync()
        {
            // given
            string inputDdsIdentifierValue = "APP-comprehensive-1";

            JsonElement source1Resource = CreateComprehensiveAppointmentResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: "appointment-comprehensive-1");

            JsonElement source2Resource = CreateComprehensiveAppointmentResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: "appointment-comprehensive-2");

            var source1Resources = new List<JsonElement> { source1Resource };
            var source2Resources = new List<JsonElement> { source2Resource };
            Dictionary<string, JsonElement> source1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> source2ResourceIndex = CreateResourceIndex();

            var expectedResourceMatch = new ResourceMatch();

            expectedResourceMatch.Matched.Add(
                new MatchedResource(source1Resource, source2Resource, inputDdsIdentifierValue));

            // when
            ResourceMatch actualResourceMatch = await this.appointmentMatcherService.MatchAsync(
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
