// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Processings.ResourceMatchings.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Processings.ResourceMatchings;

public partial class ResourceMatcherProcessingService
{
    private delegate ValueTask<T> ReturningFunction<T>();

    private async ValueTask<T> TryCatch<T>(ReturningFunction<T> returningFunction)
    {
        try
        {
            return await returningFunction();
        }
        catch (InvalidArgumentResourceMatcherProcessingException
            invalidArgumentResourceMatcherProcessingException)
        {
            throw await CreateAndLogValidationExceptionAsync(
                invalidArgumentResourceMatcherProcessingException);
        }
        catch (Exception exception)
        {
            var failedResourceMatcherProcessingException =
                new FailedResourceMatcherProcessingException(
                    message: "Failed resource matcher processing exception occurred, please contact support",
                    innerException: exception,
                    data: exception.Data);

            throw await CreateAndLogServiceExceptionAsync(failedResourceMatcherProcessingException);
        }
    }

    private async ValueTask<ResourceMatcherProcessingValidationException>
        CreateAndLogValidationExceptionAsync(Xeption exception)
    {
        var resourceMatcherProcessingValidationException =
            new ResourceMatcherProcessingValidationException(
                message: "Resource matcher processing validation error occurred, " +
                    "please fix errors and try again.",
                innerException: exception);

        await this.loggingBroker.LogErrorAsync(resourceMatcherProcessingValidationException);

        return resourceMatcherProcessingValidationException;
    }

    private async ValueTask<ResourceMatcherProcessingServiceException>
        CreateAndLogServiceExceptionAsync(Xeption exception)
    {
        var resourceMatcherProcessingServiceException =
            new ResourceMatcherProcessingServiceException(
                message: "Resource matcher processing service error occurred, contact support.",
                innerException: exception);

        await this.loggingBroker.LogErrorAsync(resourceMatcherProcessingServiceException);

        return resourceMatcherProcessingServiceException;
    }
}
