// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Patients.STU3
{
    public partial class Stu3PatientServiceTests
    {
        [Fact]
        public async Task ShouldExecuteGetStructuredRecordSerialisedWithTimeout()
        {
            // given
            Bundle randomBundle = CreateRandomBundle();
            bool inputFhirProviderIsPrimary = true;
            var fhirProvider = this.ddsFhirProviderMock.Object;
            var fhirProviderCopy = this.ddsFhirProviderMock.Object.DeepClone();
            Bundle outputBundle = randomBundle.DeepClone();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            string inputFhirProviderName = "DDS Test Provider";
            string inputDateOfBirth = DateTime.Now.ToString("yyyy-MM-dd");
            bool? inputDemographicsOnly = false;
            bool? inputActivePatientsOnly = true;
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";
            Guid identifier = Guid.NewGuid();

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{inputDateOfBirth}\", " +
                $"demographicsOnly = \"{inputDemographicsOnly}\", " +
                $"includeInactivePatients = \"{inputActivePatientsOnly}\" }}";

            string rawOutputJson = this.fhirJsonSerializer.SerializeToString(outputBundle);
            string expectedJson = rawOutputJson;

            (string Provider, string Json, Exception Exception) expectedResult =
                (inputFhirProviderName, expectedJson, null);

            this.identifierBrokerMock.Setup(broker =>
                broker.GetIdentifierAsync())
                    .ReturnsAsync(identifier);

            FhirRecord fhirRecord = new()
            {
                Id = identifier,
                CorrelationId = correlationId.ToString(),
                JsonPayload = rawOutputJson,
                SourceName = $"{fhirProvider.DisplayName} ({inputFhirProviderName})",
                IsPrimarySource = inputFhirProviderIsPrimary,
                IsProcessed = false,
            };

            FhirRecord fhirRecordPostAudit = fhirRecord.DeepClone();

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.Is(SameFhirRecordAs(fhirRecord))))
                    .ReturnsAsync(fhirRecordPostAudit);

            this.ddsFhirProviderMock.Setup(p => p.Patients.GetStructuredRecordSerialisedAsync(
                inputNhsNumber,
                inputDateOfBirth,
                inputDemographicsOnly,
                inputActivePatientsOnly,
                It.IsAny<CancellationToken>()))
                    .ReturnsAsync(rawOutputJson);

            // when
            (string Provider, string Json, Exception Exception) actualResult =
                await this.patientService.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    inputFhirProviderName,
                    inputFhirProviderIsPrimary,
                    fhirProvider,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly);

            // then
            actualResult.Should().BeEquivalentTo(expectedResult);

            this.ddsFhirProviderMock.Verify(p => p.Patients.GetStructuredRecordSerialisedAsync(
                inputNhsNumber,
                inputDateOfBirth,
                inputDemographicsOnly,
                inputActivePatientsOnly,
                It.IsAny<CancellationToken>()),
                    Times.Once());

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    $"{fhirProvider.DisplayName} Provider Execution Started",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    $"{auditType}-DATA",
                    $"{fhirProvider.DisplayName} - DATA ({inputFhirProviderName})",
                    rawOutputJson,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    It.Is<string>(s => s.StartsWith($"{fhirProvider.DisplayName} Provider Execution Completed")),
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.ddsFhirProviderMock.Verify(provider =>
                provider.DisplayName,
                    Times.AtLeastOnce);

            this.identifierBrokerMock.Verify(broker =>
                broker.GetIdentifierAsync(),
                    Times.Once);

            this.securityAuditBrokerMock.Verify(broker =>
                broker.ApplyAddAuditValuesAsync(It.Is(SameFhirRecordAs(fhirRecord))),
                    Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.InsertFhirRecordAsync(It.Is(SameFhirRecordAs(fhirRecord))),
                    Times.Once);

            this.storageBrokerFactoryMock.Verify(factory =>
                factory.CreateStorageBrokerAsync(),
                    Times.Once);

            this.storageBrokerMock.Verify(factory =>
                factory.DisposeAsync(),
                    Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.ddsFhirProviderMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task
            ShouldReturnNullAndOperationCancelledExceptionOnExecuteGetStructuredRecordSerialisedWithTimeoutWhenTokenCancelled()
        {
            // given
            Bundle randomBundle = CreateRandomBundle();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            CancellationToken alreadyCanceledToken = new CancellationToken(true);
            Guid correlationId = Guid.NewGuid();

            OperationCanceledException operationCanceledException =
                new OperationCanceledException(alreadyCanceledToken);

            string inputFhirProviderName = GetRandomString();
            bool inputFhirProviderIsPrimary = true;
            var fhirProvider = this.ddsFhirProviderMock.Object;

            (string Provider, string Json, Exception Exception) expectedResult =
                (inputFhirProviderName, null, operationCanceledException);

            // when
            (string Provider, string Json, Exception Exception) actualResult =
                await this.patientService.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    inputFhirProviderName,
                    inputFhirProviderIsPrimary,
                    fhirProvider,
                    alreadyCanceledToken,
                    correlationId,
                    inputNhsNumber,
                    null,
                    null,
                    null);

            // then
            actualResult.Should().BeEquivalentTo(expectedResult);

            this.ddsFhirProviderMock.Verify(p => p.Patients.GetStructuredRecordAsync(
                inputNhsNumber,
                null,
                null,
                null,
                alreadyCanceledToken),
                    Times.Never());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.ddsFhirProviderMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task
            ShouldReturnNullAndOperationCancelledExceptionOnExecuteGetStructuredRecordSerialisedWithTimeoutWhenCancelled()
        {
            // given
            string randomId = GetRandomString();
            string nhsNumber = randomId;
            OperationCanceledException operationCanceledException = new OperationCanceledException();
            var fhirProvider = this.ddsFhirProviderMock.Object;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";
            string inputFhirProviderName = GetRandomString();
            bool inputFhirProviderIsPrimary = true;

            string message =
                $"Parameters:  {{ nhsNumber = \"{nhsNumber}\", dateOfBirth = \"{null}\", " +
                $"demographicsOnly = \"{null}\", " +
                $"includeInactivePatients = \"{null}\" }}";

            (string Provider, string Json, Exception Exception) expectedResult =
                (inputFhirProviderName, null, operationCanceledException);

            this.ddsFhirProviderMock.Setup(p => p.Patients.GetStructuredRecordSerialisedAsync(
                nhsNumber,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
                    .ThrowsAsync(operationCanceledException);

            // when
            (string Provider, string Json, Exception Exception) actualResult =
                await this.patientService.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    inputFhirProviderName,
                    inputFhirProviderIsPrimary,
                    fhirProvider,
                    default,
                    correlationId,
                    nhsNumber,
                    null,
                    null,
                    null);

            // then
            actualResult.Should().BeEquivalentTo(expectedResult);

            this.ddsFhirProviderMock.Verify(p => p.Patients.GetStructuredRecordSerialisedAsync(
                nhsNumber,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()),
                    Times.Once());

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    $"{fhirProvider.DisplayName} Provider Execution Started",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.ddsFhirProviderMock.Verify(provider =>
                provider.DisplayName,
                    Times.AtLeastOnce);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.ddsFhirProviderMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullAndExceptionOnExecuteGetStructuredRecordSerialisedWithTimeoutWhenException()
        {
            // given
            string randomId = GetRandomString();
            string nhsNumber = randomId;
            Exception exception = new Exception(GetRandomString());
            var fhirProvider = this.ddsFhirProviderMock.Object;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";
            string inputFhirProviderName = GetRandomString();
            bool inputFhirProviderIsPrimary = true;

            string message =
                $"Parameters:  {{ nhsNumber = \"{nhsNumber}\", dateOfBirth = \"{null}\", " +
                $"demographicsOnly = \"{null}\", " +
                $"includeInactivePatients = \"{null}\" }}";

            (string Provider, string Json, Exception Exception) expectedResult =
                (inputFhirProviderName, null, exception);

            this.ddsFhirProviderMock.Setup(p => p.Patients.GetStructuredRecordSerialisedAsync(
                nhsNumber,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
                    .ThrowsAsync(exception);

            // when
            (string Provider, string Json, Exception Exception) actualResult =
                await this.patientService.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    inputFhirProviderName,
                    inputFhirProviderIsPrimary,
                    fhirProvider,
                    default,
                    correlationId,
                    nhsNumber,
                    null,
                    null,
                    null);

            // then
            actualResult.Should().BeEquivalentTo(expectedResult);

            this.ddsFhirProviderMock.Verify(p => p.Patients.GetStructuredRecordSerialisedAsync(
                nhsNumber,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()),
                    Times.Once());

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    $"{fhirProvider.DisplayName} Provider Execution Started",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    $"Parallel Provider Execution - {fhirProvider.DisplayName} failed",
                    It.IsAny<string>(),
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.ddsFhirProviderMock.Verify(provider =>
                provider.DisplayName,
                    Times.AtLeastOnce);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.ddsFhirProviderMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task
            ShouldReturnNullAndTimeoutExceptionOnExecuteGetStructuredRecordSerialisedWithTimeoutWhenEverythingTimesOut()
        {
            // given
            var timeoutMilliseconds = 1;
            this.patientServiceConfig.MaxProviderWaitTimeMilliseconds = timeoutMilliseconds;
            string randomId = GetRandomString();
            string nhsNumber = randomId;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";
            string inputFhirProviderName = GetRandomString();
            bool inputFhirProviderIsPrimary = true;

            string message =
                $"Parameters:  {{ nhsNumber = \"{nhsNumber}\", dateOfBirth = \"{null}\", " +
                $"demographicsOnly = \"{null}\", " +
                $"includeInactivePatients = \"{null}\" }}";

            OperationCanceledException operationCanceledException =
                new OperationCanceledException("A task was canceled.");

            var fhirProvider = this.ddsFhirProviderMock.Object;

            this.ddsFhirProviderMock.Setup(p => p.Patients.GetStructuredRecordSerialisedAsync(
                nhsNumber,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
                     .Returns(async
                        (string nhsNumber,
                        DateTime? dateOfBirth,
                        bool? demographicsOnly,
                        bool? includeInactivePatients,
                        CancellationToken token) =>
                         {
                             await Task.Delay(Timeout.Infinite, token);
                             return default(string);
                         });

            // when
            (string Provider, string Json, Exception Exception) actualResult =
                await this.patientService.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    inputFhirProviderName,
                    inputFhirProviderIsPrimary,
                    fhirProvider,
                    default,
                    correlationId,
                    nhsNumber,
                    null,
                    null,
                    null);

            // then
            actualResult.Json.Should().BeNull();
            actualResult.Exception.Should().BeOfType<TimeoutException>();

            actualResult.Exception.Message.Should().Be(
                $"Provider call exceeded {timeoutMilliseconds} milliseconds.");

            actualResult.Exception.InnerException.Should().BeOfType<TaskCanceledException>();

            this.ddsFhirProviderMock.Verify(p => p.Patients.GetStructuredRecordSerialisedAsync(
                nhsNumber,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()),
                    Times.Once());

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    $"{fhirProvider.DisplayName} Provider Execution Started",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.ddsFhirProviderMock.Verify(provider =>
                provider.DisplayName,
                    Times.AtLeastOnce);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.ddsFhirProviderMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerFactoryMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
