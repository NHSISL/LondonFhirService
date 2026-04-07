// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.ArrayOrderIgnoreRules.Exceptions;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.ListEntryComparisons
{
    public partial class ListEntryComparisonProcessingServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnCompareListEntryCountsIfResourceIsInvalidAsync()
        {
            // given
            JsonElement invalidSource1List = default;
            JsonElement invalidSource2List = default;

            var invalidJsonIgnoreProcessingException =
                new InvalidJsonIgnoreProcessingException(
                    message: "Invalid arguments. Please correct the errors and try again.");

            invalidJsonIgnoreProcessingException.AddData(
                key: "source1List",
                values: "Json element is undefined.");

            invalidJsonIgnoreProcessingException.AddData(
                key: "source2List",
                values: "Json element is undefined.");

            invalidJsonIgnoreProcessingException.UpsertDataList(
                key: "path",
                value: "Text is invalid.");

            var expectedArrayOrderIgnoreProcessingValidationException =
                new ArrayOrderIgnoreProcessingValidationException(
                    message:
                        "Array order ignore processing validation error occurred, " +
                        "please fix errors and try again.",
                    innerException: invalidJsonIgnoreProcessingException);

            // when
            ValueTask<bool> shouldIgnoreTask =
                this.listEntryComparisonProcessingService.CompareListEntryCounts(
                    invalidElement,
                    invalidPath);

            // then
            ArrayOrderIgnoreProcessingValidationException actualException =
                await Assert.ThrowsAsync<ArrayOrderIgnoreProcessingValidationException>(
                    shouldIgnoreTask.AsTask);

            actualException.Should()
                .BeEquivalentTo(expectedArrayOrderIgnoreProcessingValidationException);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedArrayOrderIgnoreProcessingValidationException))),
                       Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}