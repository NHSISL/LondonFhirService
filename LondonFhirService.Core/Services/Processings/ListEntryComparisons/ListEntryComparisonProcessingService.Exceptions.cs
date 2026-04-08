// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Core.Models.Processings.ListEntryComparisons.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Processings.ListEntryComparisons;

public partial class ListEntryComparisonProcessingService
{
    private delegate T ReturningFunction<T>();

    private T TryCatch<T>(ReturningFunction<T> returningFunction)
    {
        try
        {
            return returningFunction();
        }
        catch (InvalidListEntryComparisonProcessingException invalidListEntryComparisonProcessingException)
        {
            throw CreateAndLogValidationException(invalidListEntryComparisonProcessingException);
        }
        catch (Exception exception)
        {
            var failedListEntryComparisonProcessingException =
                new FailedListEntryComparisonProcessingException(
                    message: "Failed list entry comparison processing exception occurred, please contact support",
                    innerException: exception,
                    data: exception.Data);

            throw CreateAndLogServiceException(failedListEntryComparisonProcessingException);
        }
    }

    private ListEntryComparisonProcessingValidationException CreateAndLogValidationException(
        Xeption exception)
    {
        var listEntryComparisonProcessingValidationException =
            new ListEntryComparisonProcessingValidationException(
                message:
                    "List entry comparison processing validation error occurred, " +
                    "please fix errors and try again.",
                innerException: exception);

        this.loggingBroker.LogErrorAsync(listEntryComparisonProcessingValidationException)
            .GetAwaiter().GetResult();

        return listEntryComparisonProcessingValidationException;
    }

    private ListEntryComparisonProcessingServiceException CreateAndLogServiceException(
        Xeption exception)
    {
        var listEntryComparisonProcessingServiceException =
            new ListEntryComparisonProcessingServiceException(
                message: "List entry comparison processing service error occurred, contact support.",
                innerException: exception);

        this.loggingBroker.LogErrorAsync(listEntryComparisonProcessingServiceException)
            .GetAwaiter().GetResult();

        return listEntryComparisonProcessingServiceException;
    }
}
