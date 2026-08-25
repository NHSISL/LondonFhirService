// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LondonFhirService.Manage.Tests.Acceptance.Brokers;
using LondonFhirService.Manage.Tests.Acceptance.Models.Providers;
using Tynamix.ObjectFiller;

namespace LondonFhirService.Manage.Tests.Acceptance.Apis.Providers
{
    /// <summary>
    /// Unlike audits and metrics, nothing on this controller is hidden behind the invisible-api
    /// key — providers are configuration an operator is expected to manage. So every verb gets a
    /// straight success test rather than a blocked one.
    /// </summary>
    [Collection(nameof(ApiTestCollection))]
    public partial class ProviderApiTests
    {
        private readonly ApiBroker apiBroker;

        public ProviderApiTests(ApiBroker apiBroker) =>
            this.apiBroker = apiBroker;

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static string GetRandomStringWithLengthOf(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
        }

        private static Provider UpdateProviderWithRandomValues(Provider inputProvider)
        {
            Provider updatedProvider = CreateRandomProvider();
            updatedProvider.Id = inputProvider.Id;
            updatedProvider.CreatedBy = inputProvider.CreatedBy;
            updatedProvider.CreatedDate = inputProvider.CreatedDate;
            updatedProvider.UpdatedDate = DateTimeOffset.UtcNow;

            return updatedProvider;
        }

        private async ValueTask<Provider> PostRandomProviderAsync()
        {
            Provider randomProvider = CreateRandomProvider();

            return await this.apiBroker.PostProviderAsync(randomProvider);
        }

        private async ValueTask<List<Provider>> PostRandomProvidersAsync()
        {
            int randomNumber = GetRandomNumber();
            var randomProviders = new List<Provider>();

            for (int i = 0; i < randomNumber; i++)
            {
                randomProviders.Add(await PostRandomProviderAsync());
            }

            return randomProviders;
        }

        private static Provider CreateRandomProvider() =>
            CreateRandomProviderFiller().Create();

        /// <summary>
        /// IsPrimary is forced false. The patient orchestration validates that exactly one
        /// primary provider exists, so seeding a random one would make the registry invalid for
        /// anything else reading it during the run.
        /// </summary>
        private static Filler<Provider> CreateRandomProviderFiller()
        {
            string user = Guid.NewGuid().ToString();
            DateTimeOffset now = DateTimeOffset.UtcNow;
            var filler = new Filler<Provider>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(now)
                .OnType<DateTimeOffset?>().Use(now)
                .OnProperty(provider => provider.IsPrimary).Use(false)
                .OnProperty(provider => provider.FriendlyName).Use(GetRandomStringWithLengthOf(50))
                .OnProperty(provider => provider.FullyQualifiedName).Use(GetRandomStringWithLengthOf(50))
                .OnProperty(provider => provider.FhirVersion).Use("STU3")
                .OnProperty(provider => provider.CreatedBy).Use(user)
                .OnProperty(provider => provider.CreatedDate).Use(now)
                .OnProperty(provider => provider.UpdatedBy).Use(user)
                .OnProperty(provider => provider.UpdatedDate).Use(now);

            return filler;
        }
    }
}
