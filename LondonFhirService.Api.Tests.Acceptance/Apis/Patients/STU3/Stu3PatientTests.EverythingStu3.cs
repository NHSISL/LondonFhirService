// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

extern alias FhirSTU3;
using System;
using FluentAssertions;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.Consumers;
using LondonFhirService.Core.Models.Foundations.OdsDatas;
using LondonFhirService.Core.Models.Foundations.PdsDatas;
using LondonFhirService.Core.Models.Foundations.Providers;
using Patient = FhirSTU3::Hl7.Fhir.Model.Patient;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Api.Tests.Acceptance.Apis.Patients.STU3
{
    public partial class Stu3PatientTests
    {
        [Fact]
        public async Task ShouldGetPatientEverythingStu3Async()
        {
            PdsData pdsData = null;
            OdsData odsData = null;
            Consumer consumer = null;
            ConsumerAccess consumerAccess = null;
            Provider provider = null;

            try
            {
                // given
                string nhsNumber = GenerateRandom10DigitNumber();
                string inputId = nhsNumber;
                string orgCode = GetRandomStringWithLengthOf(15);
                DateTimeOffset now = DateTimeOffset.UtcNow;
                string userId = TestAuthHandler.TestUserId;
                DateTimeOffset randomInputStart = now.AddDays(-1);
                DateTimeOffset inputStart = randomInputStart;
                DateTimeOffset randomInputEnd = now.AddDays(1);
                DateTimeOffset inputEnd = randomInputEnd;
                string randomInputTypeFilter = GetRandomString();
                string inputTypeFilter = randomInputTypeFilter;
                DateTimeOffset randomInputSince = now.AddDays(-1);
                DateTimeOffset inputSince = randomInputSince;
                int randomInputCount = GetRandomNumber();
                int inputCount = randomInputCount;
                string providerName = "LDS";
                string fhirVersion = "STU3";

                Parameters inputParameters = CreateRandomParameters(
                    start: inputStart,
                    end: inputEnd,
                    typeFilter: inputTypeFilter,
                    since: inputSince,
                    count: inputCount);

                consumer = await CreateRandomConsumer(now, userId);
                odsData = await CreateRandomOdsData(orgCode, now);

                consumerAccess = await CreateRandomConsumerAccess(
                    consumer.Id,
                    orgCode,
                    now,
                    userId);

                pdsData = await CreateRandomPdsData(nhsNumber, orgCode, now);
                provider = await CreateRandomActiveProvider(providerName, fhirVersion, now);

                // when
                Bundle actualBundle =
                    await this.apiBroker.EverythingStu3Async(inputId, inputParameters);

                // then
                actualBundle.Should().NotBeNull();
                actualBundle.Type.Should().Be(Bundle.BundleType.Collection);
                actualBundle.Entry.Should().NotBeNullOrEmpty();
                actualBundle.Entry.Should().HaveCountGreaterOrEqualTo(1);
                actualBundle.Meta.Should().NotBeNull();
                actualBundle.Meta.Extension.Should().HaveCount(1);
                actualBundle.Entry[0].Resource.Should().BeOfType<Patient>();
                var patient = actualBundle.Entry[0].Resource as Patient;
                patient!.Id.Should().Be(inputId);
            }
            finally
            {
                await CleanupPdsDataAsync(pdsData);
                await CleanupOdsDataAsync(odsData);
                await CleanupConsumerAccessAsync(consumerAccess);
                await CleanupConsumerAsync(consumer);
                await CleanupProviderAsync(provider);
            }
        }
    }
}
