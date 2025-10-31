﻿// ---------------------------------------------------------
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
        public async Task EverythingShouldReturnBundles()
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

            var patientServiceMock = new Mock<PatientService>(
                this.fhirBroker,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.ExecuteWithTimeoutAsync(
                    ddsFhirProviderMock.Object.Patients,
                    default,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((outputDdsBundle, null));

            patientServiceMock.Setup(service =>
                service.ExecuteWithTimeoutAsync(
                    ldsFhirProviderMock.Object.Patients,
                    default,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((outputLdsBundle, null));

            PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<Bundle> actualBundles =
                await mockedPatientService.Everything(
                    providerNames: inputProviderNames,
                    id: inputId,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object.Patients,
                    default,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteWithTimeoutAsync(
                    this.ldsFhirProviderMock.Object.Patients,
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
