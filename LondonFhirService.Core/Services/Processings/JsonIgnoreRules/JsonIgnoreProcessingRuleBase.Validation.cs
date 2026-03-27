// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Text.Json;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Processings.JsonIgnoreRules
{
    public abstract partial class JsonIgnoreProcessingRuleBase
    {
        protected internal void ValidateOnShouldIgnore(JsonElement element, string path)
        {
            Validate(
                createException: () => new InvalidJsonIgnoreProcessingException(
                    message: "Invalid arguments. Please correct the errors and try again."),

                (Rule: IsInvalid(path), Parameter: nameof(path)),
                (Rule: IsInvalid(element), Parameter: nameof(element)));
        }

        protected internal void ValidateOnGetReplacement(JsonElement element)
        {
            Validate(
                createException: () => new InvalidJsonIgnoreProcessingException(
                    message: "Invalid arguments. Please correct the errors and try again."),

                (Rule: IsInvalid(element), Parameter: nameof(element)));
        }

        private static dynamic IsInvalid(string value) => new
        {
            Condition = String.IsNullOrWhiteSpace(value),
            Message = "Text is invalid"
        };

        private static dynamic IsInvalid(JsonElement element) => new
        {
            Condition = element.ValueKind == JsonValueKind.Undefined,
            Message = "Json element is undefined"
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
}
