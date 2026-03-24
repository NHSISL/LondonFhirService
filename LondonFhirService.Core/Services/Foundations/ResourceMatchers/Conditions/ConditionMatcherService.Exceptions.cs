// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Text.Json;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers.Conditions;

public partial class ConditionMatcherService
{
    private delegate string? ReturningStringFunction();
    private delegate ResourceMatch ReturningResourceMatchFunction();

    private static string? TryCatch(ReturningStringFunction returningStringFunction)
    {
        try
        {
            return returningStringFunction();
        }
        catch (InvalidConditionMatcherServiceException invalidConditionMatcherServiceException)
        {
            throw CreateAndThrowValidationException(invalidConditionMatcherServiceException);
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
        catch (InvalidConditionMatcherServiceException invalidConditionMatcherServiceException)
        {
            throw CreateAndThrowValidationException(invalidConditionMatcherServiceException);
        }
        catch (Exception exception)
        {
            throw CreateAndThrowServiceException(exception);
        }
    }

    private static ConditionMatcherServiceValidationException CreateAndThrowValidationException(Exception exception)
    {
        var conditionMatcherServiceValidationException =
            new ConditionMatcherServiceValidationException(
                message: "Condition matcher service validation error occurred, please fix the errors and try again.",
                innerException: exception);

        throw conditionMatcherServiceValidationException;
    }

    private static ConditionMatcherServiceException CreateAndThrowServiceException(Exception exception)
    {
        var failedConditionMatcherServiceException =
            new FailedConditionMatcherServiceException(
                message: "Failed Condition matcher service error occurred, contact support.",
                innerException: exception);

        var conditionMatcherServiceException =
            new ConditionMatcherServiceException(
                message: "Condition matcher service error occurred, contact support.",
                innerException: failedConditionMatcherServiceException);

        throw conditionMatcherServiceException;
    }
}