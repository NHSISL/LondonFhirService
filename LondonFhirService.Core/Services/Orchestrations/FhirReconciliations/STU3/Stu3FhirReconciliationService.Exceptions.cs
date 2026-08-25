// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Orchestrations.FhirReconciliations.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Orchestrations.FhirReconciliations.STU3
{
    internal partial class Stu3FhirReconciliationService : IStu3FhirReconciliationService
    {
        private delegate ValueTask<string> ReturningStringFunction();

        private async ValueTask<string> TryCatch(ReturningStringFunction returningStringFunction)
        {
            try
            {
                return await returningStringFunction();
            }
            catch (NotFoundFhirReconciliationOrchestrationException
                   notFoundFhirReconciliationOrchestrationException)
            {
                throw await CreateAndLogValidationExceptionAsync(
                    notFoundFhirReconciliationOrchestrationException);
            }
            catch (Exception exception)
            {
                var failedFhirReconciliationOrchestrationException =
                    new FailedFhirReconciliationOrchestrationException(
                        message: "Failed FHIR reconciliation orchestration error occurred, " +
                            "please contact support.",
                        innerException: exception,
                        data: exception.Data);

                throw await CreateAndLogServiceExceptionAsync(failedFhirReconciliationOrchestrationException);
            }
        }

        private async ValueTask<FhirReconciliationOrchestrationValidationException>
            CreateAndLogValidationExceptionAsync(Xeption exception)
        {
            var fhirReconciliationOrchestrationValidationException =
                new FhirReconciliationOrchestrationValidationException(
                    message: "FHIR reconciliation orchestration validation error occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(fhirReconciliationOrchestrationValidationException);

            return fhirReconciliationOrchestrationValidationException;
        }

        private async ValueTask<FhirReconciliationOrchestrationServiceException>
            CreateAndLogServiceExceptionAsync(Xeption exception)
        {
            var fhirReconciliationOrchestrationServiceException =
                new FhirReconciliationOrchestrationServiceException(
                    message: "FHIR reconciliation orchestration service error occurred, please contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(fhirReconciliationOrchestrationServiceException);

            return fhirReconciliationOrchestrationServiceException;
        }
    }
}
