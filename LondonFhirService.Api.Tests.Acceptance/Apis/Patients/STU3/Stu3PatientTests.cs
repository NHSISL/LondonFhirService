// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Api.Tests.Acceptance.Brokers;
using LondonFhirService.Core.Brokers.Storages.Sql;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Models.Orchestrations.Accesses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tynamix.ObjectFiller;

namespace LondonFhirService.Api.Tests.Acceptance.Apis.Patients.STU3
{
    [Collection(nameof(ApiTestCollection))]
    public partial class Stu3PatientTests
    {
        private readonly ApiBroker apiBroker;
        private readonly AccessConfigurations accessConfigurations;

        public Stu3PatientTests(ApiBroker apiBroker)
        {
            this.apiBroker = apiBroker;

            this.accessConfigurations = apiBroker.configuration
                .GetSection("AccessConfigurations")
                .Get<AccessConfigurations>();
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

        private static Parameters CreateRandomParametersEverything(
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

        private static Parameters CreateRandomParametersGetStructuredData(
            string nhsNumber,
            DateTime? dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null)
        {
            var parameters = new Parameters();

            if (!string.IsNullOrWhiteSpace(nhsNumber))
            {
                parameters.Parameter.Add(new Parameters.ParameterComponent
                {
                    Name = "patientNHSNumber",
                    Value = new Identifier
                    {
                        System = "https://fhir.hl7.org.uk/Id/nhs-number",
                        Value = nhsNumber
                    }
                });
            }

            if (dateOfBirth.HasValue)
            {
                parameters.Parameter.Add(new Parameters.ParameterComponent
                {
                    Name = "patientDOB",
                    Value = new Identifier
                    {
                        System = "https://fhir.hl7.org.uk/Id/dob",
                        Value = dateOfBirth.Value.ToString("yyyy-MM-dd")
                    }
                });
            }

            if (demographicsOnly.HasValue && demographicsOnly.Value == true)
            {
                parameters.Parameter.Add(new Parameters.ParameterComponent
                {
                    Name = "demographicsOnly",
                    Part = new List<Parameters.ParameterComponent>
                    {
                        new Parameters.ParameterComponent
                        {
                            Name = "includeDemographicsOnly",
                            Value = new FhirBoolean(demographicsOnly.Value)
                        }
                    }
                });
            }

            if (includeInactivePatients.HasValue && includeInactivePatients.Value == true)
            {
                parameters.Parameter.Add(new Parameters.ParameterComponent
                {
                    Name = "includeInactivePatients",
                    Part = new List<Parameters.ParameterComponent>
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

        private async Task<Provider> CreateRandomActiveProviderIfNoneExist(
            string providerFriendlyName,
            string providerFullyQualifiedName,
            string fhirVersion,
            DateTimeOffset dateTimeOffset)
        {
            Provider randomProvider = CreateActiveProviderFiller(
                providerFriendlyName,
                providerFullyQualifiedName,
                fhirVersion,
                dateTimeOffset).Create();

            Provider createdProvider = await SeedProviderAsync(randomProvider);

            return createdProvider;
        }

        private static Filler<Provider> CreateActiveProviderFiller(
            string providerFriendlyName,
            string providerFullyQualifiedName,
            string fhirVersion,
            DateTimeOffset dateTimeOffset)
        {
            var filler = new Filler<Provider>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(dateTimeOffset)
                .OnType<DateTimeOffset?>().Use(dateTimeOffset)
                .OnProperty(provider => provider.FriendlyName).Use(providerFriendlyName)
                .OnProperty(provider => provider.FullyQualifiedName).Use(providerFullyQualifiedName)
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
                IQueryable<Provider> providerQuery = await storageBroker.SelectAllProvidersAsync();

                // If seeding a primary, check by FhirVersion alone
                if (provider.IsPrimary)
                {
                    DateTimeOffset now = DateTimeOffset.UtcNow;

                    var existingPrimary = providerQuery
                        .FirstOrDefault(p =>
                            p.FhirVersion == provider.FhirVersion
                            && p.IsPrimary
                            && p.IsActive
                            && (p.ActiveFrom == null || p.ActiveFrom <= now)
                            && (p.ActiveTo == null || p.ActiveTo >= now));

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
    }
}

