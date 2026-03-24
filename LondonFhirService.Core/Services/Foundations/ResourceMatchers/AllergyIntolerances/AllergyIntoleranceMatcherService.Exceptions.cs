// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Text.Json;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.AllergyIntolerances.Exceptions;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers.AllergyIntolerances;

public partial class AllergyIntoleranceMatcherService
{
    private delegate string? ReturningStringFunction();
    private delegate ResourceMatch ReturningResourceMatchFunction();

    private static string? TryCatch(ReturningStringFunction returningStringFunction)
    {
        try
        {
            return returningStringFunction();
        }
        catch (NullAllergyIntoleranceException nullAllergyIntoleranceException)
        {
            throw CreateAndThrowValidationException(nullAllergyIntoleranceException);
        }
        catch (InvalidOperationException invalidOperationException)
        {
            throw CreateAndThrowServiceException(invalidOperationException);
        }
        catch (JsonException jsonException)
        {
            throw CreateAndThrowServiceException(jsonException);
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
        catch (NullAllergyIntoleranceException nullResourceMatcherException)
        {
            throw CreateAndThrowValidationException(nullResourceMatcherException);
        }
        catch (InvalidOperationException invalidOperationException)
        {
            throw CreateAndThrowServiceException(invalidOperationException);
        }
        catch (JsonException jsonException)
        {
            throw CreateAndThrowServiceException(jsonException);
        }
        catch (Exception exception)
        {
            throw CreateAndThrowServiceException(exception);
        }
    }

    private static ResourceMatcherValidationException CreateAndThrowValidationException(Exception exception)
    {
        var resourceMatcherValidationException =
            new ResourceMatcherValidationException(
                message: "Resource matcher validation error occurred, please fix the errors and try again.",
                innerException: exception);

        throw resourceMatcherValidationException;
    }

    private static ResourceMatcherServiceException CreateAndThrowServiceException(Exception exception)
    {
        var failedResourceMatcherServiceException =
            new FailedAllergyIntolerancesServiceException(
                message: "Failed resource matcher service error occurred, contact support.",
                innerException: exception);

        var resourceMatcherServiceException =
            new ResourceMatcherServiceException(
                message: "Resource matcher service error occurred, contact support.",
                innerException: failedResourceMatcherServiceException);

        throw resourceMatcherServiceException;
    }
}
