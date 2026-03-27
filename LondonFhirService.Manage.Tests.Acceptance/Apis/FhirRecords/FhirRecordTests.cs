// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LondonFhirService.Manage.Tests.Acceptance.Brokers;
using LondonFhirService.Manage.Tests.Acceptance.Models.FhirRecords;
using Tynamix.ObjectFiller;

namespace LondonFhirService.Manage.Tests.Acceptance.Apis.FhirRecords
{
    [Collection(nameof(ApiTestCollection))]
    public partial class FhirRecordApiTests
    {
        private readonly ApiBroker apiBroker;

        public FhirRecordApiTests(ApiBroker apiBroker) =>
            this.apiBroker = apiBroker;

        private int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTime() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static string GetRandomStringWithLengthOf(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
        }

        private static FhirRecord UpdateFhirRecordWithRandomValues(FhirRecord inputFhirRecord)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            var updatedFhirRecord = CreateRandomFhirRecord();
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

        private static FhirRecord CreateRandomFhirRecord() =>
            CreateRandomFhirRecordFiller().Create();

        private static Filler<FhirRecord> CreateRandomFhirRecordFiller()
        {
            string user = Guid.NewGuid().ToString();
            DateTime now = DateTime.UtcNow;
            var filler = new Filler<FhirRecord>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(now)
                .OnType<DateTimeOffset?>().Use(now)

                // TODO:  Add your property configurations here, 
                // this includes removing any properties that should be ignored and length constraints to avoid test failures.

                .OnProperty(fhirRecord => fhirRecord.CreatedDate).Use(now)
                .OnProperty(fhirRecord => fhirRecord.CreatedBy).Use(user)
                .OnProperty(fhirRecord => fhirRecord.UpdatedDate).Use(now)
                .OnProperty(fhirRecord => fhirRecord.UpdatedBy).Use(user);

            return filler;
        }
    }
}