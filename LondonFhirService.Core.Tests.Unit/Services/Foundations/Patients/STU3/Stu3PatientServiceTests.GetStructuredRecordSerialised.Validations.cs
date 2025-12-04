// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using LondonFhirService.Core.Models.Foundations.Patients.Exceptions;
using LondonFhirService.Core.Services.Foundations.Patients.STU3;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Patients.STU3
{
    public partial class Stu3PatientServiceTests
    {
        [Fact]
        public async Task GetStructuredRecordSerialisedShouldThrowWhenNullProviderNames()
        {
            // given
            List<string> providerNames = null;
            Guid correlationId = Guid.NewGuid();
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
            ValueTask<List<string>> everythingTask = patientService.GetStructuredRecordSerialisedAsync(
                providerNames: providerNames,
                correlationId: correlationId,
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
        public async Task GetStructuredRecordSerialisedShouldThrowWhenInvalidId(string invalidText)
        {
            // given
            List<string> randomProviderNames = new List<string>
            {
                "DDS",
                "LDS"
            };

            Guid correlationId = Guid.Empty;
            List<string> inputProviderNames = randomProviderNames.DeepClone();

            var invalidArgumentsPatientServiceException = new InvalidArgumentsPatientServiceException(
                message: "Invalid argument patient service exception, " +
                    "please correct the errors and try again.");

            invalidArgumentsPatientServiceException.AddData(
                key: "nhsNumber",
                values: "Text is invalid");

            invalidArgumentsPatientServiceException.AddData(
                key: "correlationId",
                values: "Id is invalid");

            var expectedPatientServiceValidationException =
                new PatientServiceValidationException(
                    message: "Patient service validation error occurred, please fix the errors and try again.",
                    innerException: invalidArgumentsPatientServiceException);

            // when
            ValueTask<List<string>> everythingTask = patientService.GetStructuredRecordSerialisedAsync(
                    providerNames: inputProviderNames,
                    correlationId: correlationId,
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
        public async Task GetStructuredRecordSerialisedShouldReturnSingleBundleWhenUnsupportedProvider()
        {
            // given
            List<string> randomProviderNames = new List<string>
            {
                "DDS",
                "Unsupported"
            };

            List<string> inputProviderNames = randomProviderNames.DeepClone();
            Guid correlationId = Guid.NewGuid();
            Bundle randomDdsBundle = CreateRandomBundle();
            Bundle outputDdsBundle = randomDdsBundle.DeepClone();
            string rawOutputDdsBundle = this.fhirJsonSerializer.SerializeToString(outputDdsBundle);
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;

            List<string> expectedBundles = new List<string>
            {
                rawOutputDdsBundle
            };

            var patientServiceMock = new Mock<Stu3PatientService>(
                this.fhirBroker,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ddsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputNhsNumber,
                    null,
                    null,
                    null))
                .ReturnsAsync((rawOutputDdsBundle, null));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<string> actualBundles =
                await mockedPatientService.GetStructuredRecordSerialisedAsync(
                    providerNames: inputProviderNames,
                    correlationId: correlationId,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputNhsNumber,
                    null,
                    null,
                    null),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformationAsync($"Removing '{this.unsupportedFhirProviderMock.Object.ProviderName}': " +
                    "Patients/$GetStructuredRecord not supported."),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    this.unsupportedFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputNhsNumber,
                    null,
                    null,
                    null),
                        Times.Never());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetStructuredRecordSerialisedShouldReturnSingleBundleWhenUnsupportedProviderDueToError()
        {
            // given
            List<string> randomProviderNames = new List<string>
            {
                "DDS",
                "UnsupportedError"
            };

            List<string> inputProviderNames = randomProviderNames.DeepClone();
            Guid correlationId = Guid.NewGuid();
            Bundle randomDdsBundle = CreateRandomBundle();
            Bundle outputDdsBundle = randomDdsBundle.DeepClone();
            string rawOutputDdsBundle = this.fhirJsonSerializer.SerializeToString(outputDdsBundle);
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;

            List<string> expectedBundles = new List<string>
            {
                rawOutputDdsBundle
            };

            var patientServiceMock = new Mock<Stu3PatientService>(
                this.fhirBroker,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ddsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputNhsNumber,
                    null,
                    null,
                    null))
                .ReturnsAsync((rawOutputDdsBundle, null));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<string> actualBundles =
                await mockedPatientService.GetStructuredRecordSerialisedAsync(
                    providerNames: inputProviderNames,
                    correlationId: correlationId,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputNhsNumber,
                    null,
                    null,
                    null),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformationAsync($"Removing '{this.unsupportedErrorFhirProviderMock.Object.ProviderName}': " +
                    "Patients/$GetStructuredRecord not supported."),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    this.unsupportedErrorFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputNhsNumber,
                    null,
                    null,
                    null),
                        Times.Never());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetStructuredRecordSerialisedShouldReturnSingleBundleWhenOneProviderTimesOut()
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
            Guid correlationId = Guid.NewGuid();
            Bundle randomDdsBundle = CreateRandomBundle();
            Bundle outputDdsBundle = randomDdsBundle.DeepClone();
            string rawOutputDdsBundle = this.fhirJsonSerializer.SerializeToString(outputDdsBundle);
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;

            List<string> expectedBundles = new List<string>
            {
                rawOutputDdsBundle
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
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ddsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputNhsNumber,
                    null,
                    null,
                    null))
                .ReturnsAsync((rawOutputDdsBundle, null));

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ldsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputNhsNumber,
                    null,
                    null,
                    null))
                .ReturnsAsync((null, timeoutException));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<string> actualBundles =
                await mockedPatientService.GetStructuredRecordSerialisedAsync(
                    providerNames: inputProviderNames,
                    correlationId: correlationId,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputNhsNumber,
                    null,
                    null,
                    null),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    this.ldsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputNhsNumber,
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
        public async Task GetStructuredRecordSerialisedShouldReturnEmptyBundleListWhenAllProvidersTimesOut()
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
            Guid correlationId = Guid.NewGuid();
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

            var patientServiceMock = new Mock<Stu3PatientService>(
                this.fhirBroker,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ddsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputNhsNumber,
                    null,
                    null,
                    null))
                .ReturnsAsync((null, timeoutException));

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ldsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputNhsNumber,
                    null,
                    null,
                    null))
                .ReturnsAsync((null, timeoutException2));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<string> actualBundles =
                await mockedPatientService.GetStructuredRecordSerialisedAsync(
                    providerNames: inputProviderNames,
                    correlationId: correlationId,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputNhsNumber,
                    null,
                    null,
                    null),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    this.ldsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputNhsNumber,
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
        public async Task GetStructuredRecordSerialisedShouldReturnSingleBundleWhenOneProviderHasException()
        {
            // given
            Exception exception = new Exception(GetRandomString());

            List<string> randomProviderNames = new List<string>
            {
                "DDS",
                "LDS"
            };

            List<string> inputProviderNames = randomProviderNames.DeepClone();
            Guid correlationId = Guid.NewGuid();
            Bundle randomDdsBundle = CreateRandomBundle();
            Bundle outputDdsBundle = randomDdsBundle.DeepClone();
            string rawOutputDdsBundle = this.fhirJsonSerializer.SerializeToString(outputDdsBundle);
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;

            List<string> expectedBundles = new List<string>
            {
                rawOutputDdsBundle
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
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ddsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputNhsNumber,
                    null,
                    null,
                    null))
                .ReturnsAsync((rawOutputDdsBundle, null));

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ldsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputNhsNumber,
                    null,
                    null,
                    null))
                .ReturnsAsync((null, exception));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<string> actualBundles =
                await mockedPatientService.GetStructuredRecordSerialisedAsync(
                    providerNames: inputProviderNames,
                    correlationId: correlationId,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputNhsNumber,
                    null,
                    null,
                    null),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    this.ldsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputNhsNumber,
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
        public async Task GetStructuredRecordSerialisedShouldReturnEmptyBundleListWhenAllProvidersError()
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
            Guid correlationId = Guid.NewGuid();
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

            var patientServiceMock = new Mock<Stu3PatientService>(
                this.fhirBroker,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ddsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputNhsNumber,
                    null,
                    null,
                    null))
                .ReturnsAsync((null, exception));

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ldsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputNhsNumber,
                    null,
                    null,
                    null))
                .ReturnsAsync((null, exception2));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<string> actualBundles =
                await mockedPatientService.GetStructuredRecordSerialisedAsync(
                    providerNames: inputProviderNames,
                    correlationId: correlationId,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputNhsNumber,
                    null,
                    null,
                    null),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    this.ldsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputNhsNumber,
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
