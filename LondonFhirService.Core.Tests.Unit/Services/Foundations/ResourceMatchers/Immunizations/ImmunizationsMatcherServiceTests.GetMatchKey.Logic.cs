// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Immunizations
{
    public partial class ImmunizationMatcherServiceTests
    {
        [Fact]
        public async Task ShouldGetMatchKeyWhenImmunizationHasDdsIdentifierAsync()
        {
            // given
            string inputDdsIdentifierValue = GetRandomDdsIdentifierValue();
            string randomId = GetRandomString();

            JsonElement immunizationResource = CreateImmunizationResource(
                ddsIdentifierValue: inputDdsIdentifierValue,
                id: randomId);

            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
            string expectedMatchKey = inputDdsIdentifierValue;

            // when
            string actualMatchKey =
                await this.immunizationMatcherService.GetMatchKeyAsync(immunizationResource, resourceIndex);

            // then
            actualMatchKey.Should().Be(expectedMatchKey);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullOnGetMatchKeyWhenImmunizationHasNoIdentifierPropertyAsync()
        {
            // given
            string randomId = GetRandomString();

            JsonElement immunizationResource =
                CreateImmunizationResourceWithoutIdentifierProperty(id: randomId);

            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.immunizationMatcherService.GetMatchKeyAsync(immunizationResource, resourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullOnGetMatchKeyWhenImmunizationHasNonDdsIdentifierAsync()
        {
            // given
            string randomId = GetRandomString();
            JsonElement immunizationResource = CreateNonDdsImmunizationResource(id: randomId);
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.immunizationMatcherService.GetMatchKeyAsync(immunizationResource, resourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
