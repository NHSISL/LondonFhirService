// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Organizations
{
    public partial class OrganizationMatcherServiceTests
    {
        [Fact]
        public async Task ShouldGetMatchKeyWhenOrganizationHasOdsOrganizationCodeAsync()
        {
            // given
            string inputOdsOrganizationCode = GetRandomOdsOrganizationCodeValue();
            string randomId = GetRandomString();

            JsonElement organizationResource = CreateOrganizationResource(
                odsOrganizationCode: inputOdsOrganizationCode,
                id: randomId);

            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
            string expectedMatchKey = inputOdsOrganizationCode;

            // when
            string actualMatchKey =
                await this.organizationMatcherService.GetMatchKeyAsync(organizationResource, resourceIndex);

            // then
            actualMatchKey.Should().Be(expectedMatchKey);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullOnGetMatchKeyWhenOrganizationHasNoIdentifierPropertyAsync()
        {
            // given
            string randomId = GetRandomString();

            JsonElement organizationResource =
                CreateOrganizationResourceWithoutIdentifierProperty(id: randomId);

            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.organizationMatcherService.GetMatchKeyAsync(organizationResource, resourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullOnGetMatchKeyWhenOrganizationHasNonOdsOrganizationCodeIdentifierAsync()
        {
            // given
            string randomId = GetRandomString();
            JsonElement organizationResource = CreateNonOdsOrganizationResource(id: randomId);
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.organizationMatcherService.GetMatchKeyAsync(organizationResource, resourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
