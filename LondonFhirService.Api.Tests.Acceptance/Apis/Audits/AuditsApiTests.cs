// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LondonFhirService.Api.Tests.Acceptance.Brokers;
using LondonFhirService.Api.Tests.Acceptance.Models.Audits;
using Tynamix.ObjectFiller;

namespace LondonFhirService.Api.Tests.Acceptance.Apis
{
    [Collection(nameof(ApiTestCollection))]
    public partial class AuditsApiTests
    {
        private readonly ApiBroker apiBroker;

        public AuditsApiTests(ApiBroker apiBroker) =>
            this.apiBroker = apiBroker;

        private static Audit CreateRandomAudit() =>
           CreateRandomAuditFiller().Create();

        private int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTime() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static string GetRandomStringWithLengthOf(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
        }

        private static Audit UpdateAuditWithRandomValues(Audit inputAudit)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            var updatedAudit = CreateRandomAudit();
            updatedAudit.Id = inputAudit.Id;
            updatedAudit.CreatedDate = inputAudit.CreatedDate;
            updatedAudit.CreatedBy = inputAudit.CreatedBy;
            updatedAudit.UpdatedDate = now;

            return updatedAudit;
        }

        private async ValueTask<Audit> PostRandomAuditAsync()
        {
            Audit randomAudit = CreateRandomAudit();
            Audit createdAudit = await this.apiBroker.PostAuditAsync(randomAudit);

            return createdAudit;
        }

        private async ValueTask<List<Audit>> PostRandomAuditsAsync()
        {
            int randomNumber = GetRandomNumber();
            var randomAudits = new List<Audit>();

            for (int i = 0; i < randomNumber; i++)
            {
                randomAudits.Add(await PostRandomAuditAsync());
            }

            return randomAudits;
        }

        private static Filler<Audit> CreateRandomAuditFiller()
        {
            string user = Guid.NewGuid().ToString();
            DateTime now = DateTime.UtcNow;
            var filler = new Filler<Audit>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(now)
                .OnType<DateTimeOffset?>().Use(now)
                .OnProperty(user => user.AuditType).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(user => user.LogLevel).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(user => user.CreatedBy).Use(user)
                .OnProperty(user => user.UpdatedBy).Use(user);

            return filler;
        }
    }
}
