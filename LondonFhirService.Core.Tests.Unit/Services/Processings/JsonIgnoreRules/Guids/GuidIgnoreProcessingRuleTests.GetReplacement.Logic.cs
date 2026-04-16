// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.JsonIgnoreRules.Guids
{
    public partial class GuidIgnoreProcessingRuleTests
    {
        [Fact]
        public async Task ShouldReturnGuidPlaceholderOnGetReplacementAsync()
        {
            // given
            JsonElement randomElement = ParseJsonElement("\"a1b2c3d4-e5f6-7890-abcd-ef1234567890\"");
            JsonElement inputElement = randomElement;
            JsonElement returnedElement = ParseJsonElement("\"<GUID>\"");
            JsonElement expectedElement = returnedElement;

            this.jsonElementServiceMock.Setup(service =>
                service.CreateStringElement("<GUID>"))
                    .ReturnsAsync(returnedElement);

            // when
            JsonElement actualElement =
                await this.guidIgnoreProcessingRule.GetReplacementAsync(inputElement);

            // then
            actualElement.GetRawText().Should().Be(expectedElement.GetRawText());

            this.jsonElementServiceMock.Verify(service =>
                service.CreateStringElement("<GUID>"),
                    Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}
