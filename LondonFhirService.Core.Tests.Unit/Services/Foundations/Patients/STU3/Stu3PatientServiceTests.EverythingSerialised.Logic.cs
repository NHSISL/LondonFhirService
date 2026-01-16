// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
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
            DateTimeOffset? inputStart = GetRandomDateTimeOffset();
            DateTimeOffset? inputEnd = GetRandomDateTimeOffset();
            string inputTypeFilter = GetRandomString();
            DateTimeOffset? inputSince = GetRandomDateTimeOffset();
            int? inputCount = GetRandomNumber();
            Guid correlationId = Guid.NewGuid();
            CancellationToken cancellationToken = default;
            string auditType = "STU3-Patient-EverythingSerialised";

            string message =
                $"Parameters:  {{ id = \"{inputId}\", start = \"{inputStart}\", " +
                $"end = \"{inputEnd}\", typeFilter = \"{inputTypeFilter}\", " +
                $"since = \"{inputSince}\", count = \"{inputCount}\" }}";

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
                this.auditBrokerMock.Object,
                this.identifierBrokerMock.Object,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.ExecuteEverythingSerialisedWithTimeoutAsync(
                    ddsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputId,
                    inputStart,
                    inputEnd,
                    inputTypeFilter,
                    inputSince,
                    inputCount))
                .ReturnsAsync((rawOutputDdsBundle, null));

            patientServiceMock.Setup(service =>
                service.ExecuteEverythingSerialisedWithTimeoutAsync(
                    ldsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputId,
                    inputStart,
                    inputEnd,
                    inputTypeFilter,
                    inputSince,
                    inputCount))
                .ReturnsAsync((rawOutputLdsBundle, null));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<string> actualJson =
                await mockedPatientService.EverythingSerialisedAsync(
                    providerNames: inputProviderNames,
                    correlationId: correlationId,
                    id: inputId,
                    start: inputStart,
                    end: inputEnd,
                    typeFilter: inputTypeFilter,
                    since: inputSince,
                    count: inputCount,
                    cancellationToken: cancellationToken);

            // then
            actualJson.Should().BeEquivalentTo(expectedJson);

            patientServiceMock.Verify(service =>
                service.ExecuteEverythingSerialisedWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputId,
                    inputStart,
                    inputEnd,
                    inputTypeFilter,
                    inputSince,
                    inputCount),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteEverythingSerialisedWithTimeoutAsync(
                    this.ldsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputId,
                    inputStart,
                    inputEnd,
                    inputTypeFilter,
                    inputSince,
                    inputCount),
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
