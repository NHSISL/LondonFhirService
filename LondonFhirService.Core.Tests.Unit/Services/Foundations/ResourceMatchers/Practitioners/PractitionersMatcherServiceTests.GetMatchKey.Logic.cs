// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Practitioners
{
    public partial class PractitionerMatcherServiceTests
    {
        [Fact]
        public async Task ShouldGetMatchKeyWhenPractitionerHasSdsUserIdAsync()
        {
            // given
            string inputSdsUserId = GetRandomSdsUserIdValue();
            string randomId = GetRandomString();

            JsonElement practitionerResource = CreatePractitionerResource(
                sdsUserId: inputSdsUserId,
                id: randomId);

            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
            string expectedMatchKey = inputSdsUserId;

            // when
            string actualMatchKey =
                await this.practitionerMatcherService.GetMatchKeyAsync(practitionerResource, resourceIndex);

            // then
            actualMatchKey.Should().Be(expectedMatchKey);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullOnGetMatchKeyWhenPractitionerHasNoIdentifierPropertyAsync()
        {
            // given
            string randomId = GetRandomString();

            JsonElement practitionerResource =
                CreatePractitionerResourceWithoutIdentifierProperty(id: randomId);

            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.practitionerMatcherService.GetMatchKeyAsync(practitionerResource, resourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullOnGetMatchKeyWhenPractitionerHasNonSdsUserIdIdentifierAsync()
        {
            // given
            string randomId = GetRandomString();
            JsonElement practitionerResource = CreateNonSdsPractitionerResource(id: randomId);
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.practitionerMatcherService.GetMatchKeyAsync(practitionerResource, resourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
