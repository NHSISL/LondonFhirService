// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.Providers;

namespace LondonFhirService.Core.Services.Foundations.Providers
{
    public partial class ProviderService : IProviderService
    {
        private readonly IStorageBroker storageBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ISecurityAuditBroker securityAuditBroker;
        private readonly ILoggingBroker loggingBroker;

        public ProviderService(
            IStorageBroker storageBroker,
            IDateTimeBroker dateTimeBroker,
            ISecurityAuditBroker securityAuditBroker,
            ILoggingBroker loggingBroker)
        {
            this.storageBroker = storageBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.securityAuditBroker = securityAuditBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<Provider> AddProviderAsync(Provider provider) =>
            TryCatch(async () =>
            {
                provider = await this.securityAuditBroker.ApplyAddAuditValuesAsync(provider);
                await ValidateProviderOnAdd(provider);

                return await this.storageBroker.InsertProviderAsync(provider);
            });

        public ValueTask<IQueryable<Provider>> RetrieveAllProvidersAsync() =>
            throw new NotImplementedException();

        public ValueTask<Provider> ModifyProviderAsync(Provider provider) =>
            TryCatch(async () =>
            {
                provider = await this.securityAuditBroker.ApplyModifyAuditValuesAsync(provider);
                await ValidateProviderOnModify(provider);

                Provider maybeProvider =
                    await this.storageBroker.SelectProviderByIdAsync(provider.Id);

                ValidateStorageProvider(maybeProvider, provider.Id);

                provider = await this.securityAuditBroker
                    .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(provider, maybeProvider);

                ValidateAgainstStorageProviderOnModify(
                    inputProvider: provider,
                    storageProvider: maybeProvider);

                return await this.storageBroker.UpdateProviderAsync(provider);
            });

    }
}
