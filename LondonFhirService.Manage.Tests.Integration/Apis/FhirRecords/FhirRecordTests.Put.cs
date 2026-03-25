// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Manage.Tests.Integration.Models.FhirRecords;

namespace LondonFhirService.Manage.Tests.Integration.Apis.FhirRecords
{
    public partial class FhirRecordApiTests
    {
        [Fact]
        public async Task ShouldPutFhirRecordAsync()
        {
            // given
            FhirRecord randomFhirRecord = await PostRandomFhirRecordAsync();
            FhirRecord modifiedFhirRecord = UpdateFhirRecordWithRandomValues(randomFhirRecord);

            // when
            await this.apiBroker.PutFhirRecordAsync(modifiedFhirRecord);
            FhirRecord actualFhirRecord = await this.apiBroker.GetFhirRecordByIdAsync(randomFhirRecord.Id);

            // then
            actualFhirRecord.Should().BeEquivalentTo(
                modifiedFhirRecord,
                options => options
                    .Excluding(fhirRecord => fhirRecord.CreatedBy)
                    .Excluding(fhirRecord => fhirRecord.CreatedDate)
                    .Excluding(fhirRecord => fhirRecord.UpdatedBy)
                    .Excluding(fhirRecord => fhirRecord.UpdatedDate));

            await this.apiBroker.DeleteFhirRecordByIdAsync(actualFhirRecord.Id);
        }
    }
}
