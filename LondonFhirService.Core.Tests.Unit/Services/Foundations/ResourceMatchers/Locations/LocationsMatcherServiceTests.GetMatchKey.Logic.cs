// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Locations
{
    public partial class LocationMatcherServiceTests
    {
        [Fact]
        public async Task ShouldGetMatchKeyWhenLocationHasOdsSiteCodeAsync()
        {
            // given
            string inputOdsSiteCode = GetRandomOdsSiteCodeValue();
            string randomId = GetRandomString();

            JsonElement locationResource = CreateLocationResource(
                odsSiteCode: inputOdsSiteCode,
                id: randomId);

            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
            string expectedMatchKey = inputOdsSiteCode;

            // when
            string actualMatchKey =
                await this.locationMatcherService.GetMatchKeyAsync(locationResource, resourceIndex);

            // then
            actualMatchKey.Should().Be(expectedMatchKey);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
