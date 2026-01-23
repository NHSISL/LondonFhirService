// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Api.Tests.Integration.Brokers;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.Consumers;
using LondonFhirService.Core.Models.Foundations.OdsDatas;
using LondonFhirService.Core.Models.Foundations.PdsDatas;
using LondonFhirService.Core.Models.Foundations.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tynamix.ObjectFiller;
using Xunit.Abstractions;

namespace LondonFhirService.Api.Tests.Integration.Apis.Patient.STU3
{
    [Collection(nameof(ApiTestCollection))]
    public partial class Stu3PatientTests
    {
        private readonly ApiBroker apiBroker;
        private readonly ITestOutputHelper output;
        private readonly IConfiguration configuration;
        private readonly DdsConfigurations ddsConfigurations;

        public Stu3PatientTests(ApiBroker apiBroker, ITestOutputHelper output)
        {
            var testProjectPath =
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

            configuration = new ConfigurationBuilder()
                .SetBasePath(testProjectPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.Integration.json", optional: true, reloadOnChange: false)
                .Build();

            ddsConfigurations =
                configuration
                    .GetSection("DdsConfigurations")
                    .Get<DdsConfigurations>()
                        ?? throw new InvalidOperationException(
                            "DdsConfigurations configuration section is missing or invalid.");

            this.apiBroker = apiBroker;
            this.output = output;
        }

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static string GenerateRandom10DigitNumber()
        {
            Random random = new Random();
            var randomNumber = random.Next(1000000000, 2000000000).ToString();

            return randomNumber;
        }

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static string GetRandomStringWithLengthOf(int length)
        {
            string result =
                new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
        }

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static Parameters CreateRandomParameters(
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string typeFilter = null,
            DateTimeOffset? since = null,
            int? count = null)
        {
            var parameters = new Parameters();

            if (start.HasValue)
            {
                parameters.Add("start", new FhirDateTime(start.Value));
            }

            if (end.HasValue)
            {
                parameters.Add("end", new FhirDateTime(end.Value));
            }

            if (!string.IsNullOrWhiteSpace(typeFilter))
            {
                parameters.Add("_type", new FhirString(typeFilter));
            }

            if (since.HasValue)
            {
                parameters.Add("_since", new FhirDateTime(since.Value));
            }

            if (count.HasValue)
            {
                parameters.Add("_count", new Integer(count.Value));
            }

            return parameters;
        }

        private async Task<Provider> CreateRandomActiveProvider(
            string providerName,
            string fhirVersion,
            DateTimeOffset dateTimeOffset)
        {
            Provider randomProvider = CreateActiveProviderFiller(providerName, fhirVersion, dateTimeOffset).Create();
            Provider createdProvider = await SeedProviderAsync(randomProvider);

            return createdProvider;
        }

        private static Filler<Provider> CreateActiveProviderFiller(
            string providerName,
            string fhirVersion,
            DateTimeOffset dateTimeOffset)
        {
            var filler = new Filler<Provider>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnType<DateTimeOffset?>().Use(dateTimeOffset)
                .OnProperty(provider => provider.Name).Use(providerName)
                .OnProperty(provider => provider.FhirVersion).Use(fhirVersion)
                .OnProperty(provider => provider.IsActive).Use(true)
                .OnProperty(provider => provider.IsPrimary).Use(true)
                .OnProperty(provider => provider.IsForComparisonOnly).Use(false)

                .OnProperty(provider => provider.ActiveFrom)
                    .Use(dateTimeOffset.AddDays(-30))

                .OnProperty(provider => provider.ActiveTo)
                    .Use(dateTimeOffset.AddDays(30));

            return filler;
        }

        private async Task<Consumer> CreateRandomConsumer(DateTimeOffset dateTimeOffset, string userId = "")
        {
            Consumer randomConsumer = CreateConsumerFiller(dateTimeOffset, userId).Create();
            Consumer createdConsumer = await SeedConsumerAsync(randomConsumer);

            return createdConsumer;
        }

        private static Filler<Consumer> CreateConsumerFiller(DateTimeOffset dateTimeOffset, string userId = "")
        {
            userId = string.IsNullOrEmpty(userId) ? Guid.NewGuid().ToString() : userId;
            var filler = new Filler<Consumer>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnType<DateTimeOffset?>().Use(dateTimeOffset)
                .OnProperty(consumer => consumer.UserId).Use(userId)
                .OnProperty(consumer => consumer.Name).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(consumer => consumer.ActiveFrom).Use(dateTimeOffset.AddDays(-30))
                .OnProperty(consumer => consumer.ActiveTo).Use(dateTimeOffset.AddDays(30))
                .OnProperty(consumer => consumer.CreatedBy).Use(userId)
                .OnProperty(consumer => consumer.UpdatedBy).Use(userId)
                .OnProperty(consumer => consumer.ConsumerAccesses).IgnoreIt();

            return filler;
        }

        private async Task<ConsumerAccess> CreateRandomConsumerAccess(
            Guid consumerId,
            string orgCode,
            DateTimeOffset dateTimeOffset,
            string userId = "")
        {
            ConsumerAccess randomConsumerAccess = CreateConsumerAccessFiller(
                consumerId,
                orgCode,
                dateTimeOffset,
                userId).Create();

            ConsumerAccess createdConsumerAccess = await SeedConsumerAccessAsync(randomConsumerAccess);

            return createdConsumerAccess;
        }

        private static Filler<ConsumerAccess> CreateConsumerAccessFiller(
            Guid consumerId,
            string orgCode,
            DateTimeOffset dateTimeOffset,
            string userId)
        {
            var filler = new Filler<ConsumerAccess>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnType<DateTimeOffset?>().Use(dateTimeOffset)
                .OnProperty(consumerAccess => consumerAccess.ConsumerId).Use(consumerId)
                .OnProperty(consumerAccess => consumerAccess.OrgCode).Use(orgCode)
                .OnProperty(consumerAccess => consumerAccess.CreatedBy).Use(userId)
                .OnProperty(consumerAccess => consumerAccess.UpdatedBy).Use(userId);

            return filler;
        }

        private async Task<OdsData> CreateRandomOdsData(string organisationCode, DateTimeOffset dateTimeOffset)
        {
            OdsData randomOdsData = CreateOdsDataFiller(organisationCode, dateTimeOffset).Create();
            OdsData createdOdsData = await SeedOdsDataAsync(randomOdsData);

            return createdOdsData;
        }

        private static Filler<OdsData> CreateOdsDataFiller(
            string organisationCode,
            DateTimeOffset dateTimeOffset,
            HierarchyId hierarchyId = null)
        {
            var filler = new Filler<OdsData>();

            if (hierarchyId == null)
            {
                hierarchyId = HierarchyId.Parse("/");
            }

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnType<DateTimeOffset?>().Use(dateTimeOffset)
                .OnProperty(odsData => odsData.OrganisationCode).Use(organisationCode)
                .OnProperty(odsData => odsData.OrganisationName).Use(GetRandomStringWithLengthOf(30))
                .OnProperty(odsData => odsData.OdsHierarchy).Use(hierarchyId)
                .OnProperty(odsData => odsData.RelationshipWithParentStartDate).Use(dateTimeOffset.AddDays(-30))
                .OnProperty(odsData => odsData.RelationshipWithParentEndDate).Use(dateTimeOffset.AddDays(30));

            return filler;
        }

        private async Task<PdsData> CreateRandomPdsData(
            string nhsNumber,
            string organisationCode,
            DateTimeOffset dateTimeOffset)
        {
            PdsData randomPdsData = CreatePdsDataFiller(nhsNumber, organisationCode, dateTimeOffset).Create();
            PdsData createdPdsData = await SeedPdsDataAsync(randomPdsData);

            return createdPdsData;
        }

        private static Filler<PdsData> CreatePdsDataFiller(
            string nhsNumber,
            string organisationCode,
            DateTimeOffset dateTimeOffset)
        {
            var filler = new Filler<PdsData>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnType<DateTimeOffset?>().Use(dateTimeOffset)
                .OnProperty(pdsData => pdsData.NhsNumber).Use(nhsNumber)
                .OnProperty(pdsData => pdsData.OrgCode).Use(organisationCode)

                .OnProperty(pdsData => pdsData.RelationshipWithOrganisationEffectiveFromDate)
                    .Use(dateTimeOffset.AddDays(-30))

                .OnProperty(pdsData => pdsData.RelationshipWithOrganisationEffectiveToDate)
                    .Use(dateTimeOffset.AddDays(30));

            return filler;
        }

        private async ValueTask<Consumer> SeedConsumerAsync(Consumer consumer)
        {
            using (var scope = this.apiBroker.WebApplicationFactory.Services.CreateScope())
            {
                var storageBroker = scope.ServiceProvider.GetRequiredService<StorageBroker>();

                return await storageBroker.InsertConsumerAsync(consumer);
            }
        }

        private async ValueTask<ConsumerAccess> SeedConsumerAccessAsync(ConsumerAccess consumerAccess)
        {
            using (var scope = this.apiBroker.WebApplicationFactory.Services.CreateScope())
            {
                var storageBroker = scope.ServiceProvider.GetRequiredService<StorageBroker>();

                return await storageBroker.InsertConsumerAccessAsync(consumerAccess);
            }
        }

        private async ValueTask<PdsData> SeedPdsDataAsync(PdsData pdsData)
        {
            using (var scope = this.apiBroker.WebApplicationFactory.Services.CreateScope())
            {
                var storageBroker = scope.ServiceProvider.GetRequiredService<StorageBroker>();

                return await storageBroker.InsertPdsDataAsync(pdsData);
            }
        }

        private async ValueTask<OdsData> SeedOdsDataAsync(OdsData odsData)
        {
            using (var scope = this.apiBroker.WebApplicationFactory.Services.CreateScope())
            {
                var storageBroker = scope.ServiceProvider.GetRequiredService<StorageBroker>();

                return await storageBroker.InsertOdsDataAsync(odsData);
            }
        }

        private async ValueTask<Provider> SeedProviderAsync(Provider provider)
        {
            using (var scope = this.apiBroker.WebApplicationFactory.Services.CreateScope())
            {
                var storageBroker = scope.ServiceProvider.GetRequiredService<StorageBroker>();
                var providerQuery = await storageBroker.SelectAllProvidersAsync();

                // If seeding a primary, check by FhirVersion alone
                if (provider.IsPrimary)
                {
                    var existingPrimary = providerQuery
                        .FirstOrDefault(p => p.FhirVersion == provider.FhirVersion && p.IsPrimary);

                    if (existingPrimary is not null)
                    {
                        // Either return the existing one...
                        return existingPrimary;

                        // ...or throw a clearer exception instead of letting SQL do it:
                        // throw new InvalidOperationException(
                        //     $"A primary provider for FHIR version '{provider.FhirVersion}' already exists.");
                    }
                }

                // For non-primary, keep your original "idempotent by Name + FhirVersion"
                var providerExists = providerQuery
                    .FirstOrDefault(p =>
                        p.Name == provider.Name &&
                        p.FhirVersion == provider.FhirVersion &&
                        p.IsPrimary == provider.IsPrimary);

                if (providerExists is not null)
                {
                    return providerExists;
                }

                return await storageBroker.InsertProviderAsync(provider);
            }
        }

        private async ValueTask<Consumer> CleanupConsumerAsync(Consumer consumer)
        {
            using (var scope = this.apiBroker.WebApplicationFactory.Services.CreateScope())
            {
                var storageBroker = scope.ServiceProvider.GetRequiredService<StorageBroker>();

                return await storageBroker.DeleteConsumerAsync(consumer);
            }
        }

        private async ValueTask<ConsumerAccess> CleanupConsumerAccessAsync(ConsumerAccess consumerAccess)
        {
            using (var scope = this.apiBroker.WebApplicationFactory.Services.CreateScope())
            {
                var storageBroker = scope.ServiceProvider.GetRequiredService<StorageBroker>();

                return await storageBroker.DeleteConsumerAccessAsync(consumerAccess);
            }
        }

        private async ValueTask<PdsData> CleanupPdsDataAsync(PdsData pdsData)
        {
            using (var scope = this.apiBroker.WebApplicationFactory.Services.CreateScope())
            {
                var storageBroker = scope.ServiceProvider.GetRequiredService<StorageBroker>();

                return await storageBroker.DeletePdsDataAsync(pdsData);
            }
        }

        private async ValueTask<OdsData> CleanupOdsDataAsync(OdsData odsData)
        {
            using (var scope = this.apiBroker.WebApplicationFactory.Services.CreateScope())
            {
                var storageBroker = scope.ServiceProvider.GetRequiredService<StorageBroker>();

                return await storageBroker.DeleteOdsDataAsync(odsData);
            }
        }

        private async ValueTask<Provider> CleanupProviderAsync(Provider provider)
        {
            using (var scope = this.apiBroker.WebApplicationFactory.Services.CreateScope())
            {
                var storageBroker = scope.ServiceProvider.GetRequiredService<StorageBroker>();

                return await storageBroker.DeleteProviderAsync(provider);
            }
        }

        private static Parameters CreateRandomGetStructuredRecordParameters(
            string nhsNumber,
            DateTimeOffset? dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null)
        {
            var parameters = new Parameters();

            parameters.Add(
                "patientNHSNumber",
                new Identifier
                {
                    System = "https://fhir.hl7.org.uk/Id/nhs-number",
                    Value = nhsNumber
                });

            if (dateOfBirth.HasValue)
            {
                parameters.Add("dateOfBirth", new FhirDateTime(dateOfBirth.Value));
            }

            if (demographicsOnly.HasValue)
            {
                parameters.Add("demographicsOnly", new FhirBoolean(demographicsOnly.Value));
            }

            if (includeInactivePatients.HasValue)
            {
                parameters.Add("includeInactivePatients", new FhirBoolean(includeInactivePatients.Value));
            }

            return parameters;
        }
    }
}

