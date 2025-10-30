// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Services.Foundations.Patients;
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
                "DDS",
                "LDS"
            };

            List<string> inputProviderNames = randomProviderNames.DeepClone();
            Bundle randomBundle = CreateRandomBundle();
            Bundle outputBundle = randomBundle.DeepClone();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;

            List<Bundle> expectedBundles = new List<Bundle>
            {
                outputBundle,
                outputBundle
            };

            var patientServiceMock = new Mock<PatientService>(
                this.fhirBroker,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig);

            foreach (var provider in fhirBroker.FhirProviders)
            {
                patientServiceMock.Setup(service =>
                    service.ExecuteWithTimeoutAsync(
                        provider.Patients,
                        default,
                        inputNhsNumber,
                        null,
                        null,
                        null,
                        null,
                        null))
                    .ReturnsAsync((outputBundle, null));
            }

            PatientService patientService = patientServiceMock.Object;

            // when
            List<Bundle> actualBundles =
                await patientService.Everything(
                    providerNames: inputProviderNames,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            foreach (var provider in fhirBroker.FhirProviders)
            {
                patientServiceMock.Verify(service =>
                    service.ExecuteWithTimeoutAsync(
                        provider.Patients,
                        default,
                        inputNhsNumber,
                        null,
                        null,
                        null,
                        null,
                        null),
                            Times.Once());
            }

            this.loggingBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }
    }
}
