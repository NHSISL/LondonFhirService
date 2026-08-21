// ---
// skill: the-standard-brokers
// type: template
// source-section: "1. Brokers"
// ---

// IStorageBroker.cs — root partial interface
namespace {Namespace}.Brokers.Storages.Sql
{
    public partial interface IStorageBroker
    { }
}

// IStorageBroker.{Entity}.cs — per-entity partial interface
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using {Namespace}.Models.Foundations.{Entity}s;

namespace {Namespace}.Brokers.Storages.Sql
{
    public partial interface IStorageBroker
    {
        ValueTask<{Entity}> Insert{Entity}Async(
            {Entity} {entity},
            CancellationToken cancellationToken = default);

        ValueTask<IQueryable<{Entity}>> SelectAll{Entity}sAsync();

        ValueTask<{Entity}> Select{Entity}ByIdAsync(
            Guid {entity}Id,
            CancellationToken cancellationToken = default);

        ValueTask<{Entity}> Update{Entity}Async(
            {Entity} {entity},
            CancellationToken cancellationToken = default);

        ValueTask<{Entity}> Delete{Entity}Async(
            {Entity} {entity},
            CancellationToken cancellationToken = default);

        ValueTask BulkInsert{Entity}sAsync(
            List<{Entity}> {entity}s,
            CancellationToken cancellationToken = default);

        ValueTask BulkUpdate{Entity}sAsync(
            List<{Entity}> {entity}s,
            CancellationToken cancellationToken = default);

        ValueTask BulkDelete{Entity}sAsync(
            List<{Entity}> {entity}s,
            CancellationToken cancellationToken = default);

        ValueTask<IEnumerable<{Entity}>> BulkRead{Entity}sAsync(
            List<{Entity}> {entity}s,
            CancellationToken cancellationToken = default);

        ValueTask BulkUpsert{Entity}sAsync(
            List<{Entity}> {entity}s,
            CancellationToken cancellationToken = default);

        ValueTask<bool> Exists{Entity}Async(
            Guid {entity}Id,
            CancellationToken cancellationToken = default);
    }
}

// StorageBroker.cs — root partial class
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EFxceptions;
using {Namespace}.Models.Foundations.{Entity}s;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using G2H.StorageClient.Clients;

namespace {Namespace}.Brokers.Storages.Sql
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
                .GetConnectionString(name: "{ConnectionStringName}") ?? string.Empty;

            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            AddConfigurations(modelBuilder);
        }

        private static void AddConfigurations(ModelBuilder modelBuilder)
        {
            Add{Entity}Configurations(modelBuilder.Entity<{Entity}>());
        }

        private async ValueTask<T> InsertAsync<T>(T @object, CancellationToken cancellationToken = default)
            where T : class =>
                await efCoreClient.InsertAsync(@object, cancellationToken);

        private async ValueTask<IQueryable<T>> SelectAllAsync<T>(CancellationToken cancellationToken = default)
            where T : class =>
                await efCoreClient.SelectAllAsync<T>(cancellationToken);

        private async ValueTask<T> SelectAsync<T>(object[] @objectIds, CancellationToken cancellationToken = default)
            where T : class =>
                await efCoreClient.SelectAsync<T>(@objectIds, cancellationToken);

        private async ValueTask<T> UpdateAsync<T>(T @object, CancellationToken cancellationToken = default)
            where T : class =>
                await efCoreClient.UpdateAsync(@object, cancellationToken);

        private async ValueTask<T> DeleteAsync<T>(T @object, CancellationToken cancellationToken = default)
            where T : class =>
                await efCoreClient.DeleteAsync(@object, cancellationToken);

        private async ValueTask BulkInsertAsync<T>(
            IEnumerable<T> objects,
            bool useTransaction = true,
            CancellationToken cancellationToken = default)
                where T : class =>
                    await efCoreClient.BulkInsertAsync<T>(objects, useTransaction, cancellationToken);

        private async ValueTask<IEnumerable<T>> BulkReadAsync<T>(
            IEnumerable<T> objects,
            CancellationToken cancellationToken = default)
                where T : class =>
                    await efCoreClient.BulkReadAsync<T>(objects, cancellationToken);

        private async ValueTask BulkUpdateAsync<T>(
            IEnumerable<T> objects,
            bool useTransaction = true,
            CancellationToken cancellationToken = default)
                where T : class =>
                    await efCoreClient.BulkUpdateAsync<T>(objects, useTransaction, cancellationToken);

        private async ValueTask BulkDeleteAsync<T>(
            IEnumerable<T> objects,
            bool useTransaction = true,
            CancellationToken cancellationToken = default)
                where T : class =>
                    await efCoreClient.BulkDeleteAsync<T>(objects, useTransaction, cancellationToken);

        private async ValueTask BulkUpsertAsync<T>(
            IEnumerable<T> objects,
            bool useTransaction = true,
            CancellationToken cancellationToken = default)
                where T : class =>
                    await efCoreClient.BulkUpsertAsync<T>(objects, useTransaction, cancellationToken);

        private async ValueTask<bool> ExistsAsync<T>(
            object[] objectIds,
            CancellationToken cancellationToken = default)
                where T : class =>
                    await efCoreClient.ExistsAsync<T>(objectIds, cancellationToken);
    }
}

// StorageBroker.{Entity}.cs — per-entity partial class
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EFxceptions;
using {Namespace}.Models.Foundations.{Entity}s;
using Microsoft.EntityFrameworkCore;

namespace {Namespace}.Brokers.Storages.Sql
{
    public partial class StorageBroker : EFxceptionsContext, IStorageBroker
    {
        public DbSet<{Entity}> {Entity}s { get; set; }

        public async ValueTask<{Entity}> Insert{Entity}Async(
            {Entity} {entity},
            CancellationToken cancellationToken = default) =>
                await InsertAsync({entity}, cancellationToken);

        public async ValueTask<IQueryable<{Entity}>> SelectAll{Entity}sAsync() =>
            await SelectAllAsync<{Entity}>();

        public async ValueTask<{Entity}> Select{Entity}ByIdAsync(
            Guid {entity}Id,
            CancellationToken cancellationToken = default) =>
                await SelectAsync<{Entity}>(new object[] { {entity}Id }, cancellationToken);

        public async ValueTask<{Entity}> Update{Entity}Async(
            {Entity} {entity},
            CancellationToken cancellationToken = default) =>
                await UpdateAsync({entity}, cancellationToken);

        public async ValueTask<{Entity}> Delete{Entity}Async(
            {Entity} {entity},
            CancellationToken cancellationToken = default) =>
                await DeleteAsync({entity}, cancellationToken);

        public async ValueTask BulkInsert{Entity}sAsync(
            List<{Entity}> {entity}s,
            CancellationToken cancellationToken = default) =>
                await BulkInsertAsync({entity}s, cancellationToken: cancellationToken);

        public async ValueTask BulkUpdate{Entity}sAsync(
            List<{Entity}> {entity}s,
            CancellationToken cancellationToken = default) =>
                await BulkUpdateAsync({entity}s, cancellationToken: cancellationToken);

        public async ValueTask BulkDelete{Entity}sAsync(
            List<{Entity}> {entity}s,
            CancellationToken cancellationToken = default) =>
                await BulkDeleteAsync({entity}s, cancellationToken: cancellationToken);

        public async ValueTask<IEnumerable<{Entity}>> BulkRead{Entity}sAsync(
            List<{Entity}> {entity}s,
            CancellationToken cancellationToken = default) =>
                await BulkReadAsync({entity}s, cancellationToken);

        public async ValueTask BulkUpsert{Entity}sAsync(
            List<{Entity}> {entity}s,
            CancellationToken cancellationToken = default) =>
                await BulkUpsertAsync({entity}s, cancellationToken: cancellationToken);

        public async ValueTask<bool> Exists{Entity}Async(
            Guid {entity}Id,
            CancellationToken cancellationToken = default) =>
                await ExistsAsync<{Entity}>(new object[] { {entity}Id }, cancellationToken);
    }
}

// StorageBroker.{Entity}.Configurations.cs — per-entity EF Core configurations
using {Namespace}.Models.Foundations.{Entity}s;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace {Namespace}.Brokers.Storages.Sql
{
    public partial class StorageBroker
    {
        private static void Add{Entity}Configurations(EntityTypeBuilder<{Entity}> model)
        {
            model
                .ToTable("{Entity}s");

            // Primary key
            model
                .HasKey({entity} => {entity}.Id);

            // Required properties
            model
                .Property({entity} => {entity}.Id)
                .IsRequired();

            model
                .Property({entity} => {entity}.CreatedBy)
                .HasMaxLength(255)
                .IsRequired();

            model
                .Property({entity} => {entity}.CreatedWhen)
                .IsRequired();

            model
                .Property({entity} => {entity}.UpdatedBy)
                .HasMaxLength(255)
                .IsRequired();

            model
                .Property({entity} => {entity}.UpdatedWhen)
                .IsRequired();

            // Add additional property configurations here

            // Add indexes here

            // Add relationships here
        }
    }
}
