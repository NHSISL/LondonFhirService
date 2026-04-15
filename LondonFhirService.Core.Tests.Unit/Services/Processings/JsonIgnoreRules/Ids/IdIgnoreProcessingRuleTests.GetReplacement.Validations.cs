// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.ArrayOrderIgnoreRules.Exceptions;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.JsonIgnoreRules.Ids
{
    public partial class IdIgnoreProcessingRuleTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnGetReplacementIfElementIsInvalidAsync()
        {
            // given
            JsonElement invalidElement = default;

            var invalidJsonIgnoreProcessingException =
                new InvalidJsonIgnoreRulesProcessingException(
                    message: "Invalid arguments. Please correct the errors and try again.");

            invalidJsonIgnoreProcessingException.AddData(
                key: "element",
                values: "Json element is undefined.");

            var expectedJsonIgnoreRulesProcessingValidationException =
                new JsonIgnoreRulesProcessingValidationException(
                    message:
                        "Id ignore processing validation error occurred, " +
                        "please fix errors and try again.",

                    innerException: invalidJsonIgnoreProcessingException);

            // when
            ValueTask<JsonElement> getReplacementTask =
                this.idIgnoreProcessingRule.GetReplacementAsync(
                    invalidElement);

            // then
            JsonIgnoreRulesProcessingValidationException actualException =
                await Assert.ThrowsAsync<JsonIgnoreRulesProcessingValidationException>(
                    getReplacementTask.AsTask);

            actualException.Should()
                .BeEquivalentTo(expectedJsonIgnoreRulesProcessingValidationException);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedJsonIgnoreRulesProcessingValidationException))),
                       Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}