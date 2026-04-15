// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.JsonIgnoreRules.Ids
{
    public partial class IdIgnoreProcessingRuleTests
    {
        [Fact]
        public async Task ShouldReturnIdPlaceholderOnGetReplacementAsync()
        {
            // given
            JsonElement randomElement = ParseJsonElement("\"some-id-value\"");
            JsonElement inputElement = randomElement;
            JsonElement returnedElement = ParseJsonElement("\"<id>\"");
            JsonElement expectedElement = returnedElement;

            this.jsonElementServiceMock.Setup(service =>
                service.CreateStringElement("<id>"))
                    .ReturnsAsync(returnedElement);

            // when
            JsonElement actualElement =
                await this.idIgnoreProcessingRule.GetReplacementAsync(inputElement);

            // then
            actualElement.GetRawText().Should().Be(expectedElement.GetRawText());

            this.jsonElementServiceMock.Verify(service =>
                service.CreateStringElement("<id>"),
                    Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}
