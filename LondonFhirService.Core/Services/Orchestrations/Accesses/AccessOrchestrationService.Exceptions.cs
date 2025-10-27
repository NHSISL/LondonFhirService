// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;
using LondonFhirService.Core.Models.Foundations.Consumers.Exceptions;
using LondonFhirService.Core.Models.Foundations.PdsDatas.Exceptions;
using LondonFhirService.Core.Models.Orchestrations.Accesses.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Orchestrations.Accesses
{
    public partial class AccessOrchestrationService
    {
        private delegate ValueTask ReturningNothingFunction();

        private async ValueTask TryCatch(ReturningNothingFunction returningNothingFunction)
        {
            try
            {
                await returningNothingFunction();
            }
            catch (InvalidArgumentAccessOrchestrationException invalidArgumentAccessOrchestrationException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidArgumentAccessOrchestrationException);
            }
            catch (UnauthorizedAccessOrchestrationException unauthorizedAccessOrchestrationException)
            {
                throw await CreateAndLogValidationExceptionAsync(unauthorizedAccessOrchestrationException);
            }
            catch (ForbiddenAccessOrchestrationException forbiddenAccessOrchestrationException)
            {
                throw await CreateAndLogValidationExceptionAsync(forbiddenAccessOrchestrationException);
            }
            catch (ConsumerServiceValidationException consumerValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    consumerValidationException);
            }
            catch (ConsumerServiceException consumerServiceException)
            {
                throw await CreateAndLogDependencyExceptionAsync(
                    consumerServiceException);
            }
            catch (ConsumerServiceDependencyException consumerServiceDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(
                    consumerServiceDependencyException);
            }
            catch (ConsumerServiceDependencyValidationException consumerDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    consumerDependencyValidationException);
            }
            catch (ConsumerAccessServiceValidationException consumerAccessValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    consumerAccessValidationException);
            }
            catch (ConsumerAccessServiceException consumerAccessServiceException)
            {
                throw await CreateAndLogDependencyExceptionAsync(
                    consumerAccessServiceException);
            }
            catch (ConsumerAccessServiceDependencyException consumerAccessServiceDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(
                    consumerAccessServiceDependencyException);
            }
            catch (ConsumerAccessServiceDependencyValidationException consumerAccessDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    consumerAccessDependencyValidationException);
            }
            catch (PdsDataServiceValidationException pdsDataServiceValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    pdsDataServiceValidationException);
            }
            catch (PdsDataServiceException pdsDataServiceException)
            {
                throw await CreateAndLogDependencyExceptionAsync(
                    pdsDataServiceException);
            }
            catch (PdsDataServiceDependencyException pdsDataServiceDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(
                    pdsDataServiceDependencyException);
            }
            catch (Exception exception)
            {
                var failedServiceAccessOrchestrationException =
                    new FailedServiceAccessOrchestrationException(
                        message: "Failed access orchestration service error occurred, please contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedServiceAccessOrchestrationException);
            }
        }

        private async ValueTask<AccessOrchestrationServiceException> CreateAndLogServiceExceptionAsync(
           Xeption exception)
        {
            var accessOrchestrationServiceException = new AccessOrchestrationServiceException(
                message: "Access orchestration service error occurred, please contact support.",
                innerException: exception);

            await this.loggingBroker.LogErrorAsync(accessOrchestrationServiceException);

            return accessOrchestrationServiceException;
        }

        private async ValueTask<AccessOrchestrationValidationException>
            CreateAndLogValidationExceptionAsync(Xeption exception)
        {
            var accessOrchestrationValidationException =
                new AccessOrchestrationValidationException(
                    message: "Access orchestration validation error occurred, " +
                        "fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(accessOrchestrationValidationException);

            return accessOrchestrationValidationException;
        }

        private async ValueTask<AccessOrchestrationDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var accessOrchestrationDependencyValidationException =
                new AccessOrchestrationDependencyValidationException(
                    message: "Access orchestration dependency validation error occurred, " +
                        "fix the errors and try again.",
                    innerException: exception.InnerException as Xeption);

            await this.loggingBroker.LogErrorAsync(accessOrchestrationDependencyValidationException);

            return accessOrchestrationDependencyValidationException;
        }

        private async ValueTask<AccessOrchestrationDependencyException>
            CreateAndLogDependencyExceptionAsync(Xeption exception)
        {
            var accessOrchestrationDependencyException =
                new AccessOrchestrationDependencyException(
                    message: "Access orchestration dependency error occurred, " +
                        "fix the errors and try again.",
                    innerException: exception.InnerException as Xeption);

            await this.loggingBroker.LogErrorAsync(accessOrchestrationDependencyException);

            return accessOrchestrationDependencyException;
        }
    }
}
