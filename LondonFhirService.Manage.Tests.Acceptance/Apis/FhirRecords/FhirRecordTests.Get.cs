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
        public async Task ShouldGetAllFhirRecordsAsync()
        {
            // given
            List<FhirRecord> randomFhirRecords = await PostRandomFhirRecordsAsync();
            List<FhirRecord> expectedFhirRecords = randomFhirRecords;

            // when
            List<FhirRecord> actualFhirRecords = await this.apiBroker.GetAllFhirRecordsAsync();

            // then
            foreach (FhirRecord expectedFhirRecord in expectedFhirRecords)
            {
                FhirRecord actualFhirRecord = 
                    actualFhirRecords.Single(approval => approval.Id == expectedFhirRecord.Id);

                actualFhirRecord.Should().BeEquivalentTo(expectedFhirRecord, options => options
                    .Excluding(property => property.CreatedBy)
                    .Excluding(property => property.CreatedDate)
                    .Excluding(property => property.UpdatedBy)
                    .Excluding(property => property.UpdatedDate));

                await this.apiBroker.DeleteFhirRecordByIdAsync(actualFhirRecord.Id);
            }
        }
    }
}