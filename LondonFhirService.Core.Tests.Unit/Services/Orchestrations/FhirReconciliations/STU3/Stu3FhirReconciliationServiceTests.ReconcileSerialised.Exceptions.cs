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
        public async Task ShouldThrowServiceExceptionOnReconcileSerialisedIfServiceErrorOccursAndLogItAsync()
        {
            // given
            List<(string Provider, string Json)> nullBundles = null;
            string randomNhsNumber = GetRandomString();
            Provider randomPrimaryProvider = CreateRandomProvider();
            Guid correlationId = Guid.NewGuid();

            // when
            ValueTask<string> reconcileSerialisedTask =
                this.fhirReconciliationService.ReconcileSerialisedAsync(
                    bundles: nullBundles,
                    nhsNumber: randomNhsNumber,
                    primaryProvider: randomPrimaryProvider,
                    correlationId: correlationId);

            FhirReconciliationOrchestrationServiceException actualFhirReconciliationOrchestrationServiceException =
                await Assert.ThrowsAsync<FhirReconciliationOrchestrationServiceException>(
                    testCode: reconcileSerialisedTask.AsTask);

            // then
            actualFhirReconciliationOrchestrationServiceException.Message.Should()
                .Be("FHIR reconciliation orchestration service error occurred, please contact support.");

            actualFhirReconciliationOrchestrationServiceException.InnerException.Should()
                .BeOfType<FailedFhirReconciliationOrchestrationException>();

            actualFhirReconciliationOrchestrationServiceException.InnerException.Message.Should()
                .Be("Failed FHIR reconciliation orchestration error occurred, please contact support.");

            actualFhirReconciliationOrchestrationServiceException.InnerException.InnerException.Should()
                .BeOfType<ArgumentNullException>();

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<FhirReconciliationOrchestrationServiceException>()),
                    Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
