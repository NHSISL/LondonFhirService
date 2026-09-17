// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.FhirRecordDifferences.Exceptions;
using LondonFhirService.Core.Models.Foundations.FhirRecords.Exceptions;
using LondonFhirService.Core.Models.Orchestrations.CompareQueue.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Orchestrations.CompareQueue
{
    internal partial class CompareQueueOrchestrationService
    {
        private delegate ValueTask<T> ReturningValueFunction<T>();
        private delegate ValueTask ReturningNothingFunction();

        /// <summary>
        /// One mapping for every method on this service. There used to be three copies of this
        /// catch ladder - one per return shape - which is how they drifted: the difference-service
        /// exceptions were only ever caught on the void copy, so the same exception would have
        /// surfaced as a service exception on one method and a dependency exception on another.
        /// The return type is the only thing that varied, so it is the only thing left generic.
        /// </summary>
        private async ValueTask<T> TryCatch<T>(ReturningValueFunction<T> returningValueFunction)
        {
            try
            {
                return await returningValueFunction();
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

        /// <summary>
        /// The methods that return nothing map their exceptions the same way, so they run through
        /// the same ladder and discard the placeholder.
        /// </summary>
        private async ValueTask TryCatch(ReturningNothingFunction returningNothingFunction) =>
            await TryCatch<bool>(async () =>
            {
                await returningNothingFunction();

                return true;
            });

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
                    innerException: exception.InnerException as Xeption);

            await this.loggingBroker.LogErrorAsync(compareQueueOrchestrationDependencyValidationException);

            return compareQueueOrchestrationDependencyValidationException;
        }

        private async ValueTask<CompareQueueOrchestrationDependencyException>
            CreateAndLogDependencyExceptionAsync(Xeption exception)
        {
            var compareQueueOrchestrationDependencyException =
                new CompareQueueOrchestrationDependencyException(
                    message: "Compare queue orchestration dependency error occurred, please contact support.",
                    innerException: exception.InnerException as Xeption);

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
