// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.JsonIgnoreRules.Metas
{
    public partial class MetaIgnoreProcessingRuleTests
    {
        [Fact]
        public async Task ShouldReturnMetaIgnoredPlaceholderOnGetReplacementAsync()
        {
            // given
            JsonElement randomElement = ParseJsonElement("{}");
            JsonElement inputElement = randomElement;
            JsonElement returnedElement = ParseJsonElement("\"<meta-ignored>\"");
            JsonElement expectedElement = returnedElement;

            this.jsonElementServiceMock.Setup(service =>
                service.CreateStringElement("<meta-ignored>"))
                    .ReturnsAsync(returnedElement);

            // when
            JsonElement actualElement =
                await this.metaIgnoreProcessingRule.GetReplacementAsync(inputElement);

            // then
            actualElement.GetRawText().Should().Be(expectedElement.GetRawText());

            this.jsonElementServiceMock.Verify(service =>
                service.CreateStringElement("<meta-ignored>"),
                    Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}
