// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.JsonIgnoreRules.ArrayOrders
{
    public partial class ArrayOrderIgnoreProcessingRuleTests
    {
        [Fact]
        public async Task ShouldReturnElementUnchangedOnGetReplacementWhenElementIsNotArrayAsync()
        {
            // given
            JsonElement randomElement = ParseJsonElement("\"someString\"");
            JsonElement inputElement = randomElement;
            JsonElement expectedElement = inputElement;

            // when
            JsonElement actualElement =
                await this.arrayOrderIgnoreProcessingRule.GetReplacementAsync(inputElement);

            // then
            actualElement.GetRawText().Should().Be(expectedElement.GetRawText());
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldSortArrayItemsByRawTextOnGetReplacementAsync()
        {
            // given
            JsonElement randomElement = ParseJsonElement("[\"banana\",\"apple\"]");
            JsonElement inputElement = randomElement;
            JsonElement returnedElement = ParseJsonElement("[\"apple\",\"banana\"]");
            JsonElement expectedElement = returnedElement;

            this.jsonElementServiceMock.Setup(service => service.CreateArrayElement(
                It.Is<List<JsonElement>>(elements =>
                    elements.Count == 2
                    && elements[0].GetRawText() == "\"apple\""
                    && elements[1].GetRawText() == "\"banana\"")))
                        .ReturnsAsync(returnedElement);

            // when
            JsonElement actualElement =
                await this.arrayOrderIgnoreProcessingRule.GetReplacementAsync(inputElement);

            // then
            actualElement.GetRawText().Should().Be(expectedElement.GetRawText());

            this.jsonElementServiceMock.Verify(service => service.CreateArrayElement(
                It.Is<List<JsonElement>>(elements =>
                    elements.Count == 2
                    && elements[0].GetRawText() == "\"apple\""
                    && elements[1].GetRawText() == "\"banana\"")),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldSortObjectItemsByResourceTypeOnGetReplacementAsync()
        {
            // given
            JsonElement randomElement = ParseJsonElement(
                "[{\"resourceType\":\"Patient\",\"id\":\"1\"}," +
                "{\"resourceType\":\"AllergyIntolerance\",\"id\":\"2\"}]");

            JsonElement inputElement = randomElement;
            JsonElement sortedPatient = ParseJsonElement("{\"id\":\"1\",\"resourceType\":\"Patient\"}");
            JsonElement sortedAllergyIntolerance = ParseJsonElement("{\"id\":\"2\",\"resourceType\":\"AllergyIntolerance\"}");

            JsonElement sortedArray = ParseJsonElement(
                "[{\"id\":\"2\",\"resourceType\":\"AllergyIntolerance\"}," +
                "{\"id\":\"1\",\"resourceType\":\"Patient\"}]");

            JsonElement expectedElement = sortedArray;

            this.jsonElementServiceMock.Setup(service => service.CreateObjectElement(
                It.Is<Dictionary<string, JsonElement>>(properties =>
                    properties.ContainsKey("resourceType")
                    && properties["resourceType"].GetString() == "Patient")))
                        .ReturnsAsync(sortedPatient);

            this.jsonElementServiceMock.Setup(service => service.CreateObjectElement(
                It.Is<Dictionary<string, JsonElement>>(properties =>
                    properties.ContainsKey("resourceType")
                    && properties["resourceType"].GetString() == "AllergyIntolerance")))
                        .ReturnsAsync(sortedAllergyIntolerance);

            this.jsonElementServiceMock.Setup(service => service.CreateArrayElement(
                It.Is<List<JsonElement>>(elements =>
                    elements.Count == 2
                    && elements[0].GetProperty("resourceType").GetString() == "AllergyIntolerance"
                    && elements[1].GetProperty("resourceType").GetString() == "Patient")))
                        .ReturnsAsync(sortedArray);

            // when
            JsonElement actualElement =
                await this.arrayOrderIgnoreProcessingRule.GetReplacementAsync(inputElement);

            // then
            actualElement.GetRawText().Should().Be(expectedElement.GetRawText());

            this.jsonElementServiceMock.Verify(service => service.CreateObjectElement(
                It.Is<Dictionary<string, JsonElement>>(properties =>
                    properties.ContainsKey("resourceType")
                    && properties["resourceType"].GetString() == "Patient")),
                        Times.Once);

            this.jsonElementServiceMock.Verify(service => service.CreateObjectElement(
                It.Is<Dictionary<string, JsonElement>>(properties =>
                    properties.ContainsKey("resourceType")
                    && properties["resourceType"].GetString() == "AllergyIntolerance")),
                        Times.Once);

            this.jsonElementServiceMock.Verify(service => service.CreateArrayElement(
                It.Is<List<JsonElement>>(elements =>
                    elements.Count == 2
                    && elements[0].GetProperty("resourceType").GetString() == "AllergyIntolerance"
                    && elements[1].GetProperty("resourceType").GetString() == "Patient")),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}
