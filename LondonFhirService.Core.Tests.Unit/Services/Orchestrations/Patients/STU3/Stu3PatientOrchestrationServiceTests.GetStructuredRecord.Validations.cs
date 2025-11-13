// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

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

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Patients.STU3
{
    public partial class Stu3PatientOrchestrationServiceTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnGetStructuredRecordIfIdIsNullAndLogItAsync(string invalidText)
        {
            // given
            string invalidNhsNumber = invalidText;

            var invalidArgumentPatientOrchestrationException =
                new InvalidArgumentPatientOrchestrationException(
                    message: "Invalid patient orchestration argument, please correct the errors and try again.");

            invalidArgumentPatientOrchestrationException.AddData(
                key: "NhsNumber",
                values: "Text is required");

            var expectedPatientOrchestrationValidationException =
                new PatientOrchestrationValidationException(
                    message: "Patient orchestration validation error occurred, please try again.",
                    innerException: invalidArgumentPatientOrchestrationException);

            // when
            ValueTask<Bundle> everythingTask =
                this.patientOrchestrationService.GetStructuredRecord(nhsNumber: invalidNhsNumber);

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
        public async Task ShouldThrowValidationExceptionOnGetStructuredRecordIfNoPrimaryProvidersAndLogItAsync()
        {
            // given
            string randomId = GetRandomString();
            string inputNhsNumber = randomId;
            CancellationToken cancellationToken = CancellationToken.None;

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
            ValueTask<Bundle> everythingTask = this.patientOrchestrationService
                .GetStructuredRecord(nhsNumber: inputNhsNumber, cancellationToken: cancellationToken);

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
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnGetStructuredRecordIfMultiplePrimaryProvidersAndLogItAsync()
        {
            // given
            string randomId = GetRandomString();
            string inputId = randomId;
            CancellationToken cancellationToken = CancellationToken.None;

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
            ValueTask<Bundle> everythingTask = this.patientOrchestrationService
                .GetStructuredRecord(nhsNumber: inputId, cancellationToken: cancellationToken);

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
        }
    }
}
