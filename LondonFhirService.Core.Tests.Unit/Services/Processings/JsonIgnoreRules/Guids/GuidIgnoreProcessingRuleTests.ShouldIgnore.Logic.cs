// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.JsonIgnoreRules.Guids
{
    public partial class GuidIgnoreProcessingRuleTests
    {
        [Fact]
        public async Task ShouldReturnTrueOnShouldIgnoreWhenElementIsGuidStringAsync()
        {
            // given
            JsonElement randomElement = ParseJsonElement("\"a1b2c3d4-e5f6-7890-abcd-ef1234567890\"");
            JsonElement inputElement = randomElement;
            string randomPath = GetRandomString();
            string inputPath = randomPath;
            bool expectedResult = true;

            // when
            bool actualResult =
                await this.guidIgnoreProcessingRule.ShouldIgnoreAsync(inputElement, inputPath);

            // then
            actualResult.Should().Be(expectedResult);
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("42")]
        [InlineData("true")]
        [InlineData("{}")]
        [InlineData("null")]
        [InlineData("[1,2,3]")]
        public async Task ShouldReturnFalseOnShouldIgnoreWhenElementIsNotStringAsync(string nonStringJson)
        {
            // given
            JsonElement randomElement = ParseJsonElement(nonStringJson);
            JsonElement inputElement = randomElement;
            string randomPath = GetRandomString();
            string inputPath = randomPath;
            bool expectedResult = false;

            // when
            bool actualResult =
                await this.guidIgnoreProcessingRule.ShouldIgnoreAsync(inputElement, inputPath);

            // then
            actualResult.Should().Be(expectedResult);
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnFalseOnShouldIgnoreWhenElementIsStringButNotGuidAsync()
        {
            // given
            JsonElement randomElement = ParseJsonElement("\"not-a-guid-value\"");
            JsonElement inputElement = randomElement;
            string randomPath = GetRandomString();
            string inputPath = randomPath;
            bool expectedResult = false;

            // when
            bool actualResult =
                await this.guidIgnoreProcessingRule.ShouldIgnoreAsync(inputElement, inputPath);

            // then
            actualResult.Should().Be(expectedResult);
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}
