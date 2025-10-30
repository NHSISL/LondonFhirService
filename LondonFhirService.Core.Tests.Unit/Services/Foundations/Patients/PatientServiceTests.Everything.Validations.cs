// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Foundations.Patients.Exceptions;
using LondonFhirService.Core.Services.Foundations.Patients;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Patients
{
    public partial class PatientServiceTests
    {
        [Fact]
        public async Task EverythingShouldThrowWhenNullProviderNames()
        {
            // given
            List<string> providerNames = null;
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;

            var invalidArgumentsPatientServiceException = new InvalidArgumentsPatientServiceException(
                message: "Invalid argument patient service exception, " +
                    "please correct the errors and try again.");

            invalidArgumentsPatientServiceException.AddData(
                key: "providerNames",
                values: "List cannot be null");

            var expectedPatientServiceValidationException =
                new PatientServiceValidationException(
                    message: "Patient service validation error occurred, please fix the errors and try again.",
                    innerException: invalidArgumentsPatientServiceException);

            // when
            ValueTask<List<Bundle>> everythingTask = patientService.Everything(
                    providerNames: providerNames,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            PatientServiceValidationException actualPatientServiceValidationException =
                await Assert.ThrowsAsync<PatientServiceValidationException>(
                    testCode: everythingTask.AsTask);

            // then
            actualPatientServiceValidationException.Should().BeEquivalentTo(expectedPatientServiceValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedPatientServiceValidationException))),
                    Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]

        public async Task EverythingShouldThrowWhenInvalidNhsNumber(string invalidText)
        {
            // given
            List<string> randomProviderNames = new List<string>
            {
                "DDS",
                "LDS"
            };

            List<string> inputProviderNames = randomProviderNames.DeepClone();

            var invalidArgumentsPatientServiceException = new InvalidArgumentsPatientServiceException(
                message: "Invalid argument patient service exception, " +
                    "please correct the errors and try again.");

            invalidArgumentsPatientServiceException.AddData(
                key: "nhsNumber",
                values: "Text is invalid");

            var expectedPatientServiceValidationException =
                new PatientServiceValidationException(
                    message: "Patient service validation error occurred, please fix the errors and try again.",
                    innerException: invalidArgumentsPatientServiceException);

            // when
            ValueTask<List<Bundle>> everythingTask = patientService.Everything(
                    providerNames: inputProviderNames,
                    nhsNumber: invalidText,
                    cancellationToken: default);

            PatientServiceValidationException actualPatientServiceValidationException =
                await Assert.ThrowsAsync<PatientServiceValidationException>(
                    testCode: everythingTask.AsTask);

            // then
            actualPatientServiceValidationException.Should().BeEquivalentTo(expectedPatientServiceValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedPatientServiceValidationException))),
                    Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task EverythingShouldReturnSingleBundleWhenUnsupportedProvider()
        {
            // given
            List<string> randomProviderNames = new List<string>
            {
                "DDS",
                "Unsupported"
            };

            List<string> inputProviderNames = randomProviderNames.DeepClone();
            Bundle randomDdsBundle = CreateRandomBundle();
            Bundle outputDdsBundle = randomDdsBundle.DeepClone();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;

            List<Bundle> expectedBundles = new List<Bundle>
            {
                outputDdsBundle
            };

            var patientServiceMock = new Mock<PatientService>(
                this.fhirBroker,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig);

            patientServiceMock.Setup(service =>
                service.ExecuteWithTimeoutAsync(
                    ddsFhirProviderMock.Object.Patients,
                    default,
                    inputNhsNumber,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((outputDdsBundle, null));

            PatientService patientService = patientServiceMock.Object;

            // when
            List<Bundle> actualBundles =
                await patientService.Everything(
                    providerNames: inputProviderNames,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object.Patients,
                    default,
                    inputNhsNumber,
                    null,
                    null,
                    null,
                    null,
                    null),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteWithTimeoutAsync(
                    this.unsupportedFhirProviderMock.Object.Patients,
                    default,
                    inputNhsNumber,
                    null,
                    null,
                    null,
                    null,
                    null),
                        Times.Never());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task EverythingShouldReturnSingleBundleWhenUnsupportedProviderDueToError()
        {
            // given
            List<string> randomProviderNames = new List<string>
            {
                "DDS",
                "UnsupportedError"
            };

            List<string> inputProviderNames = randomProviderNames.DeepClone();
            Bundle randomDdsBundle = CreateRandomBundle();
            Bundle outputDdsBundle = randomDdsBundle.DeepClone();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;

            List<Bundle> expectedBundles = new List<Bundle>
            {
                outputDdsBundle
            };

            var patientServiceMock = new Mock<PatientService>(
                this.fhirBroker,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig);

            patientServiceMock.Setup(service =>
                service.ExecuteWithTimeoutAsync(
                    ddsFhirProviderMock.Object.Patients,
                    default,
                    inputNhsNumber,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((outputDdsBundle, null));

            PatientService patientService = patientServiceMock.Object;

            // when
            List<Bundle> actualBundles =
                await patientService.Everything(
                    providerNames: inputProviderNames,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object.Patients,
                    default,
                    inputNhsNumber,
                    null,
                    null,
                    null,
                    null,
                    null),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteWithTimeoutAsync(
                    this.unsupportedErrorFhirProviderMock.Object.Patients,
                    default,
                    inputNhsNumber,
                    null,
                    null,
                    null,
                    null,
                    null),
                        Times.Never());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task EverythingShouldReturnSingleBundleWhenOneProviderTimesOut()
        {
            // given
            var timeoutMilliseconds = 1;
            this.patientServiceConfig.MaxProviderWaitTimeMilliseconds = timeoutMilliseconds;
            var timeoutException = new TimeoutException($"Provider call exceeded {timeoutMilliseconds} milliseconds.");

            List<string> randomProviderNames = new List<string>
            {
                "DDS",
                "LDS"
            };

            List<string> inputProviderNames = randomProviderNames.DeepClone();
            Bundle randomDdsBundle = CreateRandomBundle();
            Bundle outputDdsBundle = randomDdsBundle.DeepClone();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;

            List<Bundle> expectedBundles = new List<Bundle>
            {
                outputDdsBundle
            };

            List<Exception> exceptions = new List<Exception>
            {
                timeoutException
            };

            AggregateException aggregateException = new AggregateException(
                "One or more provider calls failed or timed out.",
                exceptions);

            var patientServiceMock = new Mock<PatientService>(
                this.fhirBroker,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig);

            patientServiceMock.Setup(service =>
                service.ExecuteWithTimeoutAsync(
                    ddsFhirProviderMock.Object.Patients,
                    default,
                    inputNhsNumber,
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
                    inputNhsNumber,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((null, timeoutException));

            PatientService patientService = patientServiceMock.Object;

            // when
            List<Bundle> actualBundles =
                await patientService.Everything(
                    providerNames: inputProviderNames,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object.Patients,
                    default,
                    inputNhsNumber,
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
                    inputNhsNumber,
                    null,
                    null,
                    null,
                    null,
                    null),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(aggregateException))),
                    Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task EverythingShouldReturnEmptyBundleListWhenAllProvidersTimesOut()
        {
            // given
            var timeoutMilliseconds = 1;
            this.patientServiceConfig.MaxProviderWaitTimeMilliseconds = timeoutMilliseconds;
            var timeoutException = new TimeoutException($"Provider call exceeded {timeoutMilliseconds} milliseconds.");
            var timeoutException2 = new TimeoutException($"Provider call exceeded {timeoutMilliseconds} milliseconds.");

            List<string> randomProviderNames = new List<string>
            {
                "DDS",
                "LDS"
            };

            List<string> inputProviderNames = randomProviderNames.DeepClone();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            List<Bundle> expectedBundles = new List<Bundle>();

            List<Exception> exceptions = new List<Exception>
            {
                timeoutException,
                timeoutException2
            };

            AggregateException aggregateException = new AggregateException(
                "One or more provider calls failed or timed out.",
                exceptions);

            var patientServiceMock = new Mock<PatientService>(
                this.fhirBroker,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig);

            patientServiceMock.Setup(service =>
                service.ExecuteWithTimeoutAsync(
                    ddsFhirProviderMock.Object.Patients,
                    default,
                    inputNhsNumber,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((null, timeoutException));

            patientServiceMock.Setup(service =>
                service.ExecuteWithTimeoutAsync(
                    ldsFhirProviderMock.Object.Patients,
                    default,
                    inputNhsNumber,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((null, timeoutException2));

            PatientService patientService = patientServiceMock.Object;

            // when
            List<Bundle> actualBundles =
                await patientService.Everything(
                    providerNames: inputProviderNames,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object.Patients,
                    default,
                    inputNhsNumber,
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
                    inputNhsNumber,
                    null,
                    null,
                    null,
                    null,
                    null),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(aggregateException))),
                    Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task EverythingShouldReturnSingleBundleWhenOneProviderHasException()
        {
            // given
            Exception exception = new Exception(GetRandomString());

            List<string> randomProviderNames = new List<string>
            {
                "DDS",
                "LDS"
            };

            List<string> inputProviderNames = randomProviderNames.DeepClone();
            Bundle randomDdsBundle = CreateRandomBundle();
            Bundle outputDdsBundle = randomDdsBundle.DeepClone();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;

            List<Bundle> expectedBundles = new List<Bundle>
            {
                outputDdsBundle
            };

            List<Exception> exceptions = new List<Exception>
            {
                exception
            };

            AggregateException aggregateException = new AggregateException(
                "One or more provider calls failed or timed out.",
                exceptions);

            var patientServiceMock = new Mock<PatientService>(
                this.fhirBroker,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig);

            patientServiceMock.Setup(service =>
                service.ExecuteWithTimeoutAsync(
                    ddsFhirProviderMock.Object.Patients,
                    default,
                    inputNhsNumber,
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
                    inputNhsNumber,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((null, exception));

            PatientService patientService = patientServiceMock.Object;

            // when
            List<Bundle> actualBundles =
                await patientService.Everything(
                    providerNames: inputProviderNames,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object.Patients,
                    default,
                    inputNhsNumber,
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
                    inputNhsNumber,
                    null,
                    null,
                    null,
                    null,
                    null),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(aggregateException))),
                    Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task EverythingShouldReturnEmptyBundleListWhenAllProvidersError()
        {
            // given
            Exception exception = new Exception(GetRandomString());
            Exception exception2 = new Exception(GetRandomString());

            List<string> randomProviderNames = new List<string>
            {
                "DDS",
                "LDS"
            };

            List<string> inputProviderNames = randomProviderNames.DeepClone();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            List<Bundle> expectedBundles = new List<Bundle>();

            List<Exception> exceptions = new List<Exception>
            {
                exception,
                exception2
            };

            AggregateException aggregateException = new AggregateException(
                "One or more provider calls failed or timed out.",
                exceptions);

            var patientServiceMock = new Mock<PatientService>(
                this.fhirBroker,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig);

            patientServiceMock.Setup(service =>
                service.ExecuteWithTimeoutAsync(
                    ddsFhirProviderMock.Object.Patients,
                    default,
                    inputNhsNumber,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((null, exception));

            patientServiceMock.Setup(service =>
                service.ExecuteWithTimeoutAsync(
                    ldsFhirProviderMock.Object.Patients,
                    default,
                    inputNhsNumber,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((null, exception2));

            PatientService patientService = patientServiceMock.Object;

            // when
            List<Bundle> actualBundles =
                await patientService.Everything(
                    providerNames: inputProviderNames,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object.Patients,
                    default,
                    inputNhsNumber,
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
                    inputNhsNumber,
                    null,
                    null,
                    null,
                    null,
                    null),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(aggregateException))),
                    Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }
    }
}
