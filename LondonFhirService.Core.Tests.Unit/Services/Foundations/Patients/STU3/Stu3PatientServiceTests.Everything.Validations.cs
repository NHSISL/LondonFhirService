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
using LondonFhirService.Core.Services.Foundations.Patients.STU3;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Patients.STU3
{
    public partial class Stu3PatientServiceTests
    {
        [Fact]
        public async Task EverythingShouldThrowWhenNullProviderNames()
        {
            // given
            List<string> providerNames = null;
            string randomId = GetRandomString();
            string inputId = randomId;

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
            ValueTask<List<Bundle>> everythingTask = patientService.EverythingAsync(
                    providerNames: providerNames,
                    id: inputId,
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
        public async Task EverythingShouldThrowWhenInvalidId(string invalidText)
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
                key: "id",
                values: "Text is invalid");

            var expectedPatientServiceValidationException =
                new PatientServiceValidationException(
                    message: "Patient service validation error occurred, please fix the errors and try again.",
                    innerException: invalidArgumentsPatientServiceException);

            // when
            ValueTask<List<Bundle>> everythingTask = patientService.EverythingAsync(
                    providerNames: inputProviderNames,
                    id: invalidText,
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
            string randomId = GetRandomString();
            string inputId = randomId;

            List<Bundle> expectedBundles = new List<Bundle>
            {
                outputDdsBundle
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
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((outputDdsBundle, null));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<Bundle> actualBundles =
                await mockedPatientService.EverythingAsync(
                    providerNames: inputProviderNames,
                    id: inputId,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteEverythingWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    default,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformationAsync($"Removing '{this.unsupportedFhirProviderMock.Object.ProviderName}': " +
                    "Patients/$Everything not supported."),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteEverythingWithTimeoutAsync(
                    this.unsupportedFhirProviderMock.Object,
                    default,
                    inputId,
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
            string randomId = GetRandomString();
            string inputId = randomId;

            List<Bundle> expectedBundles = new List<Bundle>
            {
                outputDdsBundle
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
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((outputDdsBundle, null));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<Bundle> actualBundles =
                await mockedPatientService.EverythingAsync(
                    providerNames: inputProviderNames,
                    id: inputId,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteEverythingWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    default,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformationAsync($"Removing '{this.unsupportedErrorFhirProviderMock.Object.ProviderName}': " +
                    "Patients/$Everything not supported."),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteEverythingWithTimeoutAsync(
                    this.unsupportedErrorFhirProviderMock.Object,
                    default,
                    inputId,
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
            string randomId = GetRandomString();
            string inputId = randomId;

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
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((null, timeoutException));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<Bundle> actualBundles =
                await mockedPatientService.EverythingAsync(
                    providerNames: inputProviderNames,
                    id: inputId,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteEverythingWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    default,
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
                    inputId,
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
            string randomId = GetRandomString();
            string inputId = randomId;
            List<Bundle> expectedBundles = new List<Bundle>();

            List<Exception> exceptions = new List<Exception>
            {
                timeoutException,
                timeoutException2
            };

            AggregateException aggregateException = new AggregateException(
                "One or more provider calls failed or timed out.",
                exceptions);

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
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((null, timeoutException));

            patientServiceMock.Setup(service =>
                service.ExecuteEverythingWithTimeoutAsync(
                    ldsFhirProviderMock.Object,
                    default,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((null, timeoutException2));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<Bundle> actualBundles =
                await mockedPatientService.EverythingAsync(
                    providerNames: inputProviderNames,
                    id: inputId,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteEverythingWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    default,
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
                    inputId,
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
            string randomId = GetRandomString();
            string inputId = randomId;

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
                    inputCorrelationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((null, exception));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<Bundle> actualBundles =
                await mockedPatientService.EverythingAsync(
                    providerNames: inputProviderNames,
                    id: inputId,
                    correlationId: inputCorrelationId,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteEverythingWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    default,
                    inputCorrelationId,
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
                    inputCorrelationId,
                    inputId,
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
            Guid inputCorrelationId = Guid.NewGuid();

            List<string> randomProviderNames = new List<string>
            {
                "DDS",
                "LDS"
            };

            List<string> inputProviderNames = randomProviderNames.DeepClone();
            string randomId = GetRandomString();
            string inputId = randomId;
            List<Bundle> expectedBundles = new List<Bundle>();

            List<Exception> exceptions = new List<Exception>
            {
                exception,
                exception2
            };

            AggregateException aggregateException = new AggregateException(
                "One or more provider calls failed or timed out.",
                exceptions);

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
                    inputCorrelationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((null, exception));

            patientServiceMock.Setup(service =>
                service.ExecuteEverythingWithTimeoutAsync(
                    ldsFhirProviderMock.Object,
                    default,
                    inputCorrelationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((null, exception2));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<Bundle> actualBundles =
                await mockedPatientService.EverythingAsync(
                    providerNames: inputProviderNames,
                    correlationId: inputCorrelationId,
                    id: inputId,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteEverythingWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    default,
                    inputCorrelationId,
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
                    inputCorrelationId,
                    inputId,
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
