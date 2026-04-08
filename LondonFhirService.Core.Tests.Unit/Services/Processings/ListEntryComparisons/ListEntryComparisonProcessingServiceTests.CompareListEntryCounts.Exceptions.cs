// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
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
        public async Task ShouldThrowServiceExceptionOnCompareListEntryCountsIfServiceFailureOccursAsync()
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
            ValueTask<List<DiffItem>> compareTask =
                listEntryComparisonProcessingServiceMock.Object.CompareListEntryCountsAsync(
                    randomSource1List,
                    randomSource2List,
                    randomListTitle);

            // then
            ListEntryComparisonProcessingServiceException actualException =
                await Assert.ThrowsAsync<ListEntryComparisonProcessingServiceException>(compareTask.AsTask);

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
