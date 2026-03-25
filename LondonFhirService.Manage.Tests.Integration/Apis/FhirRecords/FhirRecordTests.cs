// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LondonFhirService.Manage.Tests.Integration.Brokers;
using LondonFhirService.Manage.Tests.Integration.Models.FhirRecords;
using Tynamix.ObjectFiller;

namespace LondonFhirService.Manage.Tests.Integration.Apis.FhirRecords
{
    [Collection(nameof(ApiTestCollection))]
    public partial class FhirRecordApiTests
    {
        private readonly ApiBroker apiBroker;

        public FhirRecordApiTests(ApiBroker apiBroker) =>
            this.apiBroker = apiBroker;

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static int GetRandomNegativeNumber() =>
            -1 * new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static DateTimeOffset GetRandomPastDateTimeOffset()
        {
            DateTime now = DateTimeOffset.UtcNow.Date;
            int randomDaysInPast = GetRandomNegativeNumber();
            DateTime pastDateTime = now.AddDays(randomDaysInPast).Date;

            return new DateTimeRange(earliestDate: pastDateTime, latestDate: now).GetValue();
        }

        private static string GetRandomStringWithLengthOf(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
        }

        private static string GetRandomEmail()
        {
            string randomPrefix = GetRandomStringWithLengthOf(15);
            string emailSuffix = "@email.com";

            return randomPrefix + emailSuffix;
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
            return await this.apiBroker.PostFhirRecordAsync(randomFhirRecord);
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
    }
}
