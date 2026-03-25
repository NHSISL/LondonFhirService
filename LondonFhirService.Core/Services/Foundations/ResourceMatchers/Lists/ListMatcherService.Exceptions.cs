// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Text.Json;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers.Lists;

public partial class ListMatcherService
{
    private delegate string? ReturningStringFunction();
    private delegate ResourceMatch ReturningResourceMatchFunction();

    private static string? TryCatch(ReturningStringFunction returningStringFunction)
    {
        try
        {
            return returningStringFunction();
        }
        catch (InvalidListMatcherServiceException invalidListMatcherServiceException)
        {
            throw CreateAndThrowValidationException(invalidListMatcherServiceException);
        }
        catch (Exception exception)
        {
            throw CreateAndThrowServiceException(exception);
        }
    }

    private static ResourceMatch TryCatch(ReturningResourceMatchFunction returningResourceMatchFunction)
    {
        try
        {
            return returningResourceMatchFunction();
        }
        catch (InvalidListMatcherServiceException invalidListMatcherServiceException)
        {
            throw CreateAndThrowValidationException(invalidListMatcherServiceException);
        }
        catch (Exception exception)
        {
            throw CreateAndThrowServiceException(exception);
        }
    }

    private static ListMatcherServiceValidationException CreateAndThrowValidationException(Exception exception)
    {
        var listMatcherServiceValidationException =
            new ListMatcherServiceValidationException(
                message: "List matcher service validation error occurred, please fix the errors and try again.",
                innerException: exception);

        throw listMatcherServiceValidationException;
    }

    private static ListMatcherServiceException CreateAndThrowServiceException(Exception exception)
    {
        var failedListMatcherServiceException =
            new FailedListMatcherServiceException(
                message: "Failed List matcher service error occurred, contact support.",
                innerException: exception);

        var listMatcherServiceException =
            new ListMatcherServiceException(
                message: "List matcher service error occurred, contact support.",
                innerException: failedListMatcherServiceException);

        throw listMatcherServiceException;
    }
}