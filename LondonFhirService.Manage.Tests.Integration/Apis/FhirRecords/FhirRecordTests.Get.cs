// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Manage.Tests.Integration.Models.FhirRecords;

namespace LondonFhirService.Manage.Tests.Integration.Apis.FhirRecords
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
            actualFhirRecords.Should().NotBeNull();

            foreach (FhirRecord expectedFhirRecord in expectedFhirRecords)
            {
                FhirRecord actualFhirRecord = actualFhirRecords
                    .Single(fhirRecord => fhirRecord.Id == expectedFhirRecord.Id);

                actualFhirRecord.Should().BeEquivalentTo(
                    expectedFhirRecord,
                    options => options
                        .Excluding(property => property.CreatedBy)
                        .Excluding(property => property.CreatedDate)
                        .Excluding(property => property.UpdatedBy)
                        .Excluding(property => property.UpdatedDate));
            }

            // cleanup
            foreach (FhirRecord createdFhirRecord in expectedFhirRecords)
            {
                await this.apiBroker.DeleteFhirRecordByIdAsync(createdFhirRecord.Id);
            }
        }
    }
}
