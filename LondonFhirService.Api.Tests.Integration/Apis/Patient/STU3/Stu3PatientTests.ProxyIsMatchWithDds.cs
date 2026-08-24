// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using FluentAssertions;
using Hl7.Fhir.Model;
using LondonFhirService.Api.Tests.Integration.Utilities;
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
            DateTimeOffset now = DateTimeOffset.UtcNow;
            string providerName = "LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Providers.DdsStu3Provider";
            string fhirVersion = "STU3";
            Parameters inputParameters = CreateRandomGetStructuredRecordParameters(inputNhsNumber);
            Provider provider = await CreateRandomActiveProvider(providerName, fhirVersion, now);

            // when
            var (proxyText, proxyBundle) =
                await this.apiBroker.GetStructuredRecordStu3Async(inputNhsNumber, inputParameters);

            var (ddsText, ddsBundle) =
                await this.apiBroker.DdsGetStructuredRecordAsync(
                    grantType: "client_credentials",
                    clientId: ddsConfigurations.ClientId,
                    clientSecret: ddsConfigurations.ClientSecret,
                    nhsNumber: inputNhsNumber,
                    parameters: inputParameters);

            // then

            string proxyNormalized = JsonVolatileNormalizer.Normalize(proxyText);
            string ddsNormalized = JsonVolatileNormalizer.Normalize(ddsText);
            var patch = JsonCompare.GetPatch(proxyNormalized, ddsNormalized);

            if (!string.IsNullOrWhiteSpace(patch))
            {
                output.WriteLine(patch);
            }

            proxyNormalized.Should().Be(ddsNormalized);
        }

        [Fact]
        public async Task ShouldFailIfCheckIfProxyOutputIsNotMatchingDdsAsync()
        {
            // given
            string proxyInputNhsNumber = "9435797881";
            string ddsInputNhsNumber = "9435753973";
            DateTimeOffset now = DateTimeOffset.UtcNow;
            string providerName = "LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Providers.DdsStu3Provider";
            string fhirVersion = "STU3";
            Parameters proxyInputParameters = CreateRandomGetStructuredRecordParameters(proxyInputNhsNumber);
            Parameters ddsInputParameters = CreateRandomGetStructuredRecordParameters(ddsInputNhsNumber);
            Provider provider = await CreateRandomActiveProvider(providerName, fhirVersion, now);

            // when
            var (proxyText, proxyBundle) =
                await this.apiBroker.GetStructuredRecordStu3Async(proxyInputNhsNumber, proxyInputParameters);

            var (ddsText, ddsBundle) =
                await this.apiBroker.DdsGetStructuredRecordAsync(
                    grantType: "client_credentials",
                    clientId: ddsConfigurations.ClientId,
                    clientSecret: ddsConfigurations.ClientSecret,
                    nhsNumber: "9435753973",
                    parameters: ddsInputParameters);

            // then

            string proxyNormalized = JsonVolatileNormalizer.Normalize(proxyText);
            string ddsNormalized = JsonVolatileNormalizer.Normalize(ddsText);
            var patch = JsonCompare.GetPatch(proxyNormalized, ddsNormalized);

            if (!string.IsNullOrWhiteSpace(patch))
            {
                output.WriteLine(patch);
            }

            proxyNormalized.Should().NotBe(ddsNormalized);
        }
    }
}
