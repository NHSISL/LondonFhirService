// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Conditions
{
    public partial class ConditionMatcherServiceTests
    {
        [Fact]
        public async Task ShouldGetMatchKeyWhenConditionHasSnomedCodeAsync()
        {
            // given
            string inputSnomedCode = GetRandomSnomedCode();
            string randomDateTimeOffset = GetRandomDateTimeOffset().ToString();
            string randomId = GetRandomString();

            JsonElement conditionResource = CreateConditionResource(
                snomedCode: inputSnomedCode,
                onsetDateTime: randomDateTimeOffset,
                id : randomId);

            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
            string expectedMatchKey = inputSnomedCode;

            // when
            string actualMatchKey =
                await this.conditionMatcherService.GetMatchKeyAsync(conditionResource, resourceIndex);

            // then
            actualMatchKey.Should().Be(expectedMatchKey);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullOnGetMatchKeyWhenConditionHasNoCodingPropertyAsync()
        {
            // given
            string randomDateTimeOffset = GetRandomDateTimeOffset().ToString();
            string randomId = GetRandomString();

            JsonElement conditionResource =
                CreateConditionResourceWithoutCodingProperty(
                    id: randomId,
                    onsetDateTime: randomDateTimeOffset);

            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.conditionMatcherService.GetMatchKeyAsync(conditionResource, resourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullOnGetMatchKeyWhenConditionHasNonSnomedCodingAsync()
        {
            // given
            string randomDateTimeOffset = GetRandomDateTimeOffset().ToString();
            JsonElement conditionResource = CreateNonSnomedConditionResource(onsetDateTime: randomDateTimeOffset);
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.conditionMatcherService.GetMatchKeyAsync(conditionResource, resourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullOnGetMatchKeyWhenConditionHasNoCodePropertyAsync()
        {
            // given
            string randomDateTimeOffset = GetRandomDateTimeOffset().ToString();
            string randomId = GetRandomString();

            JsonElement conditionResource =
                CreateConditionResourceWithoutCodeProperty(
                    id: randomId,
                    onsetDateTime: randomDateTimeOffset);

            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.conditionMatcherService.GetMatchKeyAsync(conditionResource, resourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        private static JsonElement CreateConditionResourceWithoutCodeProperty(
            string id,
            string onsetDateTime)
        {
            string json = $$"""
                {
                  "resourceType": "Condition",
                  "id": "{{id}}",
                  "onsetDateTime": "{{onsetDateTime}}"
                }
                """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateConditionResourceWithoutCodingProperty(
            string id,
            string onsetDateTime)
        {
            string json = $$"""
                {
                  "resourceType": "Condition",
                  "id": "{{id}}",
                  "code": {},
                  "onsetDateTime": "{{onsetDateTime}}"
                }
                """;

            return ParseJsonElement(json);
        }
    }
}
