// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LondonFhirService.Api.Tests.Acceptance.Brokers;
using LondonFhirService.Api.Tests.Acceptance.Models.FhirRecordDifferences;
using Tynamix.ObjectFiller;

namespace LondonFhirService.Api.Tests.Acceptance.Apis.FhirRecordDifferences
{
    [Collection(nameof(ApiTestCollection))]
    public partial class FhirRecordDifferencesApiTests
    {
        private readonly ApiBroker apiBroker;

        public FhirRecordDifferencesApiTests(ApiBroker apiBroker) =>
            this.apiBroker = apiBroker;

        private static FhirRecordDifference CreateRandomFhirRecordDifference() =>
            CreateRandomFhirRecordDifferenceFiller().Create();

        private int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTime() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static FhirRecordDifference UpdateFhirRecordDifferenceWithRandomValues(
            FhirRecordDifference inputFhirRecordDifference)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            FhirRecordDifference updatedFhirRecordDifference = CreateRandomFhirRecordDifference();
            updatedFhirRecordDifference.Id = inputFhirRecordDifference.Id;
            updatedFhirRecordDifference.CreatedDate = inputFhirRecordDifference.CreatedDate;
            updatedFhirRecordDifference.CreatedBy = inputFhirRecordDifference.CreatedBy;
            updatedFhirRecordDifference.UpdatedDate = now;

            return updatedFhirRecordDifference;
        }

        private async ValueTask<FhirRecordDifference> PostRandomFhirRecordDifferenceAsync()
        {
            FhirRecordDifference randomFhirRecordDifference = CreateRandomFhirRecordDifference();

            FhirRecordDifference createdFhirRecordDifference =
                await this.apiBroker.PostFhirRecordDifferenceAsync(randomFhirRecordDifference);

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

        private static Filler<FhirRecordDifference> CreateRandomFhirRecordDifferenceFiller()
        {
            string user = Guid.NewGuid().ToString();
            DateTime now = DateTime.UtcNow;
            var filler = new Filler<FhirRecordDifference>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(now)
                .OnType<DateTimeOffset?>().Use(now)
                .OnProperty(fhirRecordDifference => fhirRecordDifference.CreatedBy).Use(user)
                .OnProperty(fhirRecordDifference => fhirRecordDifference.UpdatedBy).Use(user);

            return filler;
        }
    }
}
