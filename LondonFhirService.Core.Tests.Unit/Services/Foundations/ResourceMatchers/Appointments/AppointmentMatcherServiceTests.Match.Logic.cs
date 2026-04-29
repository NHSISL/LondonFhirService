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
    }
}
