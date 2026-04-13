// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Text.Json;
using LondonFhirService.Core.Models.Processings.ListEntryComparisons.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Processings.ListEntryComparisons;

public partial class ListEntryComparisonProcessingService
{
    protected internal virtual void ValidateOnCompareListEntryCounts(
        JsonElement source1List,
        JsonElement source2List,
        string listTitle)
    {
        Validate(
            createException: () => new InvalidListEntryComparisonProcessingException(
                message:
                    "Invalid list entry comparison arguments. " +
                    "Please correct the errors and try again."),

            (Rule: IsInvalid(source1List), Parameter: nameof(source1List)),
            (Rule: IsInvalid(source2List), Parameter: nameof(source2List)),
            (Rule: IsInvalid(listTitle), Parameter: nameof(listTitle)));
    }

    private static dynamic IsInvalid(string value) => new
    {
        Condition = String.IsNullOrWhiteSpace(value),
        Message = "Text is invalid."
    };

    private static dynamic IsInvalid(JsonElement element) => new
    {
        Condition = element.ValueKind != JsonValueKind.Object,
        Message = "Json element must be a non-null JSON object."
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
