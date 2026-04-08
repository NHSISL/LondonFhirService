// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Processings.ListEntryComparisons.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Processings.ListEntryComparisons;

public partial class ListEntryComparisonProcessingService
{
    private delegate ValueTask<T> ReturningFunction<T>();

    private async ValueTask<T> TryCatch<T>(ReturningFunction<T> returningFunction)
    {
        try
        {
            return await returningFunction();
        }
        catch (InvalidListEntryComparisonProcessingException invalidListEntryComparisonProcessingException)
        {
            throw await CreateAndLogValidationExceptionAsync(invalidListEntryComparisonProcessingException);
        }
        catch (Exception exception)
        {
            var failedListEntryComparisonProcessingException =
                new FailedListEntryComparisonProcessingException(
                    message: "Failed list entry comparison processing exception occurred, please contact support",
                    innerException: exception,
                    data: exception.Data);

            throw await CreateAndLogServiceExceptionAsync(failedListEntryComparisonProcessingException);
        }
    }

    private async ValueTask<ListEntryComparisonProcessingValidationException>
        CreateAndLogValidationExceptionAsync(Xeption exception)
    {
        var listEntryComparisonProcessingValidationException =
            new ListEntryComparisonProcessingValidationException(
                message:
                    "List entry comparison processing validation error occurred, " +
                    "please fix errors and try again.",
                innerException: exception);

        await this.loggingBroker.LogErrorAsync(listEntryComparisonProcessingValidationException);

        return listEntryComparisonProcessingValidationException;
    }

    private async ValueTask<ListEntryComparisonProcessingServiceException>
        CreateAndLogServiceExceptionAsync(Xeption exception)
    {
        var listEntryComparisonProcessingServiceException =
            new ListEntryComparisonProcessingServiceException(
                message: "List entry comparison processing service error occurred, contact support.",
                innerException: exception);

        await this.loggingBroker.LogErrorAsync(listEntryComparisonProcessingServiceException);

        return listEntryComparisonProcessingServiceException;
    }
}
