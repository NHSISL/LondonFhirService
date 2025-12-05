// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Models.Orchestrations.Patients.Exceptions;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Patients.STU3
{
    public partial class Stu3PatientOrchestrationServiceTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnEverythingIfIdIsNullAndLogItAsync(string invalidText)
        {
            // given
            string invalidId = invalidText;
            Guid correlationId = Guid.Empty;

            var invalidArgumentPatientOrchestrationException =
                new InvalidArgumentPatientOrchestrationException(
                    message: "Invalid patient orchestration argument, please correct the errors and try again.");

            invalidArgumentPatientOrchestrationException.AddData(
                key: "Id",
                values: "Text is required");

            invalidArgumentPatientOrchestrationException.AddData(
                key: "CorrelationId",
                values: "Id is required");

            var expectedPatientOrchestrationValidationException =
                new PatientOrchestrationValidationException(
                    message: "Patient orchestration validation error occurred, please try again.",
                    innerException: invalidArgumentPatientOrchestrationException);

            // when
            ValueTask<Bundle> everythingTask =
                this.patientOrchestrationService.EverythingAsync(
                    correlationId: correlationId,
                    id: invalidId);

            PatientOrchestrationValidationException actualPatientOrchestrationValidationException =
                await Assert.ThrowsAsync<PatientOrchestrationValidationException>(
                    everythingTask.AsTask);

            // then
            actualPatientOrchestrationValidationException.Should()
                .BeEquivalentTo(expectedPatientOrchestrationValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientOrchestrationValidationException))),
                        Times.Once);

            this.providerServiceMock.VerifyNoOtherCalls();
            this.patientServiceMock.VerifyNoOtherCalls();
            this.fhirReconciliationServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnEverythingIfNoPrimaryProvidersAndLogItAsync()
        {
            // given
            string randomId = GetRandomString();
            string inputId = randomId;
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            Provider randomActiveProvider = CreateRandomActiveProvider();
            Provider randomInactiveProvider = CreateRandomInactiveProvider();

            IQueryable<Provider> allProviders = new List<Provider>
            {
                randomInactiveProvider,
                randomActiveProvider,
            }.AsQueryable();

            this.providerServiceMock.Setup(service =>
                service.RetrieveAllProvidersAsync())
                    .ReturnsAsync(allProviders);

            var invalidPrimaryProviderPatientOrchestrationException =
                new InvalidPrimaryProviderPatientOrchestrationException(
                    message: "No active primary provider found. One active primary provider required.");

            var expectedPatientOrchestrationValidationException =
                new PatientOrchestrationValidationException(
                    message: "Patient orchestration validation error occurred, please try again.",
                    innerException: invalidPrimaryProviderPatientOrchestrationException);

            // when
            ValueTask<Bundle> everythingTask =
                this.patientOrchestrationService.EverythingAsync(
                    correlationId: correlationId,
                    id: inputId,
                    cancellationToken: cancellationToken);

            PatientOrchestrationValidationException actualPatientOrchestrationValidationException =
                await Assert.ThrowsAsync<PatientOrchestrationValidationException>(
                    everythingTask.AsTask);

            // then
            actualPatientOrchestrationValidationException.Should()
                .BeEquivalentTo(expectedPatientOrchestrationValidationException);

            this.providerServiceMock.Verify(service =>
                service.RetrieveAllProvidersAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientOrchestrationValidationException))),
                        Times.Once);

            this.providerServiceMock.VerifyNoOtherCalls();
            this.patientServiceMock.VerifyNoOtherCalls();
            this.fhirReconciliationServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnEverythingIfMultiplePrimaryProvidersAndLogItAsync()
        {
            // given
            string randomId = GetRandomString();
            string inputId = randomId;
            DateTimeOffset? inputStart = GetRandomDateTimeOffset();
            DateTimeOffset? inputEnd = GetRandomDateTimeOffset();
            string inputTypeFilter = GetRandomString();
            DateTimeOffset? inputSince = GetRandomDateTimeOffset();
            int? inputCount = GetRandomNumber();
            CancellationToken cancellationToken = CancellationToken.None;
            List<Bundle> randomBundles = CreateRandomBundles();
            Bundle randomBundle = CreateRandomBundle();
            Bundle expectedBundle = randomBundle.DeepClone();
            Guid correlationId = Guid.NewGuid();
            string auditType = "STU3-Patient-Everything";

            string message =
                $"Parameters:  {{ id = \"{inputId}\", start = \"{inputStart}\", " +
                $"end = \"{inputEnd}\", typeFilter = \"{inputTypeFilter}\", " +
                $"since = \"{inputSince}\", count = \"{inputCount}\" }}";

            Provider randomPrimaryProvider = CreateRandomPrimaryProvider();
            Provider anotherRandomPrimaryProvider = CreateRandomPrimaryProvider();

            IQueryable<Provider> allProviders = new List<Provider>
            {
                randomPrimaryProvider,
                anotherRandomPrimaryProvider,
            }.AsQueryable();

            this.providerServiceMock.Setup(service =>
                service.RetrieveAllProvidersAsync())
                    .ReturnsAsync(allProviders);

            var invalidPrimaryProviderPatientOrchestrationException =
                new InvalidPrimaryProviderPatientOrchestrationException(
                    message:
                        $"Multiple active providers found: " +
                        $"{string.Join(", ", allProviders.Select(provider => provider.Name))}. " +
                        $"Only one active primary provider required.");

            var expectedPatientOrchestrationValidationException =
                new PatientOrchestrationValidationException(
                    message: "Patient orchestration validation error occurred, please try again.",
                    innerException: invalidPrimaryProviderPatientOrchestrationException);

            // when
            ValueTask<Bundle> everythingTask =
                this.patientOrchestrationService.EverythingAsync(
                    correlationId,
                    inputId,
                    inputStart,
                    inputEnd,
                    inputTypeFilter,
                    inputSince,
                    inputCount,
                    cancellationToken);

            PatientOrchestrationValidationException actualPatientOrchestrationValidationException =
                await Assert.ThrowsAsync<PatientOrchestrationValidationException>(
                    everythingTask.AsTask);

            // then
            actualPatientOrchestrationValidationException.Should()
                .BeEquivalentTo(expectedPatientOrchestrationValidationException);

            this.providerServiceMock.Verify(service =>
                service.RetrieveAllProvidersAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientOrchestrationValidationException))),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Orchestration Service Request Submitted",
                    message,
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.auditBrokerMock.Verify(broker =>
                broker.LogInformationAsync(
                    auditType,
                    "Retrieve active providers and execute request",
                    message,
                    string.Empty,
                    correlationId.ToString()),
                        Times.Once);

            this.providerServiceMock.VerifyNoOtherCalls();
            this.patientServiceMock.VerifyNoOtherCalls();
            this.fhirReconciliationServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }
    }
}
