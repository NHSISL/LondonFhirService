// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.MetaIgnoreRules.Exceptions;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.JsonIgnoreRules.Metas
{
    public partial class MetaIgnoreProcessingRuleTests
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

            var expectedMetaIgnoreProcessingValidationException =
                new MetaIgnoreProcessingValidationException(
                    message:
                        "Meta ignore processing validation error occurred, " +
                        "please fix errors and try again.",
                    innerException: invalidJsonIgnoreProcessingException);

            // when
            ValueTask<bool> shouldIgnoreTask =
                this.metaIgnoreProcessingRule.ShouldIgnoreAsync(
                    invalidElement,
                    invalidPath);

            // then
            MetaIgnoreProcessingValidationException actualException =
                await Assert.ThrowsAsync<MetaIgnoreProcessingValidationException>(
                    shouldIgnoreTask.AsTask);

            actualException.Should()
                .BeEquivalentTo(expectedMetaIgnoreProcessingValidationException);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedMetaIgnoreProcessingValidationException))),
                       Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}