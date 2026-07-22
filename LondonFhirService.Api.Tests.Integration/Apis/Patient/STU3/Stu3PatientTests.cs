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
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Models.Orchestrations.Accesses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tynamix.ObjectFiller;
using Xunit;

namespace LondonFhirService.Api.Tests.Integration.Apis.Patient.STU3
{
    [Collection(nameof(ApiTestCollection))]
    public partial class Stu3PatientTests
    {
        private readonly ApiBroker apiBroker;
        private readonly ITestOutputHelper output;
        private readonly IConfiguration configuration;
        private readonly DdsConfigurations ddsConfigurations;
        private readonly AccessConfigurations accessConfigurations;

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

            accessConfigurations =
                configuration
                    .GetSection("AccessConfigurations")
                    .Get<AccessConfigurations>()
                        ?? throw new InvalidOperationException(
                            "AccessConfigurations configuration section is missing or invalid.");

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
                .OnProperty(provider => provider.FullyQualifiedName).Use(providerName)
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
                        p.FullyQualifiedName == provider.FullyQualifiedName &&
                        p.FhirVersion == provider.FhirVersion &&
                        p.IsPrimary == provider.IsPrimary);

                if (providerExists is not null)
                {
                    return providerExists;
                }

                return await storageBroker.InsertProviderAsync(provider);
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
            string dateOfBirth = null,
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

            if (!string.IsNullOrWhiteSpace(dateOfBirth))
            {
                parameters.Add(
                    "patientDOB",
                    new Identifier
                    {
                        System = "https://fhir.hl7.org.uk/Id/dob",
                        Value = dateOfBirth
                    });
            }

            if (demographicsOnly.HasValue)
            {
                parameters.Parameter.Add(new Parameters.ParameterComponent
                {
                    Name = "demographicsOnly",
                    Part =
                    {
                        new Parameters.ParameterComponent
                        {
                            Name = "includeDemographicsOnly",
                            Value = new FhirBoolean(demographicsOnly.Value)
                        }
                    }
                });
            }

            if (includeInactivePatients.HasValue)
            {
                parameters.Parameter.Add(new Parameters.ParameterComponent
                {
                    Name = "includeInactivePatients",
                    Part =
                    {
                        new Parameters.ParameterComponent
                        {
                            Name = "includeInactivePatients",
                            Value = new FhirBoolean(includeInactivePatients.Value)
                        }
                    }
                });
            }

            return parameters;
        }
    }
}

