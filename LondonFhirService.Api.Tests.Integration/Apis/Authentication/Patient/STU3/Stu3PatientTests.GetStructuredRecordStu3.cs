// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using FluentAssertions;
using Hl7.Fhir.Model;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Api.Tests.Integration.Apis.Authentication.Patient.STU3
{
    public partial class Stu3PatientTests
    {
        [Fact]
        public async Task ShouldGetStructuredRecordStu3Async()
        {
            // given
            string inputNhsNumber = "9435797881";

            Parameters inputParametersWithDemographicsOnly = CreateRandomGetStructuredRecordParameters(
                nhsNumber: inputNhsNumber,
                demographicsOnly: true);

            Parameters inputParameters = CreateRandomGetStructuredRecordParameters(
                nhsNumber: inputNhsNumber,
                demographicsOnly: false);

            // when
            var (actualText, actualBundle) =
                await this.authApiBroker.GetStructuredRecordStu3Async(inputNhsNumber, inputParameters);

            var (actualTextWithDemographics, actualBundleWithDemographics) =
                await this.authApiBroker.GetStructuredRecordStu3Async(inputNhsNumber, inputParametersWithDemographicsOnly);

            // then
            int actualTextLength = actualText.Length;
            int actualTextWithDemographicsOnlyLength = actualTextWithDemographics.Length;
            actualTextLength.Should().BeGreaterThan(actualTextWithDemographicsOnlyLength);
            output.WriteLine($"Actual Text: {actualTextLength}");
            output.WriteLine($"Actual Text With Demographics Only: {actualTextWithDemographicsOnlyLength}");
        }
    }
}
