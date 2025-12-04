// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Services.Foundations.Patients.STU3;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Patients.STU3
{
    public partial class Stu3PatientServiceTests
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
            Guid correlationId = Guid.NewGuid();

            List<Bundle> expectedBundles = new List<Bundle>
            {
                outputDdsBundle,
                outputLdsBundle
            };

            var patientServiceMock = new Mock<Stu3PatientService>(
                this.fhirBroker,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.ExecuteEverythingWithTimeoutAsync(
                    ddsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((outputDdsBundle, null));

            patientServiceMock.Setup(service =>
                service.ExecuteEverythingWithTimeoutAsync(
                    ldsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((outputLdsBundle, null));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<Bundle> actualBundles =
                await mockedPatientService.EverythingAsync(
                    providerNames: inputProviderNames,
                    correlationId: correlationId,
                    id: inputId,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteEverythingWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteEverythingWithTimeoutAsync(
                    this.ldsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null),
                        Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }
    }
}
