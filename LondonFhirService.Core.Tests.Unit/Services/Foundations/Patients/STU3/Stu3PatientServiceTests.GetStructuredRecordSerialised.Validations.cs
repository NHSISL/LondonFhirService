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
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Services.Foundations.Patients.STU3;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Patients.STU3
{
    public partial class Stu3PatientServiceTests
    {
        [Fact]
        public async Task GetStructuredRecordSerialisedShouldThrowWhenNullActiveProviders()
        {
            // given
            List<Provider> providers = null;
            Guid correlationId = Guid.NewGuid();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;

            var invalidArgumentsPatientServiceException = new InvalidArgumentsPatientServiceException(
                message: "Invalid argument patient service exception, " +
                    "please correct the errors and try again.");

            invalidArgumentsPatientServiceException.AddData(
                key: "activeProviders",
                values: "List cannot be null");

            var expectedPatientServiceValidationException =
                new PatientServiceValidationException(
                    message: "Patient service validation error occurred, please fix the errors and try again.",
                    innerException: invalidArgumentsPatientServiceException);

            // when
            ValueTask<List<string>> everythingTask = patientService.GetStructuredRecordSerialisedAsync(
                activeProviders: providers,
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
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetStructuredRecordSerialisedShouldThrowWhenInvalidId(string invalidText)
        {
            // given
            List<Provider> randomProviderNames = new List<Provider>
            {
                new Provider{ FriendlyName= "DDS" },
                new Provider{ FriendlyName= "LDS" }
            };

            Guid correlationId = Guid.Empty;
            List<Provider> inputProviders = randomProviderNames.DeepClone();

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
                    activeProviders: inputProviders,
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
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetStructuredRecordSerialisedShouldReturnSingleBundleWhenUnsupportedProvider()
        {
            // given
            Provider ddsProvider = new Provider { FriendlyName = "DDS", IsPrimary = true };
            Provider unsupportedProvider = new Provider { FriendlyName = "Unsupported", IsPrimary = false };

            List<Provider> randomProviders = new List<Provider>
            {
                ddsProvider,
                unsupportedProvider,
            };

            List<Provider> inputProviders = randomProviders.DeepClone();
            Bundle randomDdsBundle = CreateRandomBundle();
            Bundle outputDdsBundle = randomDdsBundle.DeepClone();
            string rawOutputDdsBundle = this.fhirJsonSerializer.SerializeToString(outputDdsBundle);
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            DateTime? inputDateOfBirth = DateTime.Now;
            bool? inputDemographicsOnly = false;
            bool? inputActivePatientsOnly = true;
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{inputDateOfBirth}\", " +
                $"demographicsOnly = \"{inputDemographicsOnly}\", " +
                $"includeInactivePatients = \"{inputActivePatientsOnly}\" }}";

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
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ddsProvider.FriendlyName,
                    ddsProvider.IsPrimary,
                    ddsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly))
                .ReturnsAsync((rawOutputDdsBundle, null));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<string> actualBundles =
                await mockedPatientService.GetStructuredRecordSerialisedAsync(
                    activeProviders: inputProviders,
                    correlationId: correlationId,
                    nhsNumber: inputNhsNumber,
                    dateOfBirth: inputDateOfBirth,
                    demographicsOnly: inputDemographicsOnly,
                    includeInactivePatients: inputActivePatientsOnly,
                    cancellationToken: cancellationToken);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ddsProvider.FriendlyName,
                    ddsProvider.IsPrimary,
                    this.ddsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformationAsync($"Removing '{this.unsupportedFhirProviderMock.Object.ProviderName}': " +
                    "Patients/$GetStructuredRecord not supported."),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    unsupportedProvider.FriendlyName,
                    unsupportedProvider.IsPrimary,
                    this.unsupportedFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly),
                        Times.Never());

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

        [Fact]
        public async Task GetStructuredRecordSerialisedShouldReturnSingleBundleWhenUnsupportedProviderDueToError()
        {
            // given
            Provider ddsProvider = new Provider { FriendlyName = "DDS", IsPrimary = true };
            Provider unsupportedProvider = new Provider { FriendlyName = "Unsupported", IsPrimary = false };

            List<Provider> randomProviders = new List<Provider>
            {
                ddsProvider,
                unsupportedProvider,
            };

            List<Provider> inputProviders = randomProviders.DeepClone();
            Bundle randomDdsBundle = CreateRandomBundle();
            Bundle outputDdsBundle = randomDdsBundle.DeepClone();
            string rawOutputDdsBundle = this.fhirJsonSerializer.SerializeToString(outputDdsBundle);
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            DateTime? inputDateOfBirth = DateTime.Now;
            bool? inputDemographicsOnly = false;
            bool? inputActivePatientsOnly = true;
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{inputDateOfBirth}\", " +
                $"demographicsOnly = \"{inputDemographicsOnly}\", " +
                $"includeInactivePatients = \"{inputActivePatientsOnly}\" }}";

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
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ddsProvider.FriendlyName,
                    ddsProvider.IsPrimary,
                    ddsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly))
                .ReturnsAsync((rawOutputDdsBundle, null));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<string> actualBundles =
                await mockedPatientService.GetStructuredRecordSerialisedAsync(
                    activeProviders: inputProviders,
                    correlationId: correlationId,
                    nhsNumber: inputNhsNumber,
                    dateOfBirth: inputDateOfBirth,
                    demographicsOnly: inputDemographicsOnly,
                    includeInactivePatients: inputActivePatientsOnly,
                    cancellationToken: cancellationToken);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ddsProvider.FriendlyName,
                    ddsProvider.IsPrimary,
                    this.ddsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly),
                        Times.Once());

            this.loggingBrokerMock.Verify(broker =>
                broker.LogInformationAsync($"Removing '{this.unsupportedErrorFhirProviderMock.Object.ProviderName}': " +
                    "Patients/$GetStructuredRecord not supported."),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    unsupportedProvider.FriendlyName,
                    unsupportedProvider.IsPrimary,
                    this.unsupportedErrorFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly),
                        Times.Never());

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

        [Fact]
        public async Task GetStructuredRecordSerialisedShouldReturnSingleBundleWhenOneProviderTimesOut()
        {
            // given
            var timeoutMilliseconds = 1;
            this.patientServiceConfig.MaxProviderWaitTimeMilliseconds = timeoutMilliseconds;
            var timeoutException = new TimeoutException($"Provider call exceeded {timeoutMilliseconds} milliseconds.");

            Provider ddsProvider = new Provider { FriendlyName = "DDS", IsPrimary = true };
            Provider ldsProvider = new Provider { FriendlyName = "LDS", IsPrimary = false };

            List<Provider> randomProviders = new List<Provider>
            {
                ddsProvider,
                ldsProvider,
            };

            List<Provider> inputProviders = randomProviders.DeepClone();
            Bundle randomDdsBundle = CreateRandomBundle();
            Bundle outputDdsBundle = randomDdsBundle.DeepClone();
            string rawOutputDdsBundle = this.fhirJsonSerializer.SerializeToString(outputDdsBundle);
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            DateTime? inputDateOfBirth = DateTime.Now;
            bool? inputDemographicsOnly = false;
            bool? inputActivePatientsOnly = true;
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{inputDateOfBirth}\", " +
                $"demographicsOnly = \"{inputDemographicsOnly}\", " +
                $"includeInactivePatients = \"{inputActivePatientsOnly}\" }}";

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
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ddsProvider.FriendlyName,
                    ddsProvider.IsPrimary,
                    ddsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly))
                .ReturnsAsync((rawOutputDdsBundle, null));

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ldsProvider.FriendlyName,
                    ldsProvider.IsPrimary,
                    ldsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly))
                .ReturnsAsync((null, timeoutException));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<string> actualBundles =
                await mockedPatientService.GetStructuredRecordSerialisedAsync(
                    activeProviders: inputProviders,
                    correlationId: correlationId,
                    nhsNumber: inputNhsNumber,
                    dateOfBirth: inputDateOfBirth,
                    demographicsOnly: inputDemographicsOnly,
                    includeInactivePatients: inputActivePatientsOnly,
                    cancellationToken: cancellationToken);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ddsProvider.FriendlyName,
                    ddsProvider.IsPrimary,
                    this.ddsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ldsProvider.FriendlyName,
                    ldsProvider.IsPrimary,
                    this.ldsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly),
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

        [Fact]
        public async Task GetStructuredRecordSerialisedShouldReturnEmptyBundleListWhenAllProvidersTimesOut()
        {
            // given
            var timeoutMilliseconds = 1;
            this.patientServiceConfig.MaxProviderWaitTimeMilliseconds = timeoutMilliseconds;
            var timeoutException = new TimeoutException($"Provider call exceeded {timeoutMilliseconds} milliseconds.");
            var timeoutException2 = new TimeoutException($"Provider call exceeded {timeoutMilliseconds} milliseconds.");

            Provider ddsProvider = new Provider { FriendlyName = "DDS", IsPrimary = true };
            Provider ldsProvider = new Provider { FriendlyName = "LDS", IsPrimary = false };

            List<Provider> randomProviders = new List<Provider>
            {
                ddsProvider,
                ldsProvider,
            };

            List<Provider> inputProviders = randomProviders.DeepClone();
            List<Bundle> expectedBundles = new List<Bundle>();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            DateTime? inputDateOfBirth = DateTime.Now;
            bool? inputDemographicsOnly = false;
            bool? inputActivePatientsOnly = true;
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{inputDateOfBirth}\", " +
                $"demographicsOnly = \"{inputDemographicsOnly}\", " +
                $"includeInactivePatients = \"{inputActivePatientsOnly}\" }}";

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
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ddsProvider.FriendlyName,
                    ddsProvider.IsPrimary,
                    ddsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly))
                .ReturnsAsync((null, timeoutException));

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ldsProvider.FriendlyName,
                    ldsProvider.IsPrimary,
                    ldsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly))
                .ReturnsAsync((null, timeoutException2));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<string> actualBundles =
                await mockedPatientService.GetStructuredRecordSerialisedAsync(
                    activeProviders: inputProviders,
                    correlationId: correlationId,
                    nhsNumber: inputNhsNumber,
                    dateOfBirth: inputDateOfBirth,
                    demographicsOnly: inputDemographicsOnly,
                    includeInactivePatients: inputActivePatientsOnly,
                    cancellationToken: cancellationToken);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ddsProvider.FriendlyName,
                    ddsProvider.IsPrimary,
                    this.ddsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ldsProvider.FriendlyName,
                    ldsProvider.IsPrimary,
                    this.ldsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly),
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

        [Fact]
        public async Task GetStructuredRecordSerialisedShouldReturnSingleBundleWhenOneProviderHasException()
        {
            // given
            Exception exception = new Exception(GetRandomString());

            Provider ddsProvider = new Provider { FriendlyName = "DDS", IsPrimary = true };
            Provider ldsProvider = new Provider { FriendlyName = "LDS", IsPrimary = false };

            List<Provider> randomProviders = new List<Provider>
            {
                ddsProvider,
                ldsProvider,
            };

            List<Provider> inputProviders = randomProviders.DeepClone();
            Bundle randomDdsBundle = CreateRandomBundle();
            Bundle outputDdsBundle = randomDdsBundle.DeepClone();
            string rawOutputDdsBundle = this.fhirJsonSerializer.SerializeToString(outputDdsBundle);
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            DateTime? inputDateOfBirth = DateTime.Now;
            bool? inputDemographicsOnly = false;
            bool? inputActivePatientsOnly = true;
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{inputDateOfBirth}\", " +
                $"demographicsOnly = \"{inputDemographicsOnly}\", " +
                $"includeInactivePatients = \"{inputActivePatientsOnly}\" }}";

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
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ddsProvider.FriendlyName,
                    ddsProvider.IsPrimary,
                    ddsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly))
                .ReturnsAsync((rawOutputDdsBundle, null));

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ldsProvider.FriendlyName,
                    ldsProvider.IsPrimary,
                    ldsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly))
                .ReturnsAsync((null, exception));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<string> actualBundles =
                await mockedPatientService.GetStructuredRecordSerialisedAsync(
                    activeProviders: inputProviders,
                    correlationId: correlationId,
                    nhsNumber: inputNhsNumber,
                    dateOfBirth: inputDateOfBirth,
                    demographicsOnly: inputDemographicsOnly,
                    includeInactivePatients: inputActivePatientsOnly,
                    cancellationToken: cancellationToken);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ddsProvider.FriendlyName,
                    ddsProvider.IsPrimary,
                    this.ddsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ldsProvider.FriendlyName,
                    ldsProvider.IsPrimary,
                    this.ldsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly),
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

        [Fact]
        public async Task GetStructuredRecordSerialisedShouldReturnEmptyBundleListWhenAllProvidersError()
        {
            // given
            Exception exception = new Exception(GetRandomString());
            Exception exception2 = new Exception(GetRandomString());

            Provider ddsProvider = new Provider { FriendlyName = "DDS", IsPrimary = true };
            Provider ldsProvider = new Provider { FriendlyName = "LDS", IsPrimary = false };

            List<Provider> randomProviders = new List<Provider>
            {
                ddsProvider,
                ldsProvider,
            };

            List<Provider> inputProviders = randomProviders.DeepClone();
            List<Bundle> expectedBundles = new List<Bundle>();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            DateTime? inputDateOfBirth = DateTime.Now;
            bool? inputDemographicsOnly = false;
            bool? inputActivePatientsOnly = true;
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{inputDateOfBirth}\", " +
                $"demographicsOnly = \"{inputDemographicsOnly}\", " +
                $"includeInactivePatients = \"{inputActivePatientsOnly}\" }}";

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
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ddsProvider.FriendlyName,
                    ddsProvider.IsPrimary,
                    ddsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly))
                .ReturnsAsync((null, exception));

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ldsProvider.FriendlyName,
                    ldsProvider.IsPrimary,
                    ldsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly))
                .ReturnsAsync((null, exception2));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<string> actualBundles =
                await mockedPatientService.GetStructuredRecordSerialisedAsync(
                    activeProviders: inputProviders,
                    correlationId: correlationId,
                    nhsNumber: inputNhsNumber,
                    dateOfBirth: inputDateOfBirth,
                    demographicsOnly: inputDemographicsOnly,
                    includeInactivePatients: inputActivePatientsOnly,
                    cancellationToken: cancellationToken);

            // then
            actualBundles.Should().BeEquivalentTo(expectedBundles);

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ddsProvider.FriendlyName,
                    ddsProvider.IsPrimary,
                    this.ddsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly),
                        Times.Once());

            patientServiceMock.Verify(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    ldsProvider.FriendlyName,
                    ldsProvider.IsPrimary,
                    this.ldsFhirProviderMock.Object,
                    cancellationToken,
                    correlationId,
                    inputNhsNumber,
                    inputDateOfBirth,
                    inputDemographicsOnly,
                    inputActivePatientsOnly),
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
