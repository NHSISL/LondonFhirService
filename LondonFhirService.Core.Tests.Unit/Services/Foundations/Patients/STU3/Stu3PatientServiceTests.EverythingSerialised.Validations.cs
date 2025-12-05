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
using LondonFhirService.Core.Models.Foundations.Patients.Exceptions;
using LondonFhirService.Core.Services.Foundations.Patients.STU3;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Patients.STU3
{
    public partial class Stu3PatientServiceTests
    {
        [Fact]
        public async Task EverythingSerialisedShouldThrowWhenNullProviderNames()
        {
            // given
            List<string> providerNames = null;
            Guid correlationId = Guid.NewGuid();
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
            ValueTask<List<string>> everythingSerialisedTask = patientService.EverythingSerialisedAsync(
                    providerNames: providerNames,
                    correlationId: correlationId,
                    id: inputId,
                    cancellationToken: default);

            PatientServiceValidationException actualPatientServiceValidationException =
                await Assert.ThrowsAsync<PatientServiceValidationException>(
                    testCode: everythingSerialisedTask.AsTask);

            // then
            actualPatientServiceValidationException.Should().BeEquivalentTo(expectedPatientServiceValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedPatientServiceValidationException))),
                    Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task EverythingSerialisedShouldThrowWhenInvalidId(string invalidText)
        {
            // given
            List<string> randomProviderNames = new List<string>
            {
                "DDS",
                "LDS"
            };

            List<string> inputProviderNames = randomProviderNames.DeepClone();
            Guid correlationId = Guid.Empty;

            var invalidArgumentsPatientServiceException = new InvalidArgumentsPatientServiceException(
                message: "Invalid argument patient service exception, " +
                    "please correct the errors and try again.");

            invalidArgumentsPatientServiceException.AddData(
                key: "id",
                values: "Text is invalid");

            invalidArgumentsPatientServiceException.AddData(
                key: "correlationId",
                values: "Id is invalid");

            var expectedPatientServiceValidationException =
                new PatientServiceValidationException(
                    message: "Patient service validation error occurred, please fix the errors and try again.",
                    innerException: invalidArgumentsPatientServiceException);

            // when
            ValueTask<List<string>> everythingSerialisedTask = patientService.EverythingSerialisedAsync(
                    providerNames: inputProviderNames,
                    correlationId: correlationId,
                    id: invalidText,
                    cancellationToken: default);

            PatientServiceValidationException actualPatientServiceValidationException =
                await Assert.ThrowsAsync<PatientServiceValidationException>(
                    testCode: everythingSerialisedTask.AsTask);

            // then
            actualPatientServiceValidationException.Should().BeEquivalentTo(expectedPatientServiceValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedPatientServiceValidationException))),
                    Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task EverythingSerialisedShouldReturnSingleBundleWhenUnsupportedProvider()
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
            string randomId = GetRandomString();
            string inputId = randomId;

            List<string> expectedBundles = new List<string>
            {
                rawOutputDdsBundle
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
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((rawOutputDdsBundle, null));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<string> actualJson =
                await mockedPatientService.EverythingSerialisedAsync(
                    providerNames: inputProviderNames,
                    correlationId: correlationId,
                    id: inputId,
                    cancellationToken: default);

            // then
            actualJson.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteEverythingSerialisedWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    default,
                    correlationId,
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
                service.ExecuteEverythingSerialisedWithTimeoutAsync(
                    this.unsupportedFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null),
                        Times.Never());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task EverythingSerialisedShouldReturnSingleBundleWhenUnsupportedProviderDueToError()
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
            string randomId = GetRandomString();
            string inputId = randomId;

            List<string> expectedBundles = new List<string>
            {
                rawOutputDdsBundle
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
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((rawOutputDdsBundle, null));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<string> actualJson =
                await mockedPatientService.EverythingSerialisedAsync(
                    providerNames: inputProviderNames,
                    correlationId: correlationId,
                    id: inputId,
                    cancellationToken: default);

            // then
            actualJson.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteEverythingSerialisedWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    default,
                    correlationId,
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
                service.ExecuteEverythingSerialisedWithTimeoutAsync(
                    this.unsupportedErrorFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null),
                        Times.Never());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task EverythingSerialisedShouldReturnSingleBundleWhenOneProviderTimesOut()
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
            string randomId = GetRandomString();
            string inputId = randomId;

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
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((rawOutputDdsBundle, null));

            patientServiceMock.Setup(service =>
                service.ExecuteEverythingSerialisedWithTimeoutAsync(
                    ldsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((null, timeoutException));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<string> actualJson =
                await mockedPatientService.EverythingSerialisedAsync(
                    providerNames: inputProviderNames,
                    correlationId: correlationId,
                    id: inputId,
                    cancellationToken: default);

            // then
            actualJson.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteEverythingSerialisedWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteEverythingSerialisedWithTimeoutAsync(
                    this.ldsFhirProviderMock.Object,
                    default,
                    correlationId,
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
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task EverythingSerialisedShouldReturnEmptyBundleListWhenAllProvidersTimesOut()
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
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((null, timeoutException));

            patientServiceMock.Setup(service =>
                service.ExecuteEverythingSerialisedWithTimeoutAsync(
                    ldsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((null, timeoutException2));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<string> actualJson =
                await mockedPatientService.EverythingSerialisedAsync(
                    providerNames: inputProviderNames,
                    correlationId: correlationId,
                    id: inputId,
                    cancellationToken: default);

            // then
            actualJson.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteEverythingSerialisedWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteEverythingSerialisedWithTimeoutAsync(
                    this.ldsFhirProviderMock.Object,
                    default,
                    correlationId,
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
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task EverythingSerialisedShouldReturnSingleBundleWhenOneProviderHasException()
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
            string randomId = GetRandomString();
            string inputId = randomId;

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
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((rawOutputDdsBundle, null));

            patientServiceMock.Setup(service =>
                service.ExecuteEverythingSerialisedWithTimeoutAsync(
                    ldsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null))
                .ReturnsAsync((null, exception));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<string> actualBundles =
                await mockedPatientService.EverythingSerialisedAsync(
                    providerNames: inputProviderNames,
                    correlationId: correlationId,
                    id: inputId,
                    cancellationToken: default);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteEverythingSerialisedWithTimeoutAsync(
                    this.ddsFhirProviderMock.Object,
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteEverythingSerialisedWithTimeoutAsync(
                    this.ldsFhirProviderMock.Object,
                    default,
                    correlationId,
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
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task EverythingSerialisedShouldReturnEmptyBundleListWhenAllProvidersError()
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
                .ReturnsAsync((null, exception));

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
                .ReturnsAsync((null, exception2));

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
            actualJson.Should().BeEquivalentTo(expectedBundles);

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

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(aggregateException))),
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
