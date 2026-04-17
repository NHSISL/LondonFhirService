// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.JsonElements.Exceptions;
using LondonFhirService.Core.Models.Orchestrations.Comparisons;
using LondonFhirService.Core.Models.Orchestrations.Comparisons.Exceptions;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.ArrayOrderIgnoreRules.Exceptions;
using LondonFhirService.Core.Models.Processings.ListEntryComparisons.Exceptions;
using LondonFhirService.Core.Models.Processings.ResourceMatchings.Exceptions;
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
            catch (ResourceMatcherProcessingValidationException resourceMatcherProcessingValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    resourceMatcherProcessingValidationException);
            }
            catch (ResourceMatcherProcessingServiceException resourceMatcherProcessingServiceException)
            {
                throw await CreateAndLogDependencyExceptionAsync(resourceMatcherProcessingServiceException);
            }
            catch (ListEntryComparisonProcessingValidationException listEntryComparisonValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(listEntryComparisonValidationException);
            }
            catch (ListEntryComparisonProcessingServiceException listEntryComparisonServiceException)
            {
                throw await CreateAndLogDependencyExceptionAsync(listEntryComparisonServiceException);
            }
            catch (JsonElementServiceValidationException jsonElementServiceValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(jsonElementServiceValidationException);
            }
            catch (JsonElementServiceDependencyValidationException jsonElementDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(jsonElementDependencyValidationException);
            }
            catch (JsonElementServiceException jsonElementServiceException)
            {
                throw await CreateAndLogDependencyExceptionAsync(jsonElementServiceException);
            }
            catch (JsonElementServiceDependencyException jsonElementDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(jsonElementDependencyException);
            }
            catch (JsonIgnoreRulesProcessingValidationException jsonIgnoreRulesValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(jsonIgnoreRulesValidationException);
            }
            catch (JsonIgnoreRulesProcessingDependencyValidationException jsonIgnoreRulesDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    jsonIgnoreRulesDependencyValidationException);
            }
            catch (JsonIgnoreRulesProcessingServiceException jsonIgnoreRulesServiceException)
            {
                throw await CreateAndLogDependencyExceptionAsync(jsonIgnoreRulesServiceException);
            }
            catch (JsonIgnoreRulesProcessingDependencyException jsonIgnoreRulesDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(jsonIgnoreRulesDependencyException);
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

        private async ValueTask<ComparisonOrchestrationDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var comparisonOrchestrationDependencyValidationException =
                new ComparisonOrchestrationDependencyValidationException(
                    message: "Comparison orchestration dependency validation error occurred, " +
                        "fix the errors and try again.",
                    innerException: exception.InnerException as Xeption);

            await this.loggingBroker.LogErrorAsync(comparisonOrchestrationDependencyValidationException);

            return comparisonOrchestrationDependencyValidationException;
        }

        private async ValueTask<ComparisonOrchestrationDependencyException>
            CreateAndLogDependencyExceptionAsync(Xeption exception)
        {
            var comparisonOrchestrationDependencyException =
                new ComparisonOrchestrationDependencyException(
                    message: "Comparison orchestration dependency error occurred, " +
                        "fix the errors and try again.",
                    innerException: exception.InnerException as Xeption);

            await this.loggingBroker.LogErrorAsync(comparisonOrchestrationDependencyException);

            return comparisonOrchestrationDependencyException;
        }

        private async ValueTask<ComparisonOrchestrationServiceException>
            CreateAndLogServiceExceptionAsync(Xeption exception)
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
