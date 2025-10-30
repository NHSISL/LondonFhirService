// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
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
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            var fhirProvider = this.ddsFhirProviderMock.Object;

            (Bundle Bundle, Exception Exception) expectedResult = (outputBundle, null);

            this.ddsFhirProviderMock.Setup(p => p.Patients.Everything(
                inputNhsNumber,
                null,
                null,
                null,
                null,
                null,
                default))
                    .ReturnsAsync(outputBundle);

            // when
            (Bundle Bundle, Exception Exception) actualResult =
                await this.patientService.ExecuteWithTimeoutAsync(
                    fhirProvider.Patients,
                    default,
                    inputNhsNumber,
                    null,
                    null,
                    null,
                    null,
                    null);

            // then
            actualResult.Should().BeEquivalentTo(expectedResult);

            this.ddsFhirProviderMock.Verify(p => p.Patients.Everything(
                inputNhsNumber,
                null,
                null,
                null,
                null,
                null,
                default),
                    Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.ddsFhirProviderMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullAndOperationCancelledExceptionOnExecuteWithTimeoutWhenCancelled()
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            OperationCanceledException operationCanceledException = new OperationCanceledException();
            var fhirProvider = this.ddsFhirProviderMock.Object;

            (Bundle Bundle, Exception Exception) expectedResult = (null, operationCanceledException);

            this.ddsFhirProviderMock.Setup(p => p.Patients.Everything(
                inputNhsNumber,
                null,
                null,
                null,
                null,
                null,
                default))
                    .ThrowsAsync(operationCanceledException);

            // when
            (Bundle Bundle, Exception Exception) actualResult =
                await this.patientService.ExecuteWithTimeoutAsync(
                    fhirProvider.Patients,
                    default,
                    inputNhsNumber,
                    null,
                    null,
                    null,
                    null,
                    null);

            // then
            actualResult.Should().BeEquivalentTo(expectedResult);

            this.ddsFhirProviderMock.Verify(p => p.Patients.Everything(
                inputNhsNumber,
                null,
                null,
                null,
                null,
                null,
                default),
                    Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.ddsFhirProviderMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullAndExceptionOnExecuteWithTimeoutWhenException()
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            Exception exception = new Exception(GetRandomString());
            var fhirProvider = this.ddsFhirProviderMock.Object;

            (Bundle Bundle, Exception Exception) expectedResult = (null, exception);

            this.ddsFhirProviderMock.Setup(p => p.Patients.Everything(
                inputNhsNumber,
                null,
                null,
                null,
                null,
                null,
                default))
                    .ThrowsAsync(exception);

            // when
            (Bundle Bundle, Exception Exception) actualResult =
                await this.patientService.ExecuteWithTimeoutAsync(
                    fhirProvider.Patients,
                    default,
                    inputNhsNumber,
                    null,
                    null,
                    null,
                    null,
                    null);

            // then
            actualResult.Should().BeEquivalentTo(expectedResult);

            this.ddsFhirProviderMock.Verify(p => p.Patients.Everything(
                inputNhsNumber,
                null,
                null,
                null,
                null,
                null,
                default),
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
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            var timeoutException = new TimeoutException($"Provider call exceeded {timeoutMilliseconds} milliseconds.");
            var taskCompletionSource = new TaskCompletionSource<Bundle>();
            var fhirProvider = this.ddsFhirProviderMock.Object;

            (Bundle Bundle, Exception Exception) expectedResult = (null, timeoutException);

            this.ddsFhirProviderMock.Setup(p => p.Patients.Everything(
                inputNhsNumber,
                null,
                null,
                null,
                null,
                null,
                default))
                    .Returns(new ValueTask<Bundle>(taskCompletionSource.Task));

            // when
            (Bundle Bundle, Exception Exception) actualResult =
                await this.patientService.ExecuteWithTimeoutAsync(
                    fhirProvider.Patients,
                    default,
                    inputNhsNumber,
                    null,
                    null,
                    null,
                    null,
                    null);

            // then
            actualResult.Should().BeEquivalentTo(expectedResult);

            this.ddsFhirProviderMock.Verify(p => p.Patients.Everything(
                inputNhsNumber,
                null,
                null,
                null,
                null,
                null,
                default),
                    Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.ddsFhirProviderMock.VerifyNoOtherCalls();
        }
    }
}
