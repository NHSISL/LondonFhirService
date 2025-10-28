// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Patients
{
    public partial class PatientServiceTests
    {
        [Fact]
        public async Task ShouldGetStructuredPatient()
        {
            // given
            List<string> randomProviderNames = new List<string>
            {
                GetRandomString(),
                GetRandomString()
            };

            List<string> inputProviderNames = randomProviderNames.DeepClone();
            Bundle randomBundle = CreateRandomBundle();
            Bundle outputBundle = randomBundle.DeepClone();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;

            foreach (string providerName in inputProviderNames)
            {
                this.fhirBrokerMock.Setup(broker =>
                    broker.Patients(providerName).Everything(
                        inputNhsNumber,
                        null,
                        null,
                        null,
                        null,
                        null))
                        .Returns(outputBundle);
            }

            List<Bundle> expectedBundles = new List<Bundle>
            {
                outputBundle,
                outputBundle
            };

            // when
            List<Bundle> actualBundles =
                await this.patientService.GetStructuredRecord(
                    providers: inputProviderNames,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            foreach (string providerName in inputProviderNames)
            {
                this.fhirBrokerMock.Verify(broker =>
                    broker.Patients(providerName).Everything(
                        inputNhsNumber,
                        null,
                        null,
                        null,
                        null,
                        null),
                            Times.Once);
            }

            this.fhirBrokerMock.VerifyNoOtherCalls();
        }
    }
}
