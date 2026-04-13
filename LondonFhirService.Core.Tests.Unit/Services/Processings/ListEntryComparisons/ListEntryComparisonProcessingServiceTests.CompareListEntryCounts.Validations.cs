// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Processings.ListEntryComparisons;
using LondonFhirService.Core.Models.Processings.ListEntryComparisons.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.ListEntryComparisons
{
    public partial class ListEntryComparisonProcessingServiceTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnCompareListEntryCountsIfArgumentsAreInvalidAsync(
            string invalidListTitle)
        {
            // given
            JsonElement invalidSource1List = default;
            JsonElement invalidSource2List = default;

            var invalidListEntryComparisonProcessingException =
                new InvalidListEntryComparisonProcessingException(
                    message:
                        "Invalid list entry comparison arguments. " +
                        "Please correct the errors and try again.");

            invalidListEntryComparisonProcessingException.AddData(
                key: "source1List",
                values: "Json element must be a non-null JSON object.");

            invalidListEntryComparisonProcessingException.AddData(
                key: "source2List",
                values: "Json element must be a non-null JSON object.");

            invalidListEntryComparisonProcessingException.AddData(
                key: "listTitle",
                values: "Text is invalid.");

            var expectedListEntryComparisonProcessingValidationException =
                new ListEntryComparisonProcessingValidationException(
                    message:
                        "List entry comparison processing validation error occurred, " +
                        "please fix errors and try again.",
                    innerException: invalidListEntryComparisonProcessingException);

            // when
            ValueTask<List<DiffItem>> compareTask =
                this.listEntryComparisonProcessingService.CompareListEntryCountsAsync(
                    invalidSource1List,
                    invalidSource2List,
                    invalidListTitle);

            // then
            ListEntryComparisonProcessingValidationException actualException =
                await Assert.ThrowsAsync<ListEntryComparisonProcessingValidationException>(compareTask.AsTask);

            actualException.Should()
                .BeEquivalentTo(expectedListEntryComparisonProcessingValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedListEntryComparisonProcessingValidationException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
