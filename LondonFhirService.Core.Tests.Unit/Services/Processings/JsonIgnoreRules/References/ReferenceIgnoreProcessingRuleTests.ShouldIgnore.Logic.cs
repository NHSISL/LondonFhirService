// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.JsonIgnoreRules.References
{
    public partial class ReferenceIgnoreProcessingRuleTests
    {
        [Theory]
        [InlineData("Organization/92f740a2-88ad-4e76-b948-9583b85ecb15")]
        [InlineData("Patient/a1b2c3d4-e5f6-7890-abcd-ef1234567890")]
        [InlineData("Practitioner/A1B2C3D4-E5F6-7890-ABCD-EF1234567890")]
        public async Task ShouldReturnTrueOnShouldIgnoreWhenElementIsGuidReferenceAsync(
            string referenceValue)
        {
            // given
            JsonElement randomElement = ParseJsonElement($"\"{referenceValue}\"");
            JsonElement inputElement = randomElement;
            string inputPath = "$.Patient[9660979622].managingOrganization.reference";
            bool expectedResult = true;

            // when
            bool actualResult =
                await this.referenceIgnoreProcessingRule.ShouldIgnoreAsync(inputElement, inputPath);

            // then
            actualResult.Should().Be(expectedResult);
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnTrueOnShouldIgnoreWhenGuidReferenceSitsInsideAnArrayAsync()
        {
            // given
            JsonElement randomElement =
                ParseJsonElement("\"Organization/92f740a2-88ad-4e76-b948-9583b85ecb15\"");

            JsonElement inputElement = randomElement;
            string inputPath = "$.List[Medication List].entry[3].item.reference";
            bool expectedResult = true;

            // when
            bool actualResult =
                await this.referenceIgnoreProcessingRule.ShouldIgnoreAsync(inputElement, inputPath);

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
            string inputPath = "$.Patient[9660979622].managingOrganization.reference";
            bool expectedResult = false;

            // when
            bool actualResult =
                await this.referenceIgnoreProcessingRule.ShouldIgnoreAsync(inputElement, inputPath);

            // then
            actualResult.Should().Be(expectedResult);
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// A reference whose id is a business identifier rather than a per-request GUID is stable
        /// across providers, so it is still worth comparing - masking it would throw away a real
        /// difference.
        /// </summary>
        [Theory]
        [InlineData("Patient/9660979622")]
        [InlineData("Organization/RRV")]
        [InlineData("92f740a2-88ad-4e76-b948-9583b85ecb15")]
        [InlineData("urn:uuid:92f740a2-88ad-4e76-b948-9583b85ecb15")]
        [InlineData("https://example.invalid/fhir/Organization/92f740a2-88ad-4e76-b948-9583b85ecb15")]
        [InlineData("Organization/92f740a2-88ad-4e76-b948-9583b85ecb1")]
        public async Task ShouldReturnFalseOnShouldIgnoreWhenValueIsNotAGuidReferenceAsync(
            string nonGuidReference)
        {
            // given
            JsonElement randomElement = ParseJsonElement($"\"{nonGuidReference}\"");
            JsonElement inputElement = randomElement;
            string inputPath = "$.Patient[9660979622].managingOrganization.reference";
            bool expectedResult = false;

            // when
            bool actualResult =
                await this.referenceIgnoreProcessingRule.ShouldIgnoreAsync(inputElement, inputPath);

            // then
            actualResult.Should().Be(expectedResult);
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// The value shape alone is not enough: some other string that happens to read like
        /// "Word/&lt;guid&gt;" is not a FHIR reference and is left to compare as itself.
        /// </summary>
        [Theory]
        [InlineData("$.Patient[9660979622].id")]
        [InlineData("$.Observation[54].valueString")]
        [InlineData("$.Patient[9660979622].referenceRange")]
        public async Task ShouldReturnFalseOnShouldIgnoreWhenPathIsNotAReferenceAsync(string nonReferencePath)
        {
            // given
            JsonElement randomElement =
                ParseJsonElement("\"Organization/92f740a2-88ad-4e76-b948-9583b85ecb15\"");

            JsonElement inputElement = randomElement;
            string inputPath = nonReferencePath;
            bool expectedResult = false;

            // when
            bool actualResult =
                await this.referenceIgnoreProcessingRule.ShouldIgnoreAsync(inputElement, inputPath);

            // then
            actualResult.Should().Be(expectedResult);
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}
