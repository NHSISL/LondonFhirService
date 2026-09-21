// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.JsonIgnoreRules.References
{
    public partial class ReferenceIgnoreProcessingRuleTests
    {
        [Theory]
        [InlineData("Organization/92f740a2-88ad-4e76-b948-9583b85ecb15", "Organization")]
        [InlineData("Patient/a1b2c3d4-e5f6-7890-abcd-ef1234567890", "Patient")]
        [InlineData("Practitioner/A1B2C3D4-E5F6-7890-ABCD-EF1234567890", "Practitioner")]
        public async Task ShouldKeepResourceTypeAndMaskGuidOnGetReplacementAsync(
            string referenceValue,
            string expectedResourceType)
        {
            // given
            JsonElement randomElement = ParseJsonElement($"\"{referenceValue}\"");
            JsonElement inputElement = randomElement;
            string expectedReplacementText = $"{expectedResourceType}/<GUID>";
            JsonElement returnedElement = ParseJsonElement($"\"{expectedReplacementText}\"");
            JsonElement expectedElement = returnedElement;

            this.jsonElementServiceMock.Setup(service =>
                service.CreateStringElement(expectedReplacementText))
                    .ReturnsAsync(returnedElement);

            // when
            JsonElement actualElement =
                await this.referenceIgnoreProcessingRule.GetReplacementAsync(inputElement);

            // then
            actualElement.GetRawText().Should().Be(expectedElement.GetRawText());

            this.jsonElementServiceMock.Verify(service =>
                service.CreateStringElement(expectedReplacementText),
                    Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Two references that differ only by their GUIDs normalise to the same text, which is the
        /// whole point of the rule; two that point at different resource types do not, which is
        /// what stops it hiding a real difference.
        /// </summary>
        [Fact]
        public async Task ShouldNormaliseDifferentGuidsOfSameTypeToSameValueOnGetReplacementAsync()
        {
            // given
            JsonElement source1Element =
                ParseJsonElement("\"Organization/92f740a2-88ad-4e76-b948-9583b85ecb15\"");

            JsonElement source2Element =
                ParseJsonElement("\"Organization/ffa73264-d1e2-4c3b-9a8f-1b2c3d4e5f60\"");

            JsonElement otherTypeElement =
                ParseJsonElement("\"Practitioner/ffa73264-d1e2-4c3b-9a8f-1b2c3d4e5f60\"");

            this.jsonElementServiceMock.Setup(service =>
                service.CreateStringElement("Organization/<GUID>"))
                    .ReturnsAsync(ParseJsonElement("\"Organization/<GUID>\""));

            this.jsonElementServiceMock.Setup(service =>
                service.CreateStringElement("Practitioner/<GUID>"))
                    .ReturnsAsync(ParseJsonElement("\"Practitioner/<GUID>\""));

            // when
            JsonElement source1Replacement =
                await this.referenceIgnoreProcessingRule.GetReplacementAsync(source1Element);

            JsonElement source2Replacement =
                await this.referenceIgnoreProcessingRule.GetReplacementAsync(source2Element);

            JsonElement otherTypeReplacement =
                await this.referenceIgnoreProcessingRule.GetReplacementAsync(otherTypeElement);

            // then
            source1Replacement.GetRawText().Should().Be(source2Replacement.GetRawText());
            otherTypeReplacement.GetRawText().Should().NotBe(source1Replacement.GetRawText());

            this.jsonElementServiceMock.Verify(service =>
                service.CreateStringElement("Organization/<GUID>"),
                    Times.Exactly(2));

            this.jsonElementServiceMock.Verify(service =>
                service.CreateStringElement("Practitioner/<GUID>"),
                    Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// GetReplacementAsync is public, so it can be handed something ShouldIgnoreAsync would
        /// have rejected. It gives the element back rather than replacing a real value with a
        /// placeholder.
        /// </summary>
        [Theory]
        [InlineData("\"Patient/9660979622\"")]
        [InlineData("\"not-a-reference\"")]
        [InlineData("42")]
        [InlineData("{}")]
        public async Task ShouldReturnElementUnchangedOnGetReplacementWhenNotAGuidReferenceAsync(
            string nonReferenceJson)
        {
            // given
            JsonElement randomElement = ParseJsonElement(nonReferenceJson);
            JsonElement inputElement = randomElement;
            JsonElement expectedElement = randomElement;

            // when
            JsonElement actualElement =
                await this.referenceIgnoreProcessingRule.GetReplacementAsync(inputElement);

            // then
            actualElement.GetRawText().Should().Be(expectedElement.GetRawText());
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}
