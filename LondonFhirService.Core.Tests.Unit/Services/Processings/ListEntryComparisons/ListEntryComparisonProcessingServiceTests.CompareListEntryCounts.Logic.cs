// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using LondonFhirService.Core.Models.Processings.ListEntryComparisons;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.ListEntryComparisons
{
    public partial class ListEntryComparisonProcessingServiceTests
    {
        [Fact]
        public void ShouldReturnEmptyDiffsOnCompareListEntryCountsWhenCountsMatch()
        {
            // given
            int randomEntryCount = GetRandomNumber();
            string randomListTitle = GetRandomString();
            JsonElement source1List = CreateListElementWithEntries(randomEntryCount);
            JsonElement source2List = CreateListElementWithEntries(randomEntryCount);
            var expectedDiffs = new List<DiffItem>();

            // when
            List<DiffItem> actualDiffs =
                this.listEntryComparisonProcessingService.CompareListEntryCounts(
                    source1List,
                    source2List,
                    randomListTitle);

            // then
            actualDiffs.Should().BeEquivalentTo(expectedDiffs);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void ShouldReturnDiffItemOnCompareListEntryCountsWhenCountsDoNotMatch()
        {
            // given
            int source1EntryCount = GetRandomNumber();
            int source2EntryCount = source1EntryCount + GetRandomNumber();
            string randomListTitle = GetRandomString();
            JsonElement source1List = CreateListElementWithEntries(source1EntryCount);
            JsonElement source2List = CreateListElementWithEntries(source2EntryCount);

            var expectedDiff = new DiffItem
            {
                Type = "entry-count-mismatch",
                Path = $"$.List[{randomListTitle}].entry",
                ResourceType = "List",
                Identifier = randomListTitle,
                OldValue = source1EntryCount.ToString(),
                NewValue = source2EntryCount.ToString(),

                Reason =
                    $"List '{randomListTitle}' has {source1EntryCount} entries in Source1 " +
                    $"but {source2EntryCount} entries in Source2"
            };

            var expectedDiffs = new List<DiffItem> { expectedDiff };

            // when
            List<DiffItem> actualDiffs =
                this.listEntryComparisonProcessingService.CompareListEntryCounts(
                    source1List,
                    source2List,
                    randomListTitle);

            // then
            actualDiffs.Should().BeEquivalentTo(expectedDiffs);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
