// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirReconciliations.Exceptions;
using LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.FhirReconciliations.STU3
{
    public partial class Stu3FhirReconciliationService : IStu3FhirReconciliationService
    {
        private delegate ValueTask<string> ReturningStringFunction();

        private async ValueTask<string> TryCatch(ReturningStringFunction returningStringFunction)
        {
            try
            {
                return await returningStringFunction();
            }
            catch (ResourceNotFoundException invalidFhirReconciliationServiceException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidFhirReconciliationServiceException);
            }
            catch (Exception exception)
            {
                var failedFhirReconciliationServiceException =
                    new FailedFhirReconciliationServiceException(
                        message: "Failed patient service error occurred, please contact support.",
                        innerException: exception,
                        data: exception.Data);

                throw await CreateAndLogServiceExceptionAsync(failedFhirReconciliationServiceException);
            }
        }

        private async ValueTask<FhirReconciliationServiceValidationException>
            CreateAndLogValidationExceptionAsync(Xeption exception)
        {
            var fhirReconciliationServiceValidationException =
                new FhirReconciliationServiceValidationException(
                    message: "STU3 FHIR reconciliation service validation error occurred, " +
                        "please fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(fhirReconciliationServiceValidationException);

            return fhirReconciliationServiceValidationException;
        }

        private async ValueTask<FhirReconciliationServiceException> CreateAndLogServiceExceptionAsync(Xeption exception)
        {
            var fhirReconciliationServiceException = new FhirReconciliationServiceException(
                message: "STU3 FHIR reconciliation service error occurred, contact support.",
                innerException: exception);

            await this.loggingBroker.LogErrorAsync(fhirReconciliationServiceException);

            return fhirReconciliationServiceException;
        }
    }
}
