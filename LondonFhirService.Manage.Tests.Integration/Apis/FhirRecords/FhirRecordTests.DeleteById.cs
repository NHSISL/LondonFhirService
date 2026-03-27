// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using LondonFhirService.Manage.Tests.Integration.Models.FhirRecords;
using RESTFulSense.Exceptions;

namespace LondonFhirService.Manage.Tests.Integration.Apis.FhirRecords
{
    public partial class FhirRecordApiTests
    {
        [Fact]
        public async Task ShouldDeleteFhirRecordAsync()
        {
            // given
            FhirRecord randomFhirRecord = await PostRandomFhirRecordAsync();

            // when
            await this.apiBroker.DeleteFhirRecordByIdAsync(randomFhirRecord.Id);

            // then
            ValueTask<FhirRecord> getFhirRecordByIdTask =
                this.apiBroker.GetFhirRecordByIdAsync(randomFhirRecord.Id);

            await Assert.ThrowsAsync<HttpResponseNotFoundException>(getFhirRecordByIdTask.AsTask);
        }
    }
}
