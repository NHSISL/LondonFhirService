// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

extern alias FhirR4;
using System;
using FluentAssertions;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.Consumers;
using LondonFhirService.Core.Models.Foundations.OdsDatas;
using LondonFhirService.Core.Models.Foundations.PdsDatas;
using LondonFhirService.Core.Models.Foundations.Providers;
using Patient = FhirR4::Hl7.Fhir.Model.Patient;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Api.Tests.Acceptance.Apis.Patients.R4
{
    public partial class R4PatientTests
    {
        [Fact(Skip = "TODO: Fix allowing this test to run in parallel with STU3, works in isolation")]
        public async Task ShouldGetPatientEverythingR4Async()
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
            string providerName = "DDS";
            string fhirVersion = "R4";

            Parameters inputParameters = CreateRandomParameters(
                start: inputStart,
                end: inputEnd,
                typeFilter: inputTypeFilter,
                since: inputSince,
                count: inputCount);

            Consumer consumer = await CreateRandomConsumer(now, userId);
            OdsData odsData = await CreateRandomOdsData(orgCode, now);

            ConsumerAccess consumerAccess = await CreateRandomConsumerAccess(
                consumer.Id,
                orgCode,
                now,
                userId);

            PdsData pdsData = await CreateRandomPdsData(nhsNumber, orgCode, now);
            Provider provider = await CreateRandomActiveProvider(providerName, fhirVersion, now);

            // when
            Bundle actualBundle =
                await this.apiBroker.EverythingR4Async(inputId, inputParameters);

            // then
            actualBundle.Should().NotBeNull();
            actualBundle.Type.Should().Be(Bundle.BundleType.Searchset);
            actualBundle.Entry.Should().NotBeNullOrEmpty();
            actualBundle.Entry.Should().HaveCountGreaterOrEqualTo(1);
            actualBundle.Meta.Should().NotBeNull();
            actualBundle.Entry[0].Resource.Should().BeOfType<Patient>();
            var patient = actualBundle.Entry[0].Resource as Patient;
            patient!.Id.Should().Be(inputId);
            patient.Meta.Should().NotBeNull();
            await CleanupPdsDataAsync(pdsData);
            await CleanupOdsDataAsync(odsData);
            await CleanupConsumerAccessAsync(consumerAccess);
            await CleanupConsumerAsync(consumer);
            await CleanupProviderAsync(provider);
        }
    }
}
