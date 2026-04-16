// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Core.Models.Processings.ResourceMatchings.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Processings.ResourceMatchings;

public partial class ResourceMatcherProcessingService
{
    virtual internal void ValidateResourceType(string resourceType)
    {
        Validate(
            createException: () => new InvalidArgumentResourceMatcherProcessingException(
                message: "Invalid resource matcher processing arguments. " +
                    "Please correct the errors and try again."),

            (Rule: IsInvalid(resourceType), Parameter: nameof(resourceType)));
    }

    private static dynamic IsInvalid(string value) => new
    {
        Condition = String.IsNullOrWhiteSpace(value),
        Message = "Text is invalid."
    };

    private static void Validate<T>(
        Func<T> createException,
        params (dynamic Rule, string Parameter)[] validations)
        where T : Xeption
    {
        T invalidDataException = createException();

        foreach ((dynamic rule, string parameter) in validations)
        {
            if (rule.Condition)
            {
                invalidDataException.UpsertDataList(
                    key: parameter,
                    value: rule.Message);
            }
        }

        invalidDataException.ThrowIfContainsErrors();
    }
}
