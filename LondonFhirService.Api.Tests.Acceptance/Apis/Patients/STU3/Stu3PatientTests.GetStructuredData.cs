// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using FluentAssertions;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Foundations.Providers;
using Patient = Hl7.Fhir.Model.Patient;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Api.Tests.Acceptance.Apis.Patients.STU3
{
    public partial class Stu3PatientTests
    {
        [Fact]
        public async Task ShouldGetStructuredDataAsync()
        {
            Provider provider = null;

            try
            {
                // given
                string nhsNumber = GenerateRandom10DigitNumber();
                string inputId = nhsNumber;
                DateTime? dateOfBirth = null;
                bool? demographicsOnly = false;
                bool? includeInactivePatients = false;
                DateTimeOffset now = DateTimeOffset.UtcNow;
                DateTimeOffset randomInputStart = now.AddDays(-1);
                string randomInputTypeFilter = GetRandomString();
                string inputTypeFilter = randomInputTypeFilter;
                DateTimeOffset randomInputSince = now.AddDays(-1);
                DateTimeOffset inputSince = randomInputSince;
                int randomInputCount = GetRandomNumber();
                int inputCount = randomInputCount;
                string fhirVersion = "STU3";
                string providerFriendlyName = "DDS";

                string providerFullyQualifiedName =
                    "LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Providers.DdsStu3Provider";

                Parameters inputParameters = CreateRandomParametersGetStructuredData(
                    nhsNumber,
                    dateOfBirth,
                    demographicsOnly,
                    includeInactivePatients);

                provider = await CreateRandomActiveProviderIfNoneExist(
                    providerFriendlyName,
                    providerFullyQualifiedName,
                    fhirVersion,
                    now);

                // when
                Bundle actualBundle =
                    await this.apiBroker.GetStructuredDataStu3Async(nhsNumber, inputParameters);

                // then
                actualBundle.Should().NotBeNull();
                actualBundle.Type.Should().Be(Bundle.BundleType.Collection);
                actualBundle.Entry.Should().NotBeNullOrEmpty();
                actualBundle.Entry.Should().HaveCountGreaterOrEqualTo(1);
                actualBundle.Meta.Should().NotBeNull();
                //actualBundle.Meta.Extension.Should().HaveCount(1);
                actualBundle.Entry[0].Resource.Should().BeOfType<Patient>();
                var patient = actualBundle.Entry[0].Resource as Patient;
                patient!.Id.Should().Be(inputId);
            }
            finally
            {
                if (provider is not null)
                {
                    await CleanupProviderAsync(provider);
                }
            }
        }
    }
}
