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
            Bundle outputBundle = randomBundle.DeepClone();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            string inputFhirProviderName = GetRandomString();
            bool inputFhirProviderIsPrimary = true;
            var fhirProvider = this.ddsFhirProviderMock.Object;
            var fhirProviderCopy = this.ddsFhirProviderMock.Object.DeepClone();
            DateTime? inputDateOfBirth = DateTime.Now;
            bool? inputDemographicsOnly = false;
            bool? inputActivePatientsOnly = true;
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";
            string providerDisplayName = fhirProvider.DisplayName;

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{inputDateOfBirth}\", " +
                $"demographicsOnly = \"{inputDemographicsOnly}\", " +
                $"includeInactivePatients = \"{inputActivePatientsOnly}\" }}";

            string rawOutputBundle = this.fhirJsonSerializer.SerializeToString(outputBundle);
            string expectedBundle = rawOutputBundle;

            (string Bundle, Exception Exception) expectedResult = (expectedBundle, null);

            this.ddsFhirProviderMock.Setup(p => p.Patients.GetStructuredRecordSerialisedAsync(
                inputNhsNumber,
                inputDateOfBirth,
                inputDemographicsOnly,
                inputActivePatientsOnly,
                It.IsAny<CancellationToken>()))
                    .ReturnsAsync(rawOutputBundle);

            // when
            (string Bundle, Exception Exception) actualResult =
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
                    $"{providerDisplayName} Provider Execution Started",
                    message,
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    $"{auditType}-DATA",
                    $"{fhirProvider.DisplayName} - DATA",
                    rawOutputBundle,
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    $"{providerDisplayName} Provider Execution Completed",
                    message,
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.ddsFhirProviderMock.Verify(provider =>
                provider.DisplayName,
                    Times.AtLeastOnce);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.ddsFhirProviderMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
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

            (string Json, Exception Exception) expectedResult = (null, operationCanceledException);

            // when
            (string Json, Exception Exception) actualResult =
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

            (string Json, Exception Exception) expectedResult = (null, operationCanceledException);

            this.ddsFhirProviderMock.Setup(p => p.Patients.GetStructuredRecordSerialisedAsync(
                nhsNumber,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
                    .ThrowsAsync(operationCanceledException);

            // when
            (string Json, Exception Exception) actualResult =
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
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.ddsFhirProviderMock.Verify(provider =>
                provider.DisplayName,
                    Times.AtLeastOnce);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.ddsFhirProviderMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
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

            (string Json, Exception Exception) expectedResult = (null, exception);

            this.ddsFhirProviderMock.Setup(p => p.Patients.GetStructuredRecordSerialisedAsync(
                nhsNumber,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
                    .ThrowsAsync(exception);

            // when
            (string Json, Exception Exception) actualResult =
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
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.ddsFhirProviderMock.Verify(provider =>
                provider.DisplayName,
                    Times.AtLeastOnce);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.ddsFhirProviderMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
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
            (string Json, Exception Exception) actualResult =
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
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.ddsFhirProviderMock.Verify(provider =>
                provider.DisplayName,
                    Times.AtLeastOnce);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.ddsFhirProviderMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }
    }
}
