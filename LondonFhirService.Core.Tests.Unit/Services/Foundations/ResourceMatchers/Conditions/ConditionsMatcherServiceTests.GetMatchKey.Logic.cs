// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

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
            string inputSnomedCode = "444814009";

            JsonElement conditionResource = CreateConditionResource(
                snomedCode: inputSnomedCode,
                onsetDateTime: "2024-01-01");

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
            JsonElement conditionResource = CreateConditionResourceWithoutCodingProperty();
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
            JsonElement conditionResource = CreateNonSnomedConditionResource(onsetDateTime: "2024-01-01");
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
            JsonElement conditionResource = CreateConditionResourceWithoutCodeProperty();
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();

            // when
            string actualMatchKey =
                await this.conditionMatcherService.GetMatchKeyAsync(conditionResource, resourceIndex);

            // then
            actualMatchKey.Should().BeNull();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        private static JsonElement CreateConditionResourceWithoutCodeProperty()
        {
            string json = """
                {
                  "resourceType": "Condition",
                  "id": "condition-no-code",
                  "onsetDateTime": "2024-01-01"
                }
                """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateConditionResourceWithoutCodingProperty()
        {
            string json = """
                {
                  "resourceType": "Condition",
                  "id": "condition-no-coding",
                  "code": {},
                  "onsetDateTime": "2024-01-01"
                }
                """;

            return ParseJsonElement(json);
        }
    }
}
