// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.GuidIgnoreRules.Exceptions;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.JsonIgnoreRules.Guids
{
    public partial class GuidIgnoreProcessingRuleTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnGetReplacementIfElementIsInvalidAsync()
        {
            // given
            JsonElement invalidElement = default;

            var invalidJsonIgnoreProcessingException =
                new InvalidJsonIgnoreProcessingException(
                    message: "Invalid arguments. Please correct the errors and try again.");

            invalidJsonIgnoreProcessingException.AddData(
                key: "element",
                values: "Json element is undefined.");

            var expectedGuidIgnoreProcessingValidationException =
                new GuidIgnoreProcessingValidationException(
                    message:
                        "Guid ignore processing validation error occurred, " +
                        "please fix errors and try again.",

                    innerException: invalidJsonIgnoreProcessingException);

            // when
            ValueTask<JsonElement> getReplacementTask =
                this.guidIgnoreProcessingRule.GetReplacementAsync(
                    invalidElement);

            // then
            GuidIgnoreProcessingValidationException actualException =
                await Assert.ThrowsAsync<GuidIgnoreProcessingValidationException>(
                    getReplacementTask.AsTask);

            actualException.Should()
                .BeEquivalentTo(expectedGuidIgnoreProcessingValidationException);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedGuidIgnoreProcessingValidationException))),
                       Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}