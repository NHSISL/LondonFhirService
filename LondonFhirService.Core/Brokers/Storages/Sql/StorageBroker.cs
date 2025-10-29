// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFxceptions;
using LondonFhirService.Core.Models.Foundations.Audits;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.Consumers;
using LondonFhirService.Core.Models.Foundations.OdsDatas;
using LondonFhirService.Core.Models.Foundations.PdsDatas;
using LondonFhirService.Core.Models.Foundations.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using STX.EFCore.Client.Clients;

namespace LondonFhirService.Core.Brokers.Storages.Sql
{
    public partial class StorageBroker : EFxceptionsContext, IStorageBroker
    {
        private readonly IConfiguration configuration;
        private readonly IEFCoreClient efCoreClient;

        public StorageBroker(IConfiguration configuration)
        {
            this.configuration = configuration;
            efCoreClient = new EFCoreClient(this);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            string connectionString = configuration
                .GetConnectionString(name: "LondonFhirServiceConnectionString") ?? string.Empty;

            optionsBuilder.UseSqlServer(connectionString, config => config.UseHierarchyId());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            AddConfigurations(modelBuilder);
        }

        private static void AddConfigurations(ModelBuilder modelBuilder)
        {
            AddAuditConfigurations(modelBuilder.Entity<Audit>());
            AddConsumerConfigurations(modelBuilder.Entity<Consumer>());
            AddConsumerAccessConfigurations(modelBuilder.Entity<ConsumerAccess>());
            AddPdsDataConfigurations(modelBuilder.Entity<PdsData>());
            AddOdsDataConfigurations(modelBuilder.Entity<OdsData>());
            AddProviderConfigurations(modelBuilder.Entity<Provider>());
        }

        private async ValueTask<T> InsertAsync<T>(T @object) where T : class =>
            await efCoreClient.InsertAsync(@object);

        private async ValueTask<IQueryable<T>> SelectAllAsync<T>() where T : class =>
            await efCoreClient.SelectAllAsync<T>();

        private async ValueTask<T> SelectAsync<T>(params object[] @objectIds) where T : class =>
            await efCoreClient.SelectAsync<T>(@objectIds);

        private async ValueTask<T> UpdateAsync<T>(T @object) where T : class =>
            await efCoreClient.UpdateAsync(@object);

        private async ValueTask<T> DeleteAsync<T>(T @object) where T : class =>
            await efCoreClient.DeleteAsync(@object);

        private async ValueTask BulkInsertAsync<T>(IEnumerable<T> objects) where T : class =>
            await efCoreClient.BulkInsertAsync(objects);

        private async ValueTask BulkUpdateAsync<T>(IEnumerable<T> objects) where T : class =>
            await efCoreClient.BulkUpdateAsync(objects);

        private async ValueTask BulkDeleteAsync<T>(IEnumerable<T> objects) where T : class =>
            await efCoreClient.BulkDeleteAsync(objects);
    }
}