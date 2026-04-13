// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.AllergyIntolerances
{
    public partial class AllergyIntoleranceMatcherServiceTests
    {
        [Fact]
        public async Task ShouldReturnMatchKeyAsync()
        {
            // given
            string randomSnomedCode = GetRandomSnomedCode();
            string randomOnsetDateTime = GetRandomDateString();
            string expectedMatchKey = $"{randomSnomedCode}|{randomOnsetDateTime}";
            JsonElement randomResource = CreateAllergyIntoleranceResource(randomSnomedCode, randomOnsetDateTime);
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.allergyIntoleranceMatcherService.GetMatchKeyAsync(
                    randomResource,
                    resourceIndex);

            // then
            actualMatchKey.Should().Be(expectedMatchKey);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullKeyWhenResourceHasNoSnomedCodingAsync()
        {
            // given
            string randomOnsetDateTime = GetRandomDateString();
            JsonElement resource = CreateNonSnomedAllergyIntoleranceResource(randomOnsetDateTime);
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.allergyIntoleranceMatcherService.GetMatchKeyAsync(
                    resource,
                    resourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullKeyWhenResourceHasNoOnsetDateTimeAsync()
        {
            // given
            string randomSnomedCode = GetRandomSnomedCode();
            JsonElement resource = CreateResourceWithoutOnsetDateTime(randomSnomedCode);
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.allergyIntoleranceMatcherService.GetMatchKeyAsync(
                    resource,
                    resourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
