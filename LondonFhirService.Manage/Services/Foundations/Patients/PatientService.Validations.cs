// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Globalization;
using LondonFhirService.Manage.Models.Foundations.Patients;
using LondonFhirService.Manage.Models.Foundations.Patients.Exceptions;
using Xeptions;

namespace LondonFhirService.Manage.Services.Foundations.Patients
{
    internal partial class PatientService
    {
        /// <summary>
        /// Resolves the four credential fields against configuration. Blank means "use the
        /// configured one" rather than "send nothing", which is why this runs before validation
        /// and validation is then done against the result.
        /// </summary>
        private static StructuredRecordCredentials ResolveCredentials(
            StructuredRecordRequest structuredRecordRequest,
            PatientConfiguration patientConfiguration)
        {
            ValidateRequestIsNotNull(structuredRecordRequest);

            return new StructuredRecordCredentials
            {
                ClientId = FallBackWhenBlank(
                    structuredRecordRequest.ClientId,
                    patientConfiguration?.ClientId),

                ClientSecret = FallBackWhenBlank(
                    structuredRecordRequest.ClientSecret,
                    patientConfiguration?.ClientSecret),

                Scope = FallBackWhenBlank(
                    structuredRecordRequest.Scope,
                    patientConfiguration?.Scope),

                GrantType = FallBackWhenBlank(
                    structuredRecordRequest.GrantType,
                    patientConfiguration?.GrantType)
            };
        }

        // Both sides are trimmed, not just the caller's. A configured secret pasted with a
        // trailing newline would otherwise go out untrimmed and the token exchange would fail
        // with nothing on screen to explain why.
        private static string FallBackWhenBlank(string value, string fallbackValue) =>
            string.IsNullOrWhiteSpace(value) ? fallbackValue?.Trim() : value.Trim();

        private static void ValidateOnGetStructuredRecord(
            StructuredRecordRequest structuredRecordRequest,
            StructuredRecordCredentials structuredRecordCredentials,
            PatientConfiguration patientConfiguration)
        {
            ValidateRequestIsNotNull(structuredRecordRequest);
            ValidateConfigurationIsNotNull(patientConfiguration);

            Validate(
                createException: () => new InvalidPatientServiceException(
                    message: "Invalid patient request. Please correct the errors and try again."),

                (Rule: IsInvalid(structuredRecordRequest.NhsNumber),
                Parameter: nameof(StructuredRecordRequest.NhsNumber)),

                (Rule: IsInvalidDateOnly(structuredRecordRequest.DateOfBirth),
                Parameter: nameof(StructuredRecordRequest.DateOfBirth)),

                // Reported against the request's own field names, because that is what the
                // operator filled in. A blank one here means neither the form nor configuration
                // supplied a value.
                (Rule: IsInvalid(structuredRecordCredentials.ClientId),
                Parameter: nameof(StructuredRecordRequest.ClientId)),

                (Rule: IsInvalid(structuredRecordCredentials.ClientSecret),
                Parameter: nameof(StructuredRecordRequest.ClientSecret)),

                (Rule: IsInvalid(structuredRecordCredentials.Scope),
                Parameter: nameof(StructuredRecordRequest.Scope)),

                (Rule: IsInvalid(structuredRecordCredentials.GrantType),
                Parameter: nameof(StructuredRecordRequest.GrantType)),

                (Rule: IsInvalid(patientConfiguration.AuthUrl),
                Parameter: nameof(PatientConfiguration.AuthUrl)),

                (Rule: IsInvalid(patientConfiguration.GetStructuredRecordUrl),
                Parameter: nameof(PatientConfiguration.GetStructuredRecordUrl)));
        }

        private static void ValidateRequestIsNotNull(StructuredRecordRequest structuredRecordRequest)
        {
            if (structuredRecordRequest is null)
            {
                throw new NullPatientServiceException(
                    message: "Patient request is null.");
            }
        }

        private static void ValidateConfigurationIsNotNull(PatientConfiguration patientConfiguration)
        {
            if (patientConfiguration is null)
            {
                throw new NullPatientServiceException(
                    message: "Patient configuration is null.");
            }
        }

        private static dynamic IsInvalid(string text) => new
        {
            Condition = string.IsNullOrWhiteSpace(text),
            Message = "Text is invalid"
        };

        /// <summary>
        /// A date of birth is optional - $getstructuredrecord accepts a trace on the NHS number
        /// alone - but a malformed one is rejected here rather than sent on, because the provider
        /// answers a supplied-but-unparseable date with a bare 400 that says nothing useful.
        /// </summary>
        private static dynamic IsInvalidDateOnly(string text) => new
        {
            Condition =
                !string.IsNullOrWhiteSpace(text) &&
                !DateOnly.TryParseExact(
                    text.Trim(),
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out _),

            Message = "Text must be a valid date string in format 'yyyy-MM-dd' e.g. '2002-10-01'"
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
