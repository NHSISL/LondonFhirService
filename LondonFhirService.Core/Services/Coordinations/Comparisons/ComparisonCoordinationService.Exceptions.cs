// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Coordinations.Comparisons.Exceptions;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue.Exceptions;
using LondonFhirService.Core.Models.Orchestrations.Comparisons.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Coordinations.Patients.STU3
{
    public partial class ComparisonCoordinationService
    {
        private delegate ValueTask ReturningValueTaskFunction();

        private async ValueTask TryCatch(ReturningValueTaskFunction returningValueTaskFunction)
        {
            try
            {
                await returningValueTaskFunction();
            }
            catch (InvalidArgumentComparisonCoordinationException invalidArgumentComparisonCoordinationException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidArgumentComparisonCoordinationException);
            }
            catch (CompareQueueOrchestrationValidationException compareQueueOrchestrationValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    compareQueueOrchestrationValidationException);
            }
            catch (CompareQueueOrchestrationDependencyValidationException
                compareQueueOrchestrationDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    compareQueueOrchestrationDependencyValidationException);
            }
            catch (ComparisonOrchestrationValidationException comparisonOrchestrationValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    comparisonOrchestrationValidationException);
            }
            catch (ComparisonOrchestrationDependencyValidationException
                comparisonOrchestrationDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    comparisonOrchestrationDependencyValidationException);
            }
            catch (CompareQueueOrchestrationDependencyException compareQueueOrchestrationDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(compareQueueOrchestrationDependencyException);
            }
            catch (CompareQueueOrchestrationServiceException compareQueueOrchestrationServiceException)
            {
                throw await CreateAndLogDependencyExceptionAsync(compareQueueOrchestrationServiceException);
            }
            catch (ComparisonOrchestrationDependencyException comparisonOrchestrationDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(comparisonOrchestrationDependencyException);
            }
            catch (ComparisonOrchestrationServiceException comparisonOrchestrationServiceException)
            {
                throw await CreateAndLogDependencyExceptionAsync(comparisonOrchestrationServiceException);
            }
            catch (Exception exception)
            {
                var failedComparisonCoordinationException =
                    new FailedComparisonCoordinationException(
                        message: "Failed comparison coordination service error occurred, please contact support.",
                        innerException: exception,
                        data: exception.Data);

                throw await CreateAndLogServiceExceptionAsync(failedComparisonCoordinationException);
            }
        }

        private async ValueTask<ComparisonCoordinationValidationException> CreateAndLogValidationExceptionAsync(
            InvalidArgumentComparisonCoordinationException invalidArgumentComparisonCoordinationException)
        {
            var comparisonCoordinationValidationException =
                new ComparisonCoordinationValidationException(
                    message: "Comparison coordination validation error occurred, please try again.",
                    innerException: invalidArgumentComparisonCoordinationException);

            await this.loggingBroker.LogErrorAsync(comparisonCoordinationValidationException);

            return comparisonCoordinationValidationException;
        }

        private async ValueTask<ComparisonCoordinationDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var comparisonCoordinationDependencyValidationException =
                new ComparisonCoordinationDependencyValidationException(
                    message: "Comparison coordination dependency validation error occurred, please try again.",
                    innerException: exception.InnerException as Xeption);

            await this.loggingBroker.LogErrorAsync(comparisonCoordinationDependencyValidationException);

            return comparisonCoordinationDependencyValidationException;
        }

        private async ValueTask<ComparisonCoordinationDependencyException> CreateAndLogDependencyExceptionAsync(
            Xeption exception)
        {
            var comparisonCoordinationDependencyException =
                new ComparisonCoordinationDependencyException(
                    message: "Comparison coordination dependency error occurred, fix the errors and try again.",
                    innerException: exception.InnerException as Xeption);

            await this.loggingBroker.LogErrorAsync(comparisonCoordinationDependencyException);

            return comparisonCoordinationDependencyException;
        }

        private async ValueTask<ComparisonCoordinationServiceException> CreateAndLogServiceExceptionAsync(
            Xeption exception)
        {
            var comparisonCoordinationServiceException =
                new ComparisonCoordinationServiceException(
                    message: "Comparison coordination service error occurred, please contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(comparisonCoordinationServiceException);

            return comparisonCoordinationServiceException;
        }
    }
}
