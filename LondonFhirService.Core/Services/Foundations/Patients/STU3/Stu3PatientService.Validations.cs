// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using LondonFhirService.Core.Models.Foundations.Patients.Exceptions;
using LondonFhirService.Core.Models.Foundations.Providers;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.Patients.STU3
{
    public partial class Stu3PatientService
    {
        public static void ValidateOnEverything(
            List<Provider> providerNames,
            string id,
            Guid correlationId)
        {
            Validate(
                createException: () => new InvalidArgumentsPatientServiceException(
                    message:
                        "Invalid argument patient service exception, " +
                        "please correct the errors and try again."),

                (Rule: IsInvalid(providerNames), Parameter: nameof(providerNames)),
                (Rule: IsInvalid(id), Parameter: nameof(id)),
                (Rule: IsInvalid(correlationId), Parameter: nameof(correlationId)));
        }

        public static void ValidateOnGetStructuredRecord(
            List<Provider> activeProviders,
            string nhsNumber,
            Guid correlationId)
        {
            Validate(
                createException: () => new InvalidArgumentsPatientServiceException(
                    message:
                        "Invalid argument patient service exception, " +
                        "please correct the errors and try again."),

                (Rule: IsInvalid(activeProviders), Parameter: nameof(activeProviders)),
                (Rule: IsInvalid(nhsNumber), Parameter: nameof(nhsNumber)),
                (Rule: IsInvalid(correlationId), Parameter: nameof(correlationId)));
        }

        private static dynamic IsInvalid(List<Provider> providers) => new
        {
            Condition = providers is null || providers.Count == 0,
            Message = "List cannot be null"
        };

        private static dynamic IsInvalid(List<string> strings) => new
        {
            Condition = strings is null || strings.Count == 0,
            Message = "List cannot be null"
        };

        private static dynamic IsInvalid(string name) => new
        {
            Condition = string.IsNullOrWhiteSpace(name),
            Message = "Text is invalid"
        };

        private static dynamic IsInvalid(Guid? id) => new
        {
            Condition = id == null || id == Guid.Empty,
            Message = "Id is invalid"
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
