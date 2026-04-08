// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using LondonFhirService.Core.Models.Processings.ListEntryComparisons;
using LondonFhirService.Core.Models.Processings.ListEntryComparisons.Exceptions;
using LondonFhirService.Core.Services.Processings.ListEntryComparisons;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.ListEntryComparisons
{
    public partial class ListEntryComparisonProcessingServiceTests
    {
        [Fact]
        public void ShouldThrowServiceExceptionOnCompareListEntryCountsIfServiceFailureOccurs()
        {
            // given
            JsonElement randomSource1List = new();
            JsonElement randomSource2List = new();
            string randomListTitle = GetRandomString();
            var serviceException = new Exception();

            var failedListEntryComparisonProcessingException =
                new FailedListEntryComparisonProcessingException(
                    message: "Failed list entry comparison processing exception occurred, please contact support",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedListEntryComparisonProcessingServiceException =
                new ListEntryComparisonProcessingServiceException(
                    message: "List entry comparison processing service error occurred, contact support.",
                    innerException: failedListEntryComparisonProcessingException);

            var listEntryComparisonProcessingServiceMock =
                new Mock<ListEntryComparisonProcessingService>(loggingBrokerMock.Object)
            { CallBase = true };

            listEntryComparisonProcessingServiceMock.Setup(service =>
                service.ValidateOnCompareListEntryCounts(
                    randomSource1List,
                    randomSource2List,
                    randomListTitle))
                        .Throws(serviceException);

            // when
            Action compareAction = () =>
                listEntryComparisonProcessingServiceMock.Object.CompareListEntryCounts(
                    randomSource1List,
                    randomSource2List,
                    randomListTitle);

            // then
            ListEntryComparisonProcessingServiceException actualException =
                Assert.Throws<ListEntryComparisonProcessingServiceException>(compareAction);

            actualException.Should()
                .BeEquivalentTo(expectedListEntryComparisonProcessingServiceException);

            listEntryComparisonProcessingServiceMock.Verify(service =>
                service.ValidateOnCompareListEntryCounts(
                    randomSource1List,
                    randomSource2List,
                    randomListTitle),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedListEntryComparisonProcessingServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            listEntryComparisonProcessingServiceMock.VerifyNoOtherCalls();
        }
    }
}
