// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.ResourceMatchers
{
    public abstract partial class ResourceMatcherServiceBase
    {
        protected internal void ValidateOnGetMatchKeyArguments(
            JsonElement resource,
            Dictionary<string, JsonElement> resourceIndex)
        {
            Validate(
                createException: () => new InvalidArgumentResourceMatcherException(
                    message: "Resource matcher arguments are invalid. Please correct the errors and try again."),

                (Rule: IsInvalid(resource), Parameter: nameof(resource)),
                (Rule: IsInvalid(resourceIndex), Parameter: nameof(resourceIndex)));
        }

        protected internal void ValidateOnMatchArguments(
            List<JsonElement> source1Resources,
            List<JsonElement> source2Resources,
            Dictionary<string, JsonElement> source1ResourceIndex,
            Dictionary<string, JsonElement> source2ResourceIndex)
        {
            Validate(
                createException: () => new InvalidArgumentResourceMatcherException(
                    message: "Resource matcher arguments are invalid. Please correct the errors and try again."),

                (Rule: IsInvalid(source1Resources), Parameter: nameof(source1Resources)),
                (Rule: IsInvalid(source2Resources), Parameter: nameof(source2Resources)),
                (Rule: IsInvalid(source1ResourceIndex), Parameter: nameof(source1ResourceIndex)),
                (Rule: IsInvalid(source2ResourceIndex), Parameter: nameof(source2ResourceIndex)));
        }

        private static dynamic IsInvalid(string value) => new
        {
            Condition = string.IsNullOrWhiteSpace(value),
            Message = "Text is invalid."
        };

        private static dynamic IsInvalid(JsonElement jsonElement) => new
        {
            Condition =
                jsonElement.ValueKind == JsonValueKind.Undefined ||
                jsonElement.ValueKind == JsonValueKind.Null,
            Message = "Json element is invalid."
        };

        private static dynamic IsInvalid(List<JsonElement> jsonElements) => new
        {
            Condition = jsonElements is null,
            Message = "List is required."
        };

        private static dynamic IsInvalid(Dictionary<string, JsonElement> resourceIndex) => new
        {
            Condition = resourceIndex is null,
            Message = "Dictionary is required."
        };

        private static void Validate<TException>(
            Func<TException> createException,
            params (dynamic Rule, string Parameter)[] validations)
            where TException : Xeption
        {
            TException invalidResourceMatcherException = createException();

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidResourceMatcherException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidResourceMatcherException.ThrowIfContainsErrors();
        }
    }
}