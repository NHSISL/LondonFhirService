// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LondonFhirService.Api.Tests.Acceptance.Models.FhirRecords;

namespace LondonFhirService.Api.Tests.Acceptance.Brokers
{
    public partial class ApiBroker
    {
        private const string fhirRecordsRelativeUrl = "api/fhirrecords";

        public async ValueTask<FhirRecord> PostFhirRecordAsync(FhirRecord fhirRecord) =>
            await this.apiFactoryClient.PostContentAsync(fhirRecordsRelativeUrl, fhirRecord);

        public async ValueTask<List<FhirRecord>> GetAllFhirRecordsAsync() =>
            await this.apiFactoryClient
                .GetContentAsync<List<FhirRecord>>($"{fhirRecordsRelativeUrl}/");

        public async ValueTask<List<FhirRecord>> GetSpecificFhirRecordByIdAsync(Guid fhirRecordId) =>
            await this.apiFactoryClient.GetContentAsync<List<FhirRecord>>(
                $"{fhirRecordsRelativeUrl}?$filter=Id eq {fhirRecordId}");

        public async ValueTask<FhirRecord> GetFhirRecordByIdAsync(Guid fhirRecordId) =>
            await this.apiFactoryClient
                .GetContentAsync<FhirRecord>($"{fhirRecordsRelativeUrl}/{fhirRecordId}");

        public async ValueTask<FhirRecord> PutFhirRecordAsync(FhirRecord fhirRecord) =>
            await this.apiFactoryClient.PutContentAsync(fhirRecordsRelativeUrl, fhirRecord);

        public async ValueTask<FhirRecord> DeleteFhirRecordByIdAsync(Guid fhirRecordId) =>
            await this.apiFactoryClient
                .DeleteContentAsync<FhirRecord>($"{fhirRecordsRelativeUrl}/{fhirRecordId}");
    }
}
