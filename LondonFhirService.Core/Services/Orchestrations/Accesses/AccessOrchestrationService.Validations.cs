// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Core.Models.Orchestrations.Accesses.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Orchestrations.Accesses
{
    public partial class AccessOrchestrationService
    {
        public static void ValidateNhsNumber(string nhsNumber)
        {
            Validate(
                createException: () => new InvalidArgumentAccessOrchestrationException(
                    message:
                        "Invalid argument access orchestration exception, " +
                        "please correct the errors and try again."),

                (Rule: IsInvalid(nhsNumber), Parameter: nameof(nhsNumber)));
        }

        private static dynamic IsInvalid(string name) => new
        {
            Condition = string.IsNullOrWhiteSpace(name),
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
