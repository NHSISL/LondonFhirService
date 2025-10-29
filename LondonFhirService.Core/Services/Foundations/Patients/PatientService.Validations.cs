// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using LondonFhirService.Core.Models.Foundations.Patients.Exceptions;
using Xeptions;

namespace LondonFhirService.Core.Services.Foundations.Patients
{
    public partial class PatientService
    {
        public static void ValidateOnGetStructuredRecord(List<string> providers, string nhsNumber)
        {
            Validate(
                createException: () => new InvalidArgumentsPatientServiceException(
                    message:
                        "Invalid argument patient service exception, " +
                        "please correct the errors and try again."),

            (Rule: IsInvalid(providers), Parameter: nameof(providers)),
            (Rule: IsInvalid(nhsNumber), Parameter: nameof(nhsNumber)));
        }

        private static dynamic IsInvalid(List<string> strings) => new
        {
            Condition = strings.Count <= 0 || strings.Any(s => string.IsNullOrWhiteSpace(s)),
            Message = "List is invalid"
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
