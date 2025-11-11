// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Hl7.Fhir.Model;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Models.Orchestrations.Patients.Exceptions;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Patients.R4
{
    public partial class R4PatientOrchestrationServiceTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnEverythingIfIdIsNullAndLogItAsync(string invalidText)
        {
            // given
            string invalidId = invalidText;

            var invalidArgumentPatientOrchestrationException =
                new InvalidArgumentPatientOrchestrationException(
                    message: "Invalid patient orchestration argument, please correct the errors and try again.");

            invalidArgumentPatientOrchestrationException.AddData(
                key: "Id",
                values: "Text is required");

            var expectedPatientOrchestrationValidationException =
                new PatientOrchestrationValidationException(
                    message: "Patient orchestration validation error occurred, please try again.",
                    innerException: invalidArgumentPatientOrchestrationException);

            // when
            ValueTask<Bundle> everythingTask =
                this.patientOrchestrationService.Everything(id: invalidId);

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
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnEverythingIfNoPrimaryProvidersAndLogItAsync()
        {
            // given
            string randomId = GetRandomString();
            string inputId = randomId;
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            CancellationToken cancellationToken = CancellationToken.None;

            Provider randomActiveProvider = CreateRandomActiveProvider(randomDateTimeOffset);
            Provider randomInactiveProvider = CreateRandomInactiveProvider(randomDateTimeOffset);

            IQueryable<Provider> allProviders = new List<Provider>
            {
                randomInactiveProvider,
                randomActiveProvider,
            }.AsQueryable();

            this.providerServiceMock.Setup(service =>
                service.RetrieveAllProvidersAsync())
                    .ReturnsAsync(allProviders);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            var invalidPrimaryProviderPatientOrchestrationException =
                new InvalidPrimaryProviderPatientOrchestrationException(
                    message: "No active primary provider found. One active primary provider required.");

            var expectedPatientOrchestrationValidationException =
                new PatientOrchestrationValidationException(
                    message: "Patient orchestration validation error occurred, please try again.",
                    innerException: invalidPrimaryProviderPatientOrchestrationException);

            // when
            ValueTask<Bundle> everythingTask =
                this.patientOrchestrationService.Everything(id: inputId, cancellationToken: cancellationToken);

            PatientOrchestrationValidationException actualPatientOrchestrationValidationException =
                await Assert.ThrowsAsync<PatientOrchestrationValidationException>(
                    everythingTask.AsTask);

            // then
            actualPatientOrchestrationValidationException.Should()
                .BeEquivalentTo(expectedPatientOrchestrationValidationException);

            this.providerServiceMock.Verify(service =>
                service.RetrieveAllProvidersAsync(),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientOrchestrationValidationException))),
                        Times.Once);

            this.providerServiceMock.VerifyNoOtherCalls();
            this.patientServiceMock.VerifyNoOtherCalls();
            this.fhirReconciliationServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnEverythingIfMultiplePrimaryProvidersAndLogItAsync()
        {
            // given
            string randomId = GetRandomString();
            string inputId = randomId;
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            CancellationToken cancellationToken = CancellationToken.None;

            Provider randomPrimaryProvider = CreateRandomPrimaryProvider(randomDateTimeOffset);
            Provider anotherRandomPrimaryProvider = CreateRandomPrimaryProvider(randomDateTimeOffset);

            IQueryable<Provider> allProviders = new List<Provider>
            {
                randomPrimaryProvider,
                anotherRandomPrimaryProvider,
            }.AsQueryable();

            this.providerServiceMock.Setup(service =>
                service.RetrieveAllProvidersAsync())
                    .ReturnsAsync(allProviders);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

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
                this.patientOrchestrationService.Everything(id: inputId, cancellationToken: cancellationToken);

            PatientOrchestrationValidationException actualPatientOrchestrationValidationException =
                await Assert.ThrowsAsync<PatientOrchestrationValidationException>(
                    everythingTask.AsTask);

            // then
            actualPatientOrchestrationValidationException.Should()
                .BeEquivalentTo(expectedPatientOrchestrationValidationException);

            this.providerServiceMock.Verify(service =>
                service.RetrieveAllProvidersAsync(),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientOrchestrationValidationException))),
                        Times.Once);

            this.providerServiceMock.VerifyNoOtherCalls();
            this.patientServiceMock.VerifyNoOtherCalls();
            this.fhirReconciliationServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }
    }
}
