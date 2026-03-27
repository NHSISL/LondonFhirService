// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LondonFhirService.Manage.Tests.Acceptance.Brokers;
using LondonFhirService.Manage.Tests.Acceptance.Models.FhirRecordDifferences;
using Tynamix.ObjectFiller;

namespace LondonFhirService.Manage.Tests.Acceptance.Apis.FhirRecordDifferences
{
    [Collection(nameof(ApiTestCollection))]
    public partial class FhirRecordDifferenceApiTests
    {
        private readonly ApiBroker apiBroker;

        public FhirRecordDifferenceApiTests(ApiBroker apiBroker) =>
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
            FhirRecordDifference createdFhirRecordDifference = await this.apiBroker.PostFhirRecordDifferenceAsync(randomFhirRecordDifference);

            return createdFhirRecordDifference;
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
    }
}