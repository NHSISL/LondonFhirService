// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Text.Json;
using LondonFhirService.Core.Models.Foundations.AllergyIntolerances;
using LondonFhirService.Core.Models.Foundations.AllergyIntolerances.AllergyIntolerances.Exceptions;

namespace LondonFhirService.Core.Services.Foundations.AllergyIntolerances.AllergyIntolerances;

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
        catch (NullAllergyIntoleranceException nullAllergyIntoleranceException)
        {
            throw CreateAndThrowValidationException(nullAllergyIntoleranceException);
        }
        catch (Exception exception)
        {
            throw CreateAndThrowServiceException(exception);
        }
    }

    private static AllergyIntoleranceValidationException CreateAndThrowValidationException(Exception exception)
    {
        var resourceMatcherValidationException =
            new AllergyIntoleranceValidationException(
                message: "Resource matcher validation error occurred, please fix the errors and try again.",
                innerException: exception);

        throw resourceMatcherValidationException;
    }

    private static AllergyIntoleranceServiceException CreateAndThrowServiceException(Exception exception)
    {
        var failedAllergyIntoleranceServiceException =
            new FailedAllergyIntolerancesServiceException(
                message: "Failed allergy intolerance service error occurred, contact support.",
                innerException: exception);

        var resourceMatcherServiceException =
            new AllergyIntoleranceServiceException(
                message: "Allergy intolerance service error occurred, contact support.",
                innerException: failedAllergyIntoleranceServiceException);

        throw resourceMatcherServiceException;
    }
}
