// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

extern alias FhirSTU3;
using System;
using System.Linq;
using FluentAssertions;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.Consumers;
using LondonFhirService.Core.Models.Foundations.OdsDatas;
using LondonFhirService.Core.Models.Foundations.PdsDatas;
using LondonFhirService.Core.Models.Foundations.Providers;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Api.Tests.Integration.Apis.Patient.STU3
{
    public partial class Stu3PatientTests
    {
        [Fact]
        public async Task ShouldGetStructuredRecordStu3Async()
        {
            // given
            string inputNhsNumber = "9435797881";
            string orgCode = GetRandomStringWithLengthOf(15);
            DateTimeOffset now = DateTimeOffset.UtcNow;
            string userId = TestAuthHandler.TestUserId;
            string providerName = "LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Providers.DdsStu3Provider";
            string fhirVersion = "STU3";

            Parameters inputParameters = null;

            Provider provider = await CreateRandomActiveProvider(providerName, fhirVersion, now);
            Consumer consumer = await CreateRandomConsumer(now, userId);
            OdsData odsData = await CreateRandomOdsData(orgCode, now);

            ConsumerAccess consumerAccess = await CreateRandomConsumerAccess(
                consumer.Id,
                orgCode,
                now,
                userId);

            PdsData pdsData = await CreateRandomPdsData(inputNhsNumber, orgCode, now);

            // when
            Bundle actualBundle =
                await this.apiBroker.GetStructuredRecordStu3Async(inputNhsNumber, inputParameters);

            // then
            actualBundle.Should().NotBeNull();
            actualBundle.Type.Should().Be(Bundle.BundleType.Collection);
            actualBundle.Entry.Should().NotBeNullOrEmpty();
            actualBundle.Entry.Should().HaveCountGreaterOrEqualTo(1);
            actualBundle.Meta.Should().NotBeNull();
            actualBundle.Entry[0].Resource.Should().BeOfType<FhirSTU3::Hl7.Fhir.Model.Patient>();
            var patient = actualBundle.Entry[0].Resource as FhirSTU3::Hl7.Fhir.Model.Patient;

            var nhsNumberIdentifier = patient!.Identifier
                .FirstOrDefault(id => id.System == "https://fhir.hl7.org.uk/Id/nhs-number");

            nhsNumberIdentifier.Value.Should().Be(inputNhsNumber);
            patient.Meta.Should().NotBeNull();
            await CleanupPdsDataAsync(pdsData);
            await CleanupOdsDataAsync(odsData);
            await CleanupConsumerAccessAsync(consumerAccess);
            await CleanupConsumerAsync(consumer);
            await CleanupProviderAsync(provider);
        }
    }
}
