// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Manage.Tests.Acceptance.Models.FhirRecords;

namespace LondonFhirService.Manage.Tests.Acceptance.Apis.FhirRecords
{
    public partial class FhirRecordApiTests
    {
        [Fact]
        public async Task ShouldDeleteFhirRecordByIdAsync()
        {
            // given
            FhirRecord randomFhirRecord = await PostRandomFhirRecordAsync();
            FhirRecord inputFhirRecord = randomFhirRecord;
            FhirRecord expectedFhirRecord = inputFhirRecord;

            // when
            FhirRecord deletedFhirRecord =
                await this.apiBroker.DeleteFhirRecordByIdAsync(inputFhirRecord.Id);

            List<FhirRecord> actualResult =
                await this.apiBroker.GetSpecificFhirRecordByIdAsync(inputFhirRecord.Id);

            // then
            actualResult.Count().Should().Be(0);
        }
    }
}