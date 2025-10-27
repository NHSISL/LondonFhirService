// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Models.Foundations.Providers.Exceptions;
using Microsoft.Data.SqlClient;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.Providers
{
    public partial class ProviderService
    {
        private delegate ValueTask<Provider> ReturningProviderFunction();

        private async ValueTask<Provider> TryCatch(ReturningProviderFunction returningProviderFunction)
        {
            try
            {
                return await returningProviderFunction();
            }
            catch (NullProviderServiceException nullProviderServiceException)
            {
                throw await CreateAndLogValidationException(nullProviderServiceException);
            }
            catch (InvalidProviderServiceException invalidProviderServiceException)
            {
                throw await CreateAndLogValidationException(invalidProviderServiceException);
            }
            catch (SqlException sqlException)
            {
                var failedStorageProviderServiceException =
                    new FailedStorageProviderServiceException(
                        message: "Failed provider storage error occurred, contact support.",
                        innerException: sqlException);

                throw await CreateAndLogCriticalDependencyException(failedStorageProviderServiceException);
            }
        }

        private async ValueTask<ProviderServiceValidationException> CreateAndLogValidationException(Xeption exception)
        {
            var providerServiceValidationException =
                new ProviderServiceValidationException(
                    message: "Provider validation errors occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(providerServiceValidationException);

            return providerServiceValidationException;
        }

        private async ValueTask<ProviderServiceDependencyException> CreateAndLogCriticalDependencyException(
            Xeption exception)
        {
            var providerServiceDependencyException =
                new ProviderServiceDependencyException(
                    message: "Provider dependency error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogCriticalAsync(providerServiceDependencyException);

            return providerServiceDependencyException;
        }
    }
}
