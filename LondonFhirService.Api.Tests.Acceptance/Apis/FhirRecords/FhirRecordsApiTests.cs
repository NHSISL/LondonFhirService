// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LondonFhirService.Api.Tests.Acceptance.Brokers;
using LondonFhirService.Api.Tests.Acceptance.Models.FhirRecords;
using Tynamix.ObjectFiller;

namespace LondonFhirService.Api.Tests.Acceptance.Apis
{
    [Collection(nameof(ApiTestCollection))]
    public partial class FhirRecordsApiTests
    {
        private readonly ApiBroker apiBroker;

        public FhirRecordsApiTests(ApiBroker apiBroker) =>
            this.apiBroker = apiBroker;

        private static FhirRecord CreateRandomFhirRecord() =>
            CreateRandomFhirRecordFiller().Create();

        private int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTime() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static FhirRecord UpdateFhirRecordWithRandomValues(FhirRecord inputFhirRecord)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            FhirRecord updatedFhirRecord = CreateRandomFhirRecord();
            updatedFhirRecord.Id = inputFhirRecord.Id;
            updatedFhirRecord.CreatedDate = inputFhirRecord.CreatedDate;
            updatedFhirRecord.CreatedBy = inputFhirRecord.CreatedBy;
            updatedFhirRecord.UpdatedDate = now;

            return updatedFhirRecord;
        }

        private async ValueTask<FhirRecord> PostRandomFhirRecordAsync()
        {
            FhirRecord randomFhirRecord = CreateRandomFhirRecord();
            FhirRecord createdFhirRecord = await this.apiBroker.PostFhirRecordAsync(randomFhirRecord);

            return createdFhirRecord;
        }

        private async ValueTask<List<FhirRecord>> PostRandomFhirRecordsAsync()
        {
            int randomNumber = GetRandomNumber();
            var randomFhirRecords = new List<FhirRecord>();

            for (int i = 0; i < randomNumber; i++)
            {
                randomFhirRecords.Add(await PostRandomFhirRecordAsync());
            }

            return randomFhirRecords;
        }

        private static Filler<FhirRecord> CreateRandomFhirRecordFiller()
        {
            string user = Guid.NewGuid().ToString();
            DateTime now = DateTime.UtcNow;
            var filler = new Filler<FhirRecord>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(now)
                .OnType<DateTimeOffset?>().Use(now)
                .OnProperty(fhirRecord => fhirRecord.CreatedBy).Use(user)
                .OnProperty(fhirRecord => fhirRecord.UpdatedBy).Use(user);

            return filler;
        }
    }
}
