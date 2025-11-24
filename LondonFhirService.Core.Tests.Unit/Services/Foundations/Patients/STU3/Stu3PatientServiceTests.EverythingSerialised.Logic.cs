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
        public async Task EverythingSerialisedShouldReturnBundles()
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
            Bundle randomLdsBundle = CreateRandomBundle();
            Bundle outputLdsBundle = randomLdsBundle.DeepClone();
            string randomId = GetRandomString();
            string inputId = randomId;

            List<Bundle> expectedBundles = new List<Bundle>
            {
                outputDdsBundle,
                outputLdsBundle
            };

            string rawOutputDdsBundle = this.fhirJsonSerializer.SerializeToString(outputDdsBundle);
            string rawOutputLdsBundle = this.fhirJsonSerializer.SerializeToString(outputLdsBundle);

            List<string> expectedJson = new List<string>
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
                service.ExecuteEverythingSerialisedWithTimeoutAsync(
                    ddsFhirProviderMock.Object,
                    default,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((rawOutputDdsBundle, null));

            patientServiceMock.Setup(service =>
                service.ExecuteEverythingSerialisedWithTimeoutAsync(
                    ldsFhirProviderMock.Object,
                    default,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((rawOutputLdsBundle, null));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<string> actualJson =
                await mockedPatientService.EverythingSerialisedAsync(
                    providerNames: inputProviderNames,
                    id: inputId,
                    cancellationToken: default);

            // then
            actualJson.Should().BeEquivalentTo(expectedJson);

            patientServiceMock.Verify(service =>
                service.ExecuteEverythingSerialisedWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    default,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteEverythingSerialisedWithTimeoutAsync(
                    this.ldsFhirProviderMock.Object,
                    default,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null),
                        Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }
    }
}
