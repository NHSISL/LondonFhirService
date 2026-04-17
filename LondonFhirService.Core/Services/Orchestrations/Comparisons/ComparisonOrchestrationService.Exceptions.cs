// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Orchestrations.Comparisons;
using LondonFhirService.Core.Models.Orchestrations.Comparisons.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Orchestrations.Comparisons
{
    public partial class ComparisonOrchestrationService
    {
        private delegate ValueTask<ComparisonResult> ReturningComparisonResultFunction();

        private async ValueTask<ComparisonResult> TryCatch(
            ReturningComparisonResultFunction returningComparisonResultFunction)
        {
            try
            {
                return await returningComparisonResultFunction();
            }
            catch (InvalidComparisonOrchestrationException invalidComparisonOrchestrationException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidComparisonOrchestrationException);
            }
            catch (AggregateException aggregateException)
            {
                throw await CreateAndLogServiceExceptionAsync(aggregateException);
            }
            catch (Exception exception)
            {
                var failedComparisonOrchestrationServiceException =
                    new FailedComparisonOrchestrationServiceException(
                        message: "Failed comparison orchestration service error occurred, please contact support.",
                        innerException: exception,
                        data: exception.Data);

                throw await CreateAndLogServiceExceptionAsync(failedComparisonOrchestrationServiceException);
            }
        }

        private async ValueTask<ComparisonOrchestrationValidationException>
            CreateAndLogValidationExceptionAsync(Xeption exception)
        {
            var comparisonOrchestrationValidationException =
                new ComparisonOrchestrationValidationException(
                    message: "Comparison orchestration validation error occurred, " +
                        "fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(comparisonOrchestrationValidationException);

            return comparisonOrchestrationValidationException;
        }

        private async ValueTask<ComparisonOrchestrationServiceException>
            CreateAndLogServiceExceptionAsync(Exception exception)
        {
            var comparisonOrchestrationServiceException =
                new ComparisonOrchestrationServiceException(
                    message: "Comparison orchestration service error occurred, please contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(comparisonOrchestrationServiceException);

            return comparisonOrchestrationServiceException;
        }
    }
}
