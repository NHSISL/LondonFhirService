// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.JsonIgnoreRules.Metas
{
    public partial class MetaIgnoreProcessingRuleTests
    {
        [Fact]
        public async Task ShouldReturnTrueOnShouldIgnoreWhenPathEndsWithMetaAsync()
        {
            // given
            JsonElement randomElement = ParseJsonElement("{}");
            JsonElement inputElement = randomElement;
            string inputPath = "patient.meta";
            bool expectedResult = true;

            // when
            bool actualResult =
                await this.metaIgnoreProcessingRule.ShouldIgnoreAsync(inputElement, inputPath);

            // then
            actualResult.Should().Be(expectedResult);
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnTrueOnShouldIgnoreWhenPathIsJustMetaAsync()
        {
            // given
            JsonElement randomElement = ParseJsonElement("{}");
            JsonElement inputElement = randomElement;
            string inputPath = "meta";
            bool expectedResult = true;

            // when
            bool actualResult =
                await this.metaIgnoreProcessingRule.ShouldIgnoreAsync(inputElement, inputPath);

            // then
            actualResult.Should().Be(expectedResult);
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnFalseOnShouldIgnoreWhenPathDoesNotEndWithMetaAsync()
        {
            // given
            JsonElement randomElement = ParseJsonElement("{}");
            JsonElement inputElement = randomElement;
            string inputPath = "patient.name";
            bool expectedResult = false;

            // when
            bool actualResult =
                await this.metaIgnoreProcessingRule.ShouldIgnoreAsync(inputElement, inputPath);

            // then
            actualResult.Should().Be(expectedResult);
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}
