// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
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
        public async Task ShouldExecuteEverythingWithTimeout()
        {
            // given
            Bundle randomBundle = CreateRandomBundle();
            Bundle outputBundle = randomBundle.DeepClone();
            Bundle expectedBundle = outputBundle.DeepClone();
            string randomId = GetRandomString();
            string inputId = randomId;
            Guid correlationId = Guid.NewGuid();
            var fhirProvider = this.ddsFhirProviderMock.Object;
            var fhirProviderCopy = this.ddsFhirProviderMock.Object.DeepClone();
            string auditType = "STU3-Patient-Everything";

            string message =
                $"Parameters:  {{ id = \"{inputId}\", start = \"{null}\", " +
                $"end = \"{null}\", typeFilter = \"{null}\", " +
                $"since = \"{null}\", count = \"{null}\" }}";

            expectedBundle.Meta.Extension = new List<Extension>
            {
                new Extension
                {
                    Url = "http://example.org/fhir/StructureDefinition/meta-source",
                    Value = new FhirUri(fhirProviderCopy.Source)
                }
            };

            expectedBundle.Meta.Tag = new List<Coding>
            {
                new Coding
                {
                    System = fhirProviderCopy.System,
                    Code = fhirProviderCopy.Code,
                    Display = fhirProviderCopy.ProviderName
                }
            };

            string json = fhirJsonSerializer.SerializeToString(expectedBundle);

            (Bundle Bundle, Exception Exception) expectedResult = (expectedBundle, null);

            this.ddsFhirProviderMock.Setup(p => p.Patients.EverythingAsync(
                inputId,
                null,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
                    .ReturnsAsync(outputBundle);

            // when
            (Bundle Bundle, Exception Exception) actualResult =
                await this.patientService.ExecuteEverythingWithTimeoutAsync(
                    fhirProvider,
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null);

            // then
            actualResult.Should().BeEquivalentTo(expectedResult);

            this.ddsFhirProviderMock.Verify(p => p.Patients.EverythingAsync(
                inputId,
                null,
                null,
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

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    $"{fhirProvider.DisplayName} - DATA",
                    json,
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    $"{fhirProvider.DisplayName} Provider Execution Completed",
                    message,
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.ddsFhirProviderMock.Verify(provider =>
                provider.DisplayName,
                    Times.AtLeastOnce);

            this.ddsFhirProviderMock.Verify(provider =>
                provider.System,
                    Times.Once);

            this.ddsFhirProviderMock.Verify(provider =>
                provider.Code,
                    Times.Once);

            this.ddsFhirProviderMock.Verify(provider =>
                provider.ProviderName,
                    Times.Once);

            this.ddsFhirProviderMock.Verify(provider =>
                provider.Source,
                    Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.ddsFhirProviderMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task
            ShouldReturnNullAndOperationCancelledExceptionOnExecuteEverythingWithTimeoutWhenTokenCancelled()
        {
            // given
            Bundle randomBundle = CreateRandomBundle();
            string randomId = GetRandomString();
            string inputId = randomId;
            Guid correlationId = Guid.NewGuid();
            CancellationToken alreadyCanceledToken = new CancellationToken(true);

            OperationCanceledException operationCanceledException =
                new OperationCanceledException(alreadyCanceledToken);

            var fhirProvider = this.ddsFhirProviderMock.Object;

            (Bundle Bundle, Exception Exception) expectedResult = (null, operationCanceledException);

            // when
            (Bundle Bundle, Exception Exception) actualResult =
                await this.patientService.ExecuteEverythingWithTimeoutAsync(
                    fhirProvider,
                    alreadyCanceledToken,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null);

            // then
            actualResult.Should().BeEquivalentTo(expectedResult);

            this.ddsFhirProviderMock.Verify(p => p.Patients.EverythingAsync(
                inputId,
                null,
                null,
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
        public async Task ShouldReturnNullAndOperationCancelledExceptionOnExecuteEverythingWithTimeoutWhenCancelled()
        {
            // given
            string randomId = GetRandomString();
            string inputId = randomId;
            Guid correlationId = Guid.NewGuid();
            OperationCanceledException operationCanceledException = new OperationCanceledException();
            var fhirProvider = this.ddsFhirProviderMock.Object;
            (Bundle Bundle, Exception Exception) expectedResult = (null, operationCanceledException);

            this.ddsFhirProviderMock.Setup(p => p.Patients.EverythingAsync(
                inputId,
                null,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
                    .ThrowsAsync(operationCanceledException);

            // when
            (Bundle Bundle, Exception Exception) actualResult =
                await this.patientService.ExecuteEverythingWithTimeoutAsync(
                    fhirProvider,
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null);

            // then
            actualResult.Should().BeEquivalentTo(expectedResult);

            this.ddsFhirProviderMock.Verify(p => p.Patients.EverythingAsync(
                inputId,
                null,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()),
                    Times.Once());

            this.ddsFhirProviderMock.Verify(provider =>
                provider.DisplayName,
                    Times.AtLeastOnce);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.ddsFhirProviderMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullAndExceptionOnExecuteEverythingWithTimeoutWhenException()
        {
            // given
            string randomId = GetRandomString();
            string inputId = randomId;
            Exception exception = new Exception(GetRandomString());
            Guid correlationId = Guid.NewGuid();
            var fhirProvider = this.ddsFhirProviderMock.Object;
            string auditType = "STU3-Patient-Everything";

            string message =
                $"Parameters:  {{ id = \"{inputId}\", start = \"{null}\", " +
                $"end = \"{null}\", typeFilter = \"{null}\", " +
                $"since = \"{null}\", count = \"{null}\" }}";

            (Bundle Bundle, Exception Exception) expectedResult = (null, exception);

            this.ddsFhirProviderMock.Setup(p => p.Patients.EverythingAsync(
                inputId,
                null,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
                    .ThrowsAsync(exception);

            // when
            (Bundle Bundle, Exception Exception) actualResult =
                await this.patientService.ExecuteEverythingWithTimeoutAsync(
                    fhirProvider,
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null);

            // then
            actualResult.Should().BeEquivalentTo(expectedResult);

            this.ddsFhirProviderMock.Verify(p => p.Patients.EverythingAsync(
                inputId,
                null,
                null,
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
        public async Task ShouldReturnNullAndTimeoutExceptionOnExecuteEverythingWithTimeoutWhenEverythingTimesOut()
        {
            // given
            var timeoutMilliseconds = 1;
            this.patientServiceConfig.MaxProviderWaitTimeMilliseconds = timeoutMilliseconds;
            string randomId = GetRandomString();
            string inputId = randomId;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-Everything";

            string message =
                $"Parameters:  {{ id = \"{inputId}\", start = \"{null}\", " +
                $"end = \"{null}\", typeFilter = \"{null}\", " +
                $"since = \"{null}\", count = \"{null}\" }}";

            OperationCanceledException operationCanceledException =
                new OperationCanceledException("A task was canceled.");

            var fhirProvider = this.ddsFhirProviderMock.Object;

            this.ddsFhirProviderMock.Setup(p => p.Patients.EverythingAsync(
                inputId,
                null,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
                     .Returns(async (string id,
                        DateTimeOffset? start,
                        DateTimeOffset? end,
                        string typeFiler,
                        DateTimeOffset? since,
                        int? count,
                        CancellationToken token) =>
                         {
                             await Task.Delay(Timeout.Infinite, token);
                             return default(Bundle);
                         });

            // when
            (Bundle Bundle, Exception Exception) actualResult =
                await this.patientService.ExecuteEverythingWithTimeoutAsync(
                    fhirProvider,
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null);

            // then
            actualResult.Bundle.Should().BeNull();
            actualResult.Exception.Should().BeOfType<TimeoutException>();

            actualResult.Exception.Message.Should().Be(
                $"Provider call exceeded {timeoutMilliseconds} milliseconds.");

            actualResult.Exception.InnerException.Should().BeOfType<TaskCanceledException>();

            this.ddsFhirProviderMock.Verify(p => p.Patients.EverythingAsync(
                inputId,
                null,
                null,
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
