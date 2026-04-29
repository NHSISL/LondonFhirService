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
    }
}
