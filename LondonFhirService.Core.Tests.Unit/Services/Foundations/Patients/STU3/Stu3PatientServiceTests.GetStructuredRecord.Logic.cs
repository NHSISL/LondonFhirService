// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Services.Foundations.Patients.STU3;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Patients.STU3
{
    public partial class Stu3PatientServiceTests
    {
        [Fact]
        public async Task GetStructuredRecordShouldReturnBundles()
        {
            // given
            Provider ddsProvider =
                new Provider { FriendlyName = "DDS Provider", FullyQualifiedName = "DDS", IsPrimary = true };

            Provider ldsProvider =
                new Provider { FriendlyName = "LDS Provider", FullyQualifiedName = "LDS", IsPrimary = false };

            List<Provider> randomProviders = new List<Provider>
            {
                ddsProvider,
                ldsProvider,
            };

            List<Provider> inputProviders = randomProviders.DeepClone();
            Bundle randomDdsBundle = CreateRandomBundle();
            Bundle outputDdsBundle = randomDdsBundle.DeepClone();
            Bundle randomLdsBundle = CreateRandomBundle();
            Bundle outputLdsBundle = randomLdsBundle.DeepClone();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            DateTime? inputDateOfBirth = DateTime.Now;
            bool? inputDemographicsOnly = false;
            bool? inputActivePatientsOnly = true;
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecord";

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{inputDateOfBirth}\", " +
                $"demographicsOnly = \"{inputDemographicsOnly}\", " +
                $"includeInactivePatients = \"{inputActivePatientsOnly}\" }}";

            List<Bundle> expectedBundles = new List<Bundle>
            {
                outputDdsBundle,
                outputLdsBundle
            };

            var patientServiceMock = new Mock<Stu3PatientService>(
                this.fhirBroker,
                this.auditBrokerMock.Object,
                this.identifierBrokerMock.Object,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordWithTimeoutAsync(
                    ddsProvider.FriendlyName,
                    ddsProvider.IsPrimary,
                    ddsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly))
                .ReturnsAsync((outputDdsBundle, null));

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordWithTimeoutAsync(
                    ldsProvider.FriendlyName,
                    ldsProvider.IsPrimary,
                    ldsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly))
                .ReturnsAsync((outputLdsBundle, null));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<Bundle> actualBundles =
                await mockedPatientService.GetStructuredRecordAsync(
                    activeProviders: inputProviders,
                    correlationId: correlationId,
                    nhsNumber: inputNhsNumber,
                    dateOfBirth: inputDateOfBirth,
                    demographicsOnly: inputDemographicsOnly,
                    includeInactivePatients: inputActivePatientsOnly,
                    cancellationToken: cancellationToken);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordWithTimeoutAsync(
                    ddsProvider.FriendlyName,
                    ddsProvider.IsPrimary,
                    this.ddsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordWithTimeoutAsync(
                    ldsProvider.FriendlyName,
                    ldsProvider.IsPrimary,
                    this.ldsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly),
                        Times.Once());

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Foundation Service Request Submitted",
                    message,
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Parallel Provider Execution Started",
                    message,
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Parallel Provider Execution Completed",
                    message,
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Foundation Service Request Completed",
                    message,
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }
    }
}
