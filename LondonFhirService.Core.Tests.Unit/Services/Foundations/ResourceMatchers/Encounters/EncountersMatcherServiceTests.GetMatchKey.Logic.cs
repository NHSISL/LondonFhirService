// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Encounters
{
    public partial class EncounterMatcherServiceTests
    {
        [Fact]
        public async Task ShouldGetMatchKeyWhenEncounterHasDdsIdentifierAsync()
        {
            // given
            string inputDdsIdentifierValue = GetRandomDdsIdentifierValue();
            string randomId = GetRandomString();

            JsonElement encounterResource = CreateEncounterResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: randomId);

            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
            string expectedMatchKey = inputDdsIdentifierValue;

            // when
            string actualMatchKey =
                await this.encounterMatcherService.GetMatchKeyAsync(encounterResource, resourceIndex);

            // then
            actualMatchKey.Should().Be(expectedMatchKey);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
