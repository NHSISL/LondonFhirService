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
            ValueTask<List<(string Provider, string Json)>> getStructuredRecordSerialisedTask =
                patientService.GetStructuredRecordSerialisedAsync(
                    activeProviders: providers,
                    correlationId: correlationId,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: default);

            PatientServiceValidationException actualPatientServiceValidationException =
                await Assert.ThrowsAsync<PatientServiceValidationException>(
                    testCode: getStructuredRecordSerialisedTask.AsTask);

            // then
            actualPatientServiceValidationException.Should().BeEquivalentTo(expectedPatientServiceValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedPatientServiceValidationException))),
                    Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
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
            ValueTask<List<(string Provider, string Json)>> getStructuredRecordSerialisedTask = patientService.GetStructuredRecordSerialisedAsync(
                    activeProviders: inputProviders,
                    correlationId: correlationId,
                    nhsNumber: invalidText,
                    cancellationToken: default);

            PatientServiceValidationException actualPatientServiceValidationException =
                await Assert.ThrowsAsync<PatientServiceValidationException>(
                    testCode: getStructuredRecordSerialisedTask.AsTask);

            // then
            actualPatientServiceValidationException.Should().BeEquivalentTo(expectedPatientServiceValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedPatientServiceValidationException))),
                    Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("2002-24-01")]
        [InlineData("24-01-2002")]
        public async Task GetStructuredRecordSerialisedShouldThrowWhenInvalidDateAsync(string dateString)
        {
            // given
            string nhsNumber = "1234567890";

            List<Provider> randomProviderNames = new List<Provider>
            {
                new Provider{ FriendlyName= "DDS" },
                new Provider{ FriendlyName= "LDS" }
            };

            Guid correlationId = Guid.NewGuid();
            List<Provider> inputProviders = randomProviderNames.DeepClone();

            var invalidArgumentsPatientServiceException = new InvalidArgumentsPatientServiceException(
                message: "Invalid argument patient service exception, " +
                    "please correct the errors and try again.");

            invalidArgumentsPatientServiceException.AddData(
                key: "dateOfBirth",
                values: "Text must be a valid date string in format 'yyyy-MM-dd' e.g. '2002-10-01'");

            var expectedPatientServiceValidationException =
                new PatientServiceValidationException(
                    message: "Patient service validation error occurred, please fix the errors and try again.",
                    innerException: invalidArgumentsPatientServiceException);

            // when
            ValueTask<List<(string Provider, string Json)>> getStructuredRecordSerialisedTask = patientService.GetStructuredRecordSerialisedAsync(
                    activeProviders: inputProviders,
                    correlationId: correlationId,
                    nhsNumber: nhsNumber,
                    dateOfBirth: dateString,
                    cancellationToken: default);

            PatientServiceValidationException actualPatientServiceValidationException =
                await Assert.ThrowsAsync<PatientServiceValidationException>(
                    testCode: getStructuredRecordSerialisedTask.AsTask);

            // then
            actualPatientServiceValidationException.Should().BeEquivalentTo(expectedPatientServiceValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedPatientServiceValidationException))),
                    Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetStructuredRecordSerialisedShouldReturnSingleBundleWhenUnsupportedProvider()
        {
            // given
            Provider ddsProvider =
                new Provider
                {
                    FriendlyName = "DDS Provider",
                    FullyQualifiedName = "DDS",
                    IsPrimary = true
                };

            Provider unsupportedProvider =
                new Provider
                {
                    FriendlyName = "Unsupported Provider",
                    FullyQualifiedName = "Unsupported",
                    IsPrimary = false
                };

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
            string inputDateOfBirth = DateTime.Now.ToString("yyyy-MM-dd");
            bool? inputDemographicsOnly = false;
            bool? inputActivePatientsOnly = true;
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{inputDateOfBirth}\", " +
                $"demographicsOnly = \"{inputDemographicsOnly}\", " +
                $"includeInactivePatients = \"{inputActivePatientsOnly}\" }}";

            List<(string Provider, string Json)> expectedBundles = new List<(string Provider, string Json)>
            {
                (ddsProvider.FriendlyName, rawOutputDdsBundle)
            };

            var patientServiceMock = new Mock<Stu3PatientService>(
                this.fhirBroker,
                this.auditBrokerMock.Object,
                this.identifierBrokerMock.Object,
                this.securityAuditBrokerMock.Object,
                this.storageBrokerMock.Object,
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
                .ReturnsAsync((ddsProvider.FriendlyName, rawOutputDdsBundle, null));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<(string Provider, string Json)> actualBundles =
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
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Parallel Provider Execution Started",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    It.Is<string>(s => s.StartsWith("Parallel Provider Execution Completed")),
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    It.Is<string>(s => s.StartsWith("Foundation Service Request Completed")),
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetStructuredRecordSerialisedShouldReturnSingleBundleWhenUnsupportedProviderDueToError()
        {
            // given
            Provider ddsProvider =
                new Provider
                {
                    FriendlyName = "DDS Provider",
                    FullyQualifiedName = "DDS",
                    IsPrimary = true
                };

            Provider unsupportedProvider =
                new Provider
                {
                    FriendlyName = "Unsupported Error Provider",
                    FullyQualifiedName = "UnsupportedError",
                    IsPrimary = false
                };

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
            string inputDateOfBirth = DateTime.Now.ToString("yyyy-MM-dd");
            bool? inputDemographicsOnly = false;
            bool? inputActivePatientsOnly = true;
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{inputDateOfBirth}\", " +
                $"demographicsOnly = \"{inputDemographicsOnly}\", " +
                $"includeInactivePatients = \"{inputActivePatientsOnly}\" }}";

            List<(string Provider, string Json)> expectedBundles = new List<(string Provider, string Json)>
            {
                (ddsProvider.FriendlyName, rawOutputDdsBundle)
            };

            var patientServiceMock = new Mock<Stu3PatientService>(
                this.fhirBroker,
                this.auditBrokerMock.Object,
                this.identifierBrokerMock.Object,
                this.securityAuditBrokerMock.Object,
                this.storageBrokerMock.Object,
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
                .ReturnsAsync((ddsProvider.FriendlyName, rawOutputDdsBundle, null));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<(string Provider, string Json)> actualBundles =
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
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Parallel Provider Execution Started",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    It.Is<string>(s => s.StartsWith("Parallel Provider Execution Completed")),
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    It.Is<string>(s => s.StartsWith("Foundation Service Request Completed")),
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetStructuredRecordSerialisedShouldReturnSingleBundleWhenOneProviderTimesOut()
        {
            // given
            var timeoutMilliseconds = 1;
            this.patientServiceConfig.MaxProviderWaitTimeMilliseconds = timeoutMilliseconds;
            var timeoutException = new TimeoutException($"Provider call exceeded {timeoutMilliseconds} milliseconds.");

            Provider ddsProvider =
                new Provider { FriendlyName = "DDS Provider", FullyQualifiedName = "DDS", IsPrimary = true };

            Provider ldsProvider =
                new Provider { FriendlyName = "LDS Provider", FullyQualifiedName = "LDS", IsPrimary = false };

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
            string inputDateOfBirth = DateTime.Now.ToString("yyyy-MM-dd");
            bool? inputDemographicsOnly = false;
            bool? inputActivePatientsOnly = true;
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{inputDateOfBirth}\", " +
                $"demographicsOnly = \"{inputDemographicsOnly}\", " +
                $"includeInactivePatients = \"{inputActivePatientsOnly}\" }}";

            List<(string Provider, string Json)> expectedBundles = new List<(string Provider, string Json)>
            {
                (ddsProvider.FriendlyName, rawOutputDdsBundle)
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
                this.securityAuditBrokerMock.Object,
                this.storageBrokerMock.Object,
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
                .ReturnsAsync((ddsProvider.FriendlyName, rawOutputDdsBundle, null));

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
                .ReturnsAsync((ldsProvider.FriendlyName, null, timeoutException));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<(string Provider, string Json)> actualBundles =
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
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Parallel Provider Execution Started",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    It.Is<string>(s => s.StartsWith("Parallel Provider Execution Completed")),
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    It.Is<string>(s => s.StartsWith("Foundation Service Request Completed")),
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
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

            Provider ddsProvider =
                new Provider { FriendlyName = "DDS Provider", FullyQualifiedName = "DDS", IsPrimary = true };

            Provider ldsProvider =
                new Provider { FriendlyName = "LDS Provider", FullyQualifiedName = "LDS", IsPrimary = false };

            List<Provider> randomProviders = new List<Provider>
            {
                ddsProvider,
                ldsProvider,
            };

            List<Provider> inputProviders = randomProviders.DeepClone();
            List<Bundle> expectedBundles = new List<Bundle>();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            string inputDateOfBirth = DateTime.Now.ToString("yyyy-MM-dd");
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
                this.securityAuditBrokerMock.Object,
                this.storageBrokerMock.Object,
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
                .ReturnsAsync((ddsProvider.FriendlyName, null, timeoutException));

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
                .ReturnsAsync((ldsProvider.FriendlyName, null, timeoutException2));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<(string Provider, string Json)> actualBundles =
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
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Parallel Provider Execution Started",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    It.Is<string>(s => s.StartsWith("Parallel Provider Execution Completed")),
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    It.Is<string>(s => s.StartsWith("Foundation Service Request Completed")),
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetStructuredRecordSerialisedShouldReturnSingleBundleWhenOneProviderHasException()
        {
            // given
            Exception exception = new Exception(GetRandomString());

            Provider ddsProvider =
                new Provider { FriendlyName = "DDS Provider", FullyQualifiedName = "DDS", IsPrimary = true };

            Provider ldsProvider =
                new Provider { FriendlyName = "LDS Provider", FullyQualifiedName = "LDS", IsPrimary = false };

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
            string inputDateOfBirth = DateTime.Now.ToString("yyyy-MM-dd");
            bool? inputDemographicsOnly = false;
            bool? inputActivePatientsOnly = true;
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-GetStructuredRecordSerialised";

            string message =
                $"Parameters:  {{ nhsNumber = \"{inputNhsNumber}\", dateOfBirth = \"{inputDateOfBirth}\", " +
                $"demographicsOnly = \"{inputDemographicsOnly}\", " +
                $"includeInactivePatients = \"{inputActivePatientsOnly}\" }}";

            List<(string Provider, string Json)> expectedBundles = new List<(string Provider, string Json)>
            {
                (ddsProvider.FriendlyName, rawOutputDdsBundle)
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
                this.securityAuditBrokerMock.Object,
                this.storageBrokerMock.Object,
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
                .ReturnsAsync((ddsProvider.FriendlyName, rawOutputDdsBundle, null));

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
                .ReturnsAsync((ldsProvider.FriendlyName, null, exception));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<(string Provider, string Json)> actualBundles =
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
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Parallel Provider Execution Started",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    It.Is<string>(s => s.StartsWith("Parallel Provider Execution Completed")),
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    It.Is<string>(s => s.StartsWith("Foundation Service Request Completed")),
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetStructuredRecordSerialisedShouldReturnEmptyBundleListWhenAllProvidersError()
        {
            // given
            Exception exception = new Exception(GetRandomString());
            Exception exception2 = new Exception(GetRandomString());

            Provider ddsProvider =
                new Provider { FriendlyName = "DDS Provider", FullyQualifiedName = "DDS", IsPrimary = true };

            Provider ldsProvider =
                new Provider { FriendlyName = "LDS Provider", FullyQualifiedName = "LDS", IsPrimary = false };

            List<Provider> randomProviders = new List<Provider>
            {
                ddsProvider,
                ldsProvider,
            };

            List<Provider> inputProviders = randomProviders.DeepClone();
            List<Bundle> expectedBundles = new List<Bundle>();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            string inputDateOfBirth = DateTime.Now.ToString("yyyy-MM-dd");
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
                this.securityAuditBrokerMock.Object,
                this.storageBrokerMock.Object,
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
                .ReturnsAsync((ddsProvider.FriendlyName, null, exception));

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
                .ReturnsAsync((ldsProvider.FriendlyName, null, exception2));

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            List<(string Provider, string Json)> actualBundles =
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
                broker.LogErrorAsync(It.Is(SameExceptionAsUnorderedAggregate(aggregateException))),
                    Times.Once());

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Foundation Service Request Submitted",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Parallel Provider Execution Started",
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    It.Is<string>(s => s.StartsWith("Parallel Provider Execution Completed")),
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    It.Is<string>(s => s.StartsWith("Foundation Service Request Completed")),
                    message,
                    null,
                    correlationId.ToString()),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.securityAuditBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            patientServiceMock.VerifyNoOtherCalls();
        }
    }
}
