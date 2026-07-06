// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using FluentAssertions;
using Hl7.Fhir.Model;
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
            DateTimeOffset now = DateTimeOffset.UtcNow;
            string providerName = "LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Providers.DdsStu3Provider";
            string fhirVersion = "STU3";

            Parameters inputParametersWithDemographicsOnly = CreateRandomGetStructuredRecordParameters(
                nhsNumber: inputNhsNumber,
                demographicsOnly: true);

            Parameters inputParameters = CreateRandomGetStructuredRecordParameters(
                nhsNumber: inputNhsNumber,
                demographicsOnly: false);

            Provider provider = await CreateRandomActiveProvider(providerName, fhirVersion, now);

            // when
            var (actualText, actualBundle) =
                await this.apiBroker.GetStructuredRecordStu3Async(inputNhsNumber, inputParameters);

            var (actualTextWithDemographics, actualBundleWithDemographics) =
                await this.apiBroker.GetStructuredRecordStu3Async(inputNhsNumber, inputParametersWithDemographicsOnly);

            // then
            int actualTextLength = actualText.Length;
            int actualTextWithDemographicsOnlyLength = actualTextWithDemographics.Length;
            actualTextLength.Should().BeGreaterThan(actualTextWithDemographicsOnlyLength);
            output.WriteLine($"Actual Text: {actualTextLength}");
            output.WriteLine($"Actual Text With Demographics Only: {actualTextWithDemographicsOnlyLength}");
            actualBundle.Should().NotBeNull();
            actualBundle.Type.Should().Be(Bundle.BundleType.Collection);
            actualBundle.Entry.Should().NotBeNullOrEmpty();
            actualBundle.Entry.Should().HaveCountGreaterOrEqualTo(1);
            actualBundle.Meta.Should().NotBeNull();
            actualBundle.Entry[0].Resource.Should().BeOfType<Hl7.Fhir.Model.Patient>();
            var patient = actualBundle.Entry[0].Resource as Hl7.Fhir.Model.Patient;

            var nhsNumberIdentifier = patient!.Identifier
                .FirstOrDefault(id => id.System == "https://fhir.hl7.org.uk/Id/nhs-number");

            nhsNumberIdentifier.Value.Should().Be(inputNhsNumber);
            patient.Meta.Should().NotBeNull();
        }
    }
}
