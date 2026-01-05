// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

extern alias FhirSTU3;
using System;
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
        public async Task ShouldCheckIfProxyOutputIsMatchingDdsAsync()
        {
            // given
            string inputNhsNumber = "9435797881";
            string orgCode = GetRandomStringWithLengthOf(15);
            DateTimeOffset now = DateTimeOffset.UtcNow;
            string userId = TestAuthHandler.TestUserId;
            string providerName = "LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Providers.DdsStu3Provider";
            string fhirVersion = "STU3";
            Parameters inputParameters = CreateRandomGetStructuredRecordParameters(inputNhsNumber);
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
            Bundle proxyBundle =
                await this.apiBroker.GetStructuredRecordStu3Async(inputNhsNumber, inputParameters);

            Bundle ddsBundle =
                await this.apiBroker.DdsGetStructuredRecordAsync(
                    grantType: "client_credentials",
                    clientId: ddsConfigurations.ClientId,
                    clientSecret: ddsConfigurations.ClientSecret,
                    nhsNumber: inputNhsNumber,
                    parameters: inputParameters);

            // then






            await CleanupPdsDataAsync(pdsData);
            await CleanupOdsDataAsync(odsData);
            await CleanupConsumerAccessAsync(consumerAccess);
            await CleanupConsumerAsync(consumer);
            await CleanupProviderAsync(provider);
        }
    }
}
