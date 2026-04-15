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

            var expectedMetaIgnoreProcessingValidationException =
                new MetaIgnoreProcessingValidationException(
                    message:
                        "Meta ignore processing validation error occurred, " +
                        "please fix errors and try again.",

                    innerException: invalidJsonIgnoreProcessingException);

            // when
            ValueTask<JsonElement> getReplacementTask =
                this.metaIgnoreProcessingRule.GetReplacementAsync(
                    invalidElement);

            // then
            MetaIgnoreProcessingValidationException actualException =
                await Assert.ThrowsAsync<MetaIgnoreProcessingValidationException>(
                    getReplacementTask.AsTask);

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