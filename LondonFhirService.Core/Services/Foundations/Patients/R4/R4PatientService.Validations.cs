// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using LondonFhirService.Core.Models.Foundations.Patients.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.Patients.R4
{
    public partial class R4PatientService
    {
        public static void ValidateOnGetStructuredRecord(List<string> providerNames, string id)
        {
            Validate(
                createException: () => new InvalidArgumentsPatientServiceException(
                    message:
                        "Invalid argument patient service exception, " +
                        "please correct the errors and try again."),

                (Rule: IsInvalid(providerNames), Parameter: nameof(providerNames)),
                (Rule: IsInvalid(id), Parameter: nameof(id)));
        }

        private static dynamic IsInvalid(List<string> strings) => new
        {
            Condition = strings is null,
            Message = "List cannot be null"
        };

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
