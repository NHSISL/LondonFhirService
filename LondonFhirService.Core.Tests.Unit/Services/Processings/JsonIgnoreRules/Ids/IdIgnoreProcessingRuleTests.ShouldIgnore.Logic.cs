// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.JsonIgnoreRules.Ids
{
    public partial class IdIgnoreProcessingRuleTests
    {
        [Fact]
        public async Task ShouldReturnTrueOnShouldIgnoreWhenElementIsStringAndPathEndsWithIdAsync()
        {
            // given
            JsonElement randomElement = ParseJsonElement("\"some-id-value\"");
            JsonElement inputElement = randomElement;
            string inputPath = "patient.id";
            bool expectedResult = true;

            // when
            bool actualResult =
                await this.idIgnoreProcessingRule.ShouldIgnoreAsync(inputElement, inputPath);

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
            string inputPath = "patient.id";
            bool expectedResult = false;

            // when
            bool actualResult =
                await this.idIgnoreProcessingRule.ShouldIgnoreAsync(inputElement, inputPath);

            // then
            actualResult.Should().Be(expectedResult);
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnFalseOnShouldIgnoreWhenElementIsStringButPathDoesNotEndWithIdAsync()
        {
            // given
            JsonElement randomElement = ParseJsonElement("\"some-value\"");
            JsonElement inputElement = randomElement;
            string inputPath = "patient.name";
            bool expectedResult = false;

            // when
            bool actualResult =
                await this.idIgnoreProcessingRule.ShouldIgnoreAsync(inputElement, inputPath);

            // then
            actualResult.Should().Be(expectedResult);
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}
