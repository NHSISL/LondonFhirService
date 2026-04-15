// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.JsonIgnoreRules.ArrayOrders
{
    public partial class ArrayOrderIgnoreProcessingRuleTests
    {
        [Fact]
        public async Task ShouldReturnTrueOnShouldIgnoreWhenElementIsArrayAsync()
        {
            // given
            JsonElement randomElement = ParseJsonElement("[1,2,3]");
            JsonElement inputElement = randomElement;
            string randomPath = GetRandomString();
            string inputPath = randomPath;
            bool expectedResult = true;

            // when
            bool actualResult =
                await this.arrayOrderIgnoreProcessingRule.ShouldIgnoreAsync(inputElement, inputPath);

            // then
            actualResult.Should().Be(expectedResult);
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("\"someString\"")]
        [InlineData("42")]
        [InlineData("true")]
        [InlineData("{}")]
        [InlineData("null")]
        public async Task ShouldReturnFalseOnShouldIgnoreWhenElementIsNotArrayAsync(string nonArrayJson)
        {
            // given
            JsonElement randomElement = ParseJsonElement(nonArrayJson);
            JsonElement inputElement = randomElement;
            string randomPath = GetRandomString();
            string inputPath = randomPath;
            bool expectedResult = false;

            // when
            bool actualResult =
                await this.arrayOrderIgnoreProcessingRule.ShouldIgnoreAsync(inputElement, inputPath);

            // then
            actualResult.Should().Be(expectedResult);
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}
