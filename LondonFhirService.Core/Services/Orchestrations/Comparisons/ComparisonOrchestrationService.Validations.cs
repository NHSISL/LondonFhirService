// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Core.Models.Orchestrations.Comparisons.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Orchestrations.Comparisons
{
    public partial class ComparisonOrchestrationService
    {
        private static void ValidateCompareArguments(
            string correlationId,
            string source1Json,
            string source2Json)
        {
            Validate(
                createException: () => new InvalidComparisonOrchestrationException(
                    message: "Invalid comparison orchestration argument(s), " +
                        "fix the errors and try again."),

                (Rule: IsInvalid(correlationId), Parameter: nameof(correlationId)),
                (Rule: IsInvalid(source1Json), Parameter: nameof(source1Json)),
                (Rule: IsInvalid(source2Json), Parameter: nameof(source2Json)));
        }

        private static dynamic IsInvalid(string text) => new
        {
            Condition = string.IsNullOrWhiteSpace(text),
            Message = "Text is invalid"
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
