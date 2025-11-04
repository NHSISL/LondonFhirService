// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Models.Orchestrations.Patients.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Orchestrations.Patients.R4
{
    public partial class R4PatientOrchestrationService
    {
        private static void ValidateArgsOnEverything(string id)
        {
            Validate(
                createException: () => new InvalidArgumentPatientOrchestrationException(
                    message: "Invalid patient orchestration argument, please correct the errors and try again."),

                (Rule: IsInvalid(id), Parameter: "Id"));
        }

        private static void ValidatePrimaryProviders(List<Provider> primaryProviders)
        {
            if (primaryProviders.Count != 1)
            {
                string message = primaryProviders.Count == 0
                    ? "No active primary provider found. One active primary provider required."
                    : $"Multiple active providers found: " +
                      $"{string.Join(", ", primaryProviders.Select(provider => provider.Name))}. " +
                      $"Only one active primary provider required.";

                throw new InvalidPrimaryProviderPatientOrchestrationException(message);
            }
        }

        private static dynamic IsInvalid(string text) => new
        {
            Condition = string.IsNullOrWhiteSpace(text),
            Message = "Text is required"
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