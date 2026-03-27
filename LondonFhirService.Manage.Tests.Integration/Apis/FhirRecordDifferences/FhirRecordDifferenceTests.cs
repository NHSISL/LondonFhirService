// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LondonFhirService.Manage.Tests.Integration.Brokers;
using LondonFhirService.Manage.Tests.Integration.Models.FhirRecordDifferences;
using Tynamix.ObjectFiller;

namespace LondonFhirService.Manage.Tests.Integration.Apis.FhirRecordDifferences
{
    [Collection(nameof(ApiTestCollection))]
    public partial class FhirRecordDifferenceApiTests
    {
        private readonly ApiBroker apiBroker;

        public FhirRecordDifferenceApiTests(ApiBroker apiBroker) =>
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

        private static FhirRecordDifference CreateRandomFhirRecordDifference() =>
            CreateRandomFhirRecordDifferenceFiller().Create();

        private static Filler<FhirRecordDifference> CreateRandomFhirRecordDifferenceFiller()
        {
            string user = Guid.NewGuid().ToString();
            DateTime now = DateTime.UtcNow;
            var filler = new Filler<FhirRecordDifference>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(now)
                .OnType<DateTimeOffset?>().Use(now)

                // TODO:  Add your property configurations here, 
                // this includes removing any properties that should be ignored and length constraints to avoid test failures.

                .OnProperty(fhirRecordDifference => fhirRecordDifference.CreatedDate).Use(now)
                .OnProperty(fhirRecordDifference => fhirRecordDifference.CreatedBy).Use(user)
                .OnProperty(fhirRecordDifference => fhirRecordDifference.UpdatedDate).Use(now)
                .OnProperty(fhirRecordDifference => fhirRecordDifference.UpdatedBy).Use(user);

            return filler;
        }

        private static FhirRecordDifference UpdateFhirRecordDifferenceWithRandomValues(FhirRecordDifference inputFhirRecordDifference)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            var updatedFhirRecordDifference = CreateRandomFhirRecordDifference();
            updatedFhirRecordDifference.Id = inputFhirRecordDifference.Id;
            updatedFhirRecordDifference.CreatedDate = inputFhirRecordDifference.CreatedDate;
            updatedFhirRecordDifference.CreatedBy = inputFhirRecordDifference.CreatedBy;
            updatedFhirRecordDifference.UpdatedDate = now;

            return updatedFhirRecordDifference;
        }

        private async ValueTask<FhirRecordDifference> PostRandomFhirRecordDifferenceAsync()
        {
            FhirRecordDifference randomFhirRecordDifference = CreateRandomFhirRecordDifference();
            return await this.apiBroker.PostFhirRecordDifferenceAsync(randomFhirRecordDifference);
        }

        private async ValueTask<List<FhirRecordDifference>> PostRandomFhirRecordDifferencesAsync()
        {
            int randomNumber = GetRandomNumber();
            var randomFhirRecordDifferences = new List<FhirRecordDifference>();

            for (int i = 0; i < randomNumber; i++)
            {
                randomFhirRecordDifferences.Add(await PostRandomFhirRecordDifferenceAsync());
            }

            return randomFhirRecordDifferences;
        }
    }
}
