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
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnShouldIgnoreIfResourceIsInvalidAsync(string invalidText)
        {
            // given
            JsonElement invalidElement = default;
            string invalidPath = invalidText;

            var invalidJsonIgnoreProcessingException =
                new InvalidJsonIgnoreProcessingException(
                    message: "Invalid arguments. Please correct the errors and try again.");

            invalidJsonIgnoreProcessingException.AddData(
                key: "element",
                values: "Json element is undefined.");

            invalidJsonIgnoreProcessingException.UpsertDataList(
                key: "path",
                value: "Text is invalid.");

            var expectedGuidIgnoreProcessingValidationException =
                new GuidIgnoreProcessingValidationException(
                    message:
                        "Guid ignore processing validation error occurred, " +
                        "please fix errors and try again.",
                    innerException: invalidJsonIgnoreProcessingException);

            // when
            ValueTask<bool> shouldIgnoreTask =
                this.guidIgnoreProcessingRule.ShouldIgnoreAsync(
                    invalidElement,
                    invalidPath);

            // then
            GuidIgnoreProcessingValidationException actualException =
                await Assert.ThrowsAsync<GuidIgnoreProcessingValidationException>(
                    shouldIgnoreTask.AsTask);

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