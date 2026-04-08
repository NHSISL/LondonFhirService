// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Text.Json;
using FluentAssertions;
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
        public void ShouldThrowValidationExceptionOnCompareListEntryCountsIfArgumentsAreInvalid(
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
                values: "Json element is undefined.");

            invalidListEntryComparisonProcessingException.AddData(
                key: "source2List",
                values: "Json element is undefined.");

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
            Action compareAction = () =>
                this.listEntryComparisonProcessingService.CompareListEntryCounts(
                    invalidSource1List,
                    invalidSource2List,
                    invalidListTitle);

            // then
            ListEntryComparisonProcessingValidationException actualException =
                Assert.Throws<ListEntryComparisonProcessingValidationException>(compareAction);

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
