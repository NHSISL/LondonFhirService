// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Manage.Tests.Acceptance.Models.FhirRecords;

namespace LondonFhirService.Manage.Tests.Acceptance.Apis.FhirRecords
{
    public partial class FhirRecordApiTests
    {
        [Fact]
        public async Task ShouldGetFhirRecordByIdAsync()
        {
            // given
            FhirRecord randomFhirRecord = await PostRandomFhirRecordAsync();
            FhirRecord expectedFhirRecord = randomFhirRecord;

            // when
            FhirRecord actualFhirRecord =
                await this.apiBroker.GetFhirRecordByIdAsync(randomFhirRecord.Id);

            // then
            actualFhirRecord.Should().BeEquivalentTo(expectedFhirRecord, options => options
                .Excluding(property => property.CreatedBy)
                .Excluding(property => property.CreatedDate)
                .Excluding(property => property.UpdatedBy)
                .Excluding(property => property.UpdatedDate));

            await this.apiBroker.DeleteFhirRecordByIdAsync(actualFhirRecord.Id);
        }
    }
}