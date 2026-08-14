// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Models.Orchestrations.FhirReconciliations.Exceptions;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.FhirReconciliations.STU3
{
    public partial class Stu3FhirReconciliationServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnReconcileSerialisedIfNoBundlesAndLogItAsync()
        {
            // given
            List<(string Provider, string Json)> emptyBundles = new List<(string Provider, string Json)>();
            string randomNhsNumber = GetRandomString();
            Provider randomPrimaryProvider = CreateRandomProvider();
            Guid correlationId = Guid.NewGuid();

            var notFoundFhirReconciliationOrchestrationException =
                new NotFoundFhirReconciliationOrchestrationException(
                    $"NotFound:Patient resource with id = '{randomNhsNumber}' not found.  " +
                    $"CorrelationId: {correlationId.ToString()}");

            var expectedFhirReconciliationOrchestrationValidationException =
                new FhirReconciliationOrchestrationValidationException(
                    message: "FHIR reconciliation orchestration validation error occurred, please try again.",
                    innerException: notFoundFhirReconciliationOrchestrationException);

            // when
            ValueTask<string> reconcileSerialisedTask =
                this.fhirReconciliationService.ReconcileSerialisedAsync(
                    bundles: emptyBundles,
                    nhsNumber: randomNhsNumber,
                    primaryProvider: randomPrimaryProvider,
                    correlationId: correlationId);

            FhirReconciliationOrchestrationValidationException
                actualFhirReconciliationOrchestrationValidationException =
                    await Assert.ThrowsAsync<FhirReconciliationOrchestrationValidationException>(
                        testCode: reconcileSerialisedTask.AsTask);

            // then
            actualFhirReconciliationOrchestrationValidationException.Should()
                .BeEquivalentTo(expectedFhirReconciliationOrchestrationValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirReconciliationOrchestrationValidationException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnReconcileSerialisedIfEveryBundleIsEmptyAndLogItAsync()
        {
            // given
            List<(string Provider, string Json)> emptyBundles = new List<(string Provider, string Json)>
            {
                (GetRandomString(), null),
                (GetRandomString(), string.Empty)
            };

            string randomNhsNumber = GetRandomString();
            Provider randomPrimaryProvider = CreateRandomProvider();
            Guid correlationId = Guid.NewGuid();

            var notFoundFhirReconciliationOrchestrationException =
                new NotFoundFhirReconciliationOrchestrationException(
                    $"NotFound:Patient resource with id = '{randomNhsNumber}' not found.  " +
                    $"CorrelationId: {correlationId.ToString()}");

            var expectedFhirReconciliationOrchestrationValidationException =
                new FhirReconciliationOrchestrationValidationException(
                    message: "FHIR reconciliation orchestration validation error occurred, please try again.",
                    innerException: notFoundFhirReconciliationOrchestrationException);

            // when
            ValueTask<string> reconcileSerialisedTask =
                this.fhirReconciliationService.ReconcileSerialisedAsync(
                    bundles: emptyBundles,
                    nhsNumber: randomNhsNumber,
                    primaryProvider: randomPrimaryProvider,
                    correlationId: correlationId);

            FhirReconciliationOrchestrationValidationException
                actualFhirReconciliationOrchestrationValidationException =
                    await Assert.ThrowsAsync<FhirReconciliationOrchestrationValidationException>(
                        testCode: reconcileSerialisedTask.AsTask);

            // then
            actualFhirReconciliationOrchestrationValidationException.Should()
                .BeEquivalentTo(expectedFhirReconciliationOrchestrationValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedFhirReconciliationOrchestrationValidationException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
