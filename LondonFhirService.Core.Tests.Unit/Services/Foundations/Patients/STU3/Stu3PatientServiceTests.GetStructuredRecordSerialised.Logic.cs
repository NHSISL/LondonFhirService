// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using LondonFhirService.Core.Services.Foundations.Patients.STU3;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Patients.STU3
{
    public partial class Stu3PatientServiceTests
    {
        [Fact]
        public async Task GetStructuredRecordSerialisedShouldReturnBundles()
        {
            // given
            List<string> randomProviderNames = new List<string>
            {
                "DDS",
                "LDS"
            };

            List<string> inputProviderNames = randomProviderNames.DeepClone();
            Bundle randomDdsBundle = CreateRandomBundle();
            Bundle outputDdsBundle = randomDdsBundle.DeepClone();
            string rawOutputDdsBundle = this.fhirJsonSerializer.SerializeToString(outputDdsBundle);
            Bundle randomLdsBundle = CreateRandomBundle();
            Bundle outputLdsBundle = randomLdsBundle.DeepClone();
            string rawOutputLdsBundle = this.fhirJsonSerializer.SerializeToString(outputLdsBundle);
            string randomNhsNumber = GetRandomString();
            string inputnhsNumber = randomNhsNumber;

            List<string> expectedBundles = new List<string>
            {
                rawOutputDdsBundle,
                rawOutputLdsBundle
            };

            var patientServiceMock = new Mock<Stu3PatientService>(
                this.fhirBroker,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ddsFhirProviderMock.Object,
                    default,
                    inputnhsNumber,
                    null,
                    null,
                    null))
                .ReturnsAsync((rawOutputDdsBundle, null));

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ldsFhirProviderMock.Object,
                    default,
                    inputnhsNumber,
                    null,
                    null,
                    null))
                .ReturnsAsync((rawOutputLdsBundle, null));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<string> actualBundles =
                await mockedPatientService.GetStructuredRecordSerialisedAsync(
                    providerNames: inputProviderNames,
                    nhsNumber: inputnhsNumber,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    default,
                    inputnhsNumber,
                    null,
                    null,
                    null),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    this.ldsFhirProviderMock.Object,
                    default,
                    inputnhsNumber,
                    null,
                    null,
                    null),
                        Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }
    }
}
