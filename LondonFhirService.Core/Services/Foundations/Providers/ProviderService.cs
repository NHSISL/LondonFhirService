// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.DateTimes;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Brokers.Securities;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.Providers;

namespace LondonFhirService.Core.Services.Foundations.Providers
{
    internal partial class ProviderService : IProviderService
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

        public ValueTask<Provider> AddProviderAsync(
            Provider provider,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                provider = await this.securityAuditBroker.ApplyAddAuditValuesAsync(provider);
                await ValidateProviderOnAdd(provider);

                return await this.storageBroker.InsertProviderAsync(provider, cancellationToken);
            });

        public ValueTask<IQueryable<Provider>> RetrieveAllProvidersAsync(
            CancellationToken cancellationToken = default) =>
            TryCatch(async () => await this.storageBroker.SelectAllProvidersAsync(cancellationToken));

        public ValueTask<List<Provider>> RetrieveAllProvidersAsListAsync(
            CancellationToken cancellationToken = default) =>
            TryCatch(async () => await this.storageBroker.SelectAllProvidersAsListAsync(cancellationToken));

        public ValueTask<Provider> RetrieveProviderByIdAsync(
            Guid providerId,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                ValidateProviderId(providerId);

                Provider maybeProvider = await this.storageBroker
                    .SelectProviderByIdAsync(providerId, cancellationToken);

                ValidateStorageProvider(maybeProvider, providerId);

                return maybeProvider;
            });

        public ValueTask<Provider> ModifyProviderAsync(
            Provider provider,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                provider = await this.securityAuditBroker.ApplyModifyAuditValuesAsync(provider);
                await ValidateProviderOnModify(provider);

                Provider maybeProvider =
                    await this.storageBroker.SelectProviderByIdAsync(provider.Id, cancellationToken);

                ValidateStorageProvider(maybeProvider, provider.Id);

                provider = await this.securityAuditBroker
                    .EnsureAddAuditValuesRemainsUnchangedOnModifyAsync(provider, maybeProvider);

                ValidateAgainstStorageProviderOnModify(
                    inputProvider: provider,
                    storageProvider: maybeProvider);

                return await this.storageBroker.UpdateProviderAsync(provider, cancellationToken);
            });

        public ValueTask<Provider> RemoveProviderByIdAsync(
            Guid providerId,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                ValidateProviderId(providerId);

                Provider maybeProvider = await this.storageBroker
                    .SelectProviderByIdAsync(providerId, cancellationToken);

                ValidateStorageProvider(maybeProvider, providerId);

                return await this.storageBroker.DeleteProviderAsync(maybeProvider, cancellationToken);
            });
    }
}
