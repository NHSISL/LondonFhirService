// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences.Exceptions;
using LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Orchestrations.CompareQueue
{
    public partial class CompareQueueOrchestrationService
    {
        private delegate ValueTask<CompareQueueItem> ReturningCompareQueueItemFunction();
        private delegate ValueTask ReturningNothingFunction();

        private async ValueTask<CompareQueueItem> TryCatch(
            ReturningCompareQueueItemFunction returningCompareQueueItemFunction)
        {
            try
            {
                return await returningCompareQueueItemFunction();
            }
            catch (NullCompareQueueItemException nullCompareQueueItemException)
            {
                throw await CreateAndLogValidationExceptionAsync(nullCompareQueueItemException);
            }
            catch (InvalidCompareQueueOrchestrationException invalidCompareQueueOrchestrationException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidCompareQueueOrchestrationException);
            }
            catch (FhirRecordValidationException fhirRecordValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(fhirRecordValidationException);
            }
            catch (FhirRecordDependencyValidationException fhirRecordDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    fhirRecordDependencyValidationException);
            }
            catch (FhirRecordDependencyException fhirRecordDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(fhirRecordDependencyException);
            }
            catch (FhirRecordServiceException fhirRecordServiceException)
            {
                throw await CreateAndLogDependencyExceptionAsync(fhirRecordServiceException);
            }
            catch (Exception exception)
            {
                var failedCompareQueueOrchestrationServiceException =
                    new FailedCompareQueueOrchestrationServiceException(
                        message: "Failed compare queue orchestration service error occurred, please contact support.",
                        innerException: exception,
                        data: exception.Data);

                throw await CreateAndLogServiceExceptionAsync(
                    failedCompareQueueOrchestrationServiceException);
            }
        }

        private async ValueTask TryCatch(ReturningNothingFunction returningNothingFunction)
        {
            try
            {
                await returningNothingFunction();
            }
            catch (NullCompareQueueItemException nullCompareQueueItemException)
            {
                throw await CreateAndLogValidationExceptionAsync(nullCompareQueueItemException);
            }
            catch (InvalidCompareQueueOrchestrationException invalidCompareQueueOrchestrationException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidCompareQueueOrchestrationException);
            }
            catch (FhirRecordValidationException fhirRecordValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(fhirRecordValidationException);
            }
            catch (FhirRecordDependencyValidationException fhirRecordDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    fhirRecordDependencyValidationException);
            }
            catch (FhirRecordDependencyException fhirRecordDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(fhirRecordDependencyException);
            }
            catch (FhirRecordServiceException fhirRecordServiceException)
            {
                throw await CreateAndLogDependencyExceptionAsync(fhirRecordServiceException);
            }
            catch (FhirRecordDifferenceValidationException fhirRecordDifferenceValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    fhirRecordDifferenceValidationException);
            }
            catch (FhirRecordDifferenceDependencyValidationException
                fhirRecordDifferenceDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    fhirRecordDifferenceDependencyValidationException);
            }
            catch (FhirRecordDifferenceDependencyException fhirRecordDifferenceDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(fhirRecordDifferenceDependencyException);
            }
            catch (FhirRecordDifferenceServiceException fhirRecordDifferenceServiceException)
            {
                throw await CreateAndLogDependencyExceptionAsync(fhirRecordDifferenceServiceException);
            }
            catch (Exception exception)
            {
                var failedCompareQueueOrchestrationServiceException =
                    new FailedCompareQueueOrchestrationServiceException(
                        message: "Failed compare queue orchestration service error occurred, please contact support.",
                        innerException: exception,
                        data: exception.Data);

                throw await CreateAndLogServiceExceptionAsync(
                    failedCompareQueueOrchestrationServiceException);
            }
        }

        private async ValueTask<CompareQueueOrchestrationValidationException>
            CreateAndLogValidationExceptionAsync(Xeption exception)
        {
            var compareQueueOrchestrationValidationException =
                new CompareQueueOrchestrationValidationException(
                    message: "Compare queue orchestration validation error occurred, fix errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(compareQueueOrchestrationValidationException);

            return compareQueueOrchestrationValidationException;
        }

        private async ValueTask<CompareQueueOrchestrationDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var compareQueueOrchestrationDependencyValidationException =
                new CompareQueueOrchestrationDependencyValidationException(
                    message: "Compare queue orchestration dependency validation error occurred, " +
                        "fix errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(compareQueueOrchestrationDependencyValidationException);

            return compareQueueOrchestrationDependencyValidationException;
        }

        private async ValueTask<CompareQueueOrchestrationDependencyException>
            CreateAndLogDependencyExceptionAsync(Xeption exception)
        {
            var compareQueueOrchestrationDependencyException =
                new CompareQueueOrchestrationDependencyException(
                    message: "Compare queue orchestration dependency error occurred, please contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(compareQueueOrchestrationDependencyException);

            return compareQueueOrchestrationDependencyException;
        }

        private async ValueTask<CompareQueueOrchestrationServiceException>
            CreateAndLogServiceExceptionAsync(Xeption exception)
        {
            var compareQueueOrchestrationServiceException =
                new CompareQueueOrchestrationServiceException(
                    message: "Compare queue orchestration service error occurred, please contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(compareQueueOrchestrationServiceException);

            return compareQueueOrchestrationServiceException;
        }
    }
}
