// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LondonFhirService.Manage.Tests.Acceptance.Brokers;
using LondonFhirService.Manage.Tests.Acceptance.Models.Audits;
using Tynamix.ObjectFiller;

namespace LondonFhirService.Manage.Tests.Acceptance.Apis.Audits
{
    /// <summary>
    /// Reads are the surface this controller actually offers. Create, update and delete carry
    /// [InvisibleApi], so the only success worth asserting on them is that they are unroutable
    /// without the key - which is what the Blocked tests do.
    ///
    /// The keyed broker still posts and deletes here, because that is exactly what the hidden
    /// verbs exist for: seeding a row to read back, and clearing it afterwards.
    /// </summary>
    [Collection(nameof(ApiTestCollection))]
    public partial class AuditApiTests
    {
        private readonly ApiBroker apiBroker;

        public AuditApiTests(ApiBroker apiBroker) =>
            this.apiBroker = apiBroker;

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static string GetRandomStringWithLengthOf(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
        }

        private async ValueTask<Audit> PostRandomAuditAsync()
        {
            Audit randomAudit = CreateRandomAudit();

            return await this.apiBroker.PostAuditAsync(randomAudit);
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

        private static Audit CreateRandomAudit() =>
            CreateRandomAuditFiller().Create();

        private static Filler<Audit> CreateRandomAuditFiller()
        {
            string user = Guid.NewGuid().ToString();
            DateTimeOffset now = DateTimeOffset.UtcNow;
            var filler = new Filler<Audit>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(now)
                .OnType<DateTimeOffset?>().Use(now)
                .OnProperty(audit => audit.AuditType).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(audit => audit.LogLevel).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(audit => audit.CreatedBy).Use(user)
                .OnProperty(audit => audit.CreatedDate).Use(now)
                .OnProperty(audit => audit.UpdatedBy).Use(user)
                .OnProperty(audit => audit.UpdatedDate).Use(now);

            return filler;
        }
    }
}
