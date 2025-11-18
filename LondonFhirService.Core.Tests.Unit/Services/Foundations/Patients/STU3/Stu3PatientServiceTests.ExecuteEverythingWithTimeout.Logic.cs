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
            var fhirProvider = this.ddsFhirProviderMock.Object;
            var fhirProviderCopy = this.ddsFhirProviderMock.Object.DeepClone();

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

            (Bundle Bundle, Exception Exception) expectedResult = (expectedBundle, null);

            this.ddsFhirProviderMock.Setup(p => p.Patients.Everything(
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
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null);

            // then
            actualResult.Should().BeEquivalentTo(expectedResult);

            this.ddsFhirProviderMock.Verify(p => p.Patients.Everything(
                inputId,
                null,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()),
                    Times.Once());

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
        }

        [Fact]
        public async Task
            ShouldReturnNullAndOperationCancelledExceptionOnExecuteEverythingWithTimeoutWhenTokenCancelled()
        {
            // given
            Bundle randomBundle = CreateRandomBundle();
            string randomId = GetRandomString();
            string inputId = randomId;
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
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null);

            // then
            actualResult.Should().BeEquivalentTo(expectedResult);

            this.ddsFhirProviderMock.Verify(p => p.Patients.Everything(
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
        }

        [Fact]
        public async Task ShouldReturnNullAndOperationCancelledExceptionOnExecuteEverythingWithTimeoutWhenCancelled()
        {
            // given
            string randomId = GetRandomString();
            string inputId = randomId;
            OperationCanceledException operationCanceledException = new OperationCanceledException();
            var fhirProvider = this.ddsFhirProviderMock.Object;

            (Bundle Bundle, Exception Exception) expectedResult = (null, operationCanceledException);

            this.ddsFhirProviderMock.Setup(p => p.Patients.Everything(
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
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null);

            // then
            actualResult.Should().BeEquivalentTo(expectedResult);

            this.ddsFhirProviderMock.Verify(p => p.Patients.Everything(
                inputId,
                null,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()),
                    Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.ddsFhirProviderMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullAndExceptionOnExecuteEverythingWithTimeoutWhenException()
        {
            // given
            string randomId = GetRandomString();
            string inputId = randomId;
            Exception exception = new Exception(GetRandomString());
            var fhirProvider = this.ddsFhirProviderMock.Object;

            (Bundle Bundle, Exception Exception) expectedResult = (null, exception);

            this.ddsFhirProviderMock.Setup(p => p.Patients.Everything(
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
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null);

            // then
            actualResult.Should().BeEquivalentTo(expectedResult);

            this.ddsFhirProviderMock.Verify(p => p.Patients.Everything(
                inputId,
                null,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()),
                    Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.ddsFhirProviderMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullAndTimeoutExceptionOnExecuteEverythingWithTimeoutWhenEverythingTimesOut()
        {
            // given
            var timeoutMilliseconds = 1;
            this.patientServiceConfig.MaxProviderWaitTimeMilliseconds = timeoutMilliseconds;
            string randomId = GetRandomString();
            string inputId = randomId;

            OperationCanceledException operationCanceledException =
                new OperationCanceledException("A task was canceled.");

            var fhirProvider = this.ddsFhirProviderMock.Object;

            this.ddsFhirProviderMock.Setup(p => p.Patients.Everything(
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

            this.ddsFhirProviderMock.Verify(p => p.Patients.Everything(
                inputId,
                null,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()),
                    Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.ddsFhirProviderMock.VerifyNoOtherCalls();
        }
    }
}
