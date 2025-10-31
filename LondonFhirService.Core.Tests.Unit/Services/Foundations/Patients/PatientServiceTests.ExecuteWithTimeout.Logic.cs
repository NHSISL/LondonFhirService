﻿// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Patients
{
    public partial class PatientServiceTests
    {
        [Fact]
        public async Task ShouldExecuteWithTimeout()
        {
            // given
            Bundle randomBundle = CreateRandomBundle();
            Bundle outputBundle = randomBundle.DeepClone();
            string randomId = GetRandomString();
            string inputId = randomId;
            var fhirProvider = this.ddsFhirProviderMock.Object;

            (Bundle Bundle, Exception Exception) expectedResult = (outputBundle, null);

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
                await this.patientService.ExecuteWithTimeoutAsync(
                    fhirProvider.Patients,
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
        public async Task ShouldReturnNullAndOperationCancelledExceptionOnExecuteWithTimeoutWhenTokenCancelled()
        {
            // given
            Bundle randomBundle = CreateRandomBundle();
            Bundle outputBundle = randomBundle.DeepClone();
            string randomId = GetRandomString();
            string inputId = randomId;
            CancellationToken alreadyCanceledToken = new CancellationToken(true);
            OperationCanceledException operationCanceledException = new OperationCanceledException(alreadyCanceledToken);
            var fhirProvider = this.ddsFhirProviderMock.Object;

            (Bundle Bundle, Exception Exception) expectedResult = (null, operationCanceledException);

            // when
            (Bundle Bundle, Exception Exception) actualResult =
                await this.patientService.ExecuteWithTimeoutAsync(
                    fhirProvider.Patients,
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
        public async Task ShouldReturnNullAndOperationCancelledExceptionOnExecuteWithTimeoutWhenCancelled()
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
                await this.patientService.ExecuteWithTimeoutAsync(
                    fhirProvider.Patients,
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
        public async Task ShouldReturnNullAndExceptionOnExecuteWithTimeoutWhenException()
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
                await this.patientService.ExecuteWithTimeoutAsync(
                    fhirProvider.Patients,
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
        public async Task ShouldReturnNullAndTimeoutExceptionOnExecuteWithTimeoutWhenEverythingTimesOut()
        {
            // given
            var timeoutMilliseconds = 1;
            this.patientServiceConfig.MaxProviderWaitTimeMilliseconds = timeoutMilliseconds;
            string randomId = GetRandomString();
            string inputId = randomId;

            OperationCanceledException operationCanceledException =
                new OperationCanceledException("A task was canceled.");

            var timeoutException = new TimeoutException(
                $"Provider call exceeded {timeoutMilliseconds} milliseconds.",
                operationCanceledException);

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
                await this.patientService.ExecuteWithTimeoutAsync(
                    fhirProvider.Patients,
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
