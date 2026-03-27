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
        public async Task ShouldGetFhirRecordByIdAsync()
        {
            // given
            FhirRecord randomFhirRecord = await PostRandomFhirRecordAsync();
            FhirRecord expectedFhirRecord = randomFhirRecord;

            // when
            FhirRecord actualFhirRecord =
                await this.apiBroker.GetFhirRecordByIdAsync(randomFhirRecord.Id);

            // then
            actualFhirRecord.Should().BeEquivalentTo(expectedFhirRecord);
            await this.apiBroker.DeleteFhirRecordByIdAsync(actualFhirRecord.Id);
        }
    }
}
