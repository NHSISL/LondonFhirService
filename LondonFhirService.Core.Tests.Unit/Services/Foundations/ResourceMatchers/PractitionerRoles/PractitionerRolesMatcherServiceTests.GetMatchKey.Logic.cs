// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.PractitionerRoles
{
    public partial class PractitionerRoleMatcherServiceTests
    {
        [Fact]
        public async Task ShouldGetMatchKeyWhenPractitionerRoleHasSdsRoleProfileIdAsync()
        {
            // given
            string inputSdsRoleProfileId = GetRandomSdsRoleProfileIdValue();
            string randomId = GetRandomString();

            JsonElement practitionerRoleResource = CreatePractitionerRoleResource(
                sdsRoleProfileId: inputSdsRoleProfileId,
                id: randomId);

            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
            string expectedMatchKey = inputSdsRoleProfileId;

            // when
            string actualMatchKey =
                await this.practitionerRoleMatcherService.GetMatchKeyAsync(practitionerRoleResource, resourceIndex);

            // then
            actualMatchKey.Should().Be(expectedMatchKey);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullOnGetMatchKeyWhenPractitionerRoleHasNoIdentifierPropertyAsync()
        {
            // given
            string randomId = GetRandomString();

            JsonElement practitionerRoleResource =
                CreatePractitionerRoleResourceWithoutIdentifierProperty(id: randomId);

            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.practitionerRoleMatcherService.GetMatchKeyAsync(practitionerRoleResource, resourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
