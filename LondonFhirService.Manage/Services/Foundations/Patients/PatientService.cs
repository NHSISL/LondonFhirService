// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Manage.Brokers.Https;
using LondonFhirService.Manage.Models.Foundations.Patients;
using LondonFhirService.Manage.Models.Foundations.Patients.Exceptions;

namespace LondonFhirService.Manage.Services.Foundations.Patients
{
    /// <summary>
    /// Two calls through one broker: a client credentials token exchange, then the CareConnect
    /// $getstructuredrecord operation carrying that token. Both go out on IHttpBroker, so the
    /// service stays a single-broker foundation even though the work is two round trips.
    ///
    /// The credentials are resolved per request rather than at construction. Each of the four
    /// fields falls back to its PatientConfiguration counterpart only when the caller leaves it
    /// blank, which is what lets the page prove one consumer's credentials without disturbing the
    /// configured ones.
    /// </summary>
    internal partial class PatientService : IPatientService
    {
        /// <summary>
        /// What the CareConnect operation body is sent as. Every other caller of
        /// $getstructuredrecord and $everything in this solution sends this - the integration and
        /// performance suites both do - and a FHIR server is entitled to answer plain
        /// application/json with a 415.
        /// </summary>
        private const string FhirJsonMediaType = "application/fhir+json";

        private readonly IHttpBroker httpBroker;
        private readonly PatientConfiguration patientConfiguration;
        private readonly ILoggingBroker loggingBroker;

        public PatientService(
            IHttpBroker httpBroker,
            PatientConfiguration patientConfiguration,
            ILoggingBroker loggingBroker)
        {
            this.httpBroker = httpBroker;
            this.patientConfiguration = patientConfiguration;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<string> GetStructuredRecordAsync(
            StructuredRecordRequest structuredRecordRequest,
            CancellationToken cancellationToken = default) =>
            TryCatch(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                StructuredRecordCredentials structuredRecordCredentials =
                    ResolveCredentials(structuredRecordRequest, this.patientConfiguration);

                ValidateOnGetStructuredRecord(
                    structuredRecordRequest,
                    structuredRecordCredentials,
                    this.patientConfiguration);

                string accessToken = await GetAccessTokenAsync(
                    structuredRecordCredentials,
                    cancellationToken);

                // Trimmed on the way into the body, not just for the validation checks. Validation
                // parses the date from a trimmed copy, so " 1994-05-21 " was accepted and then sent
                // with its spaces intact for the provider to reject.
                string requestBody = CreateRequestBody(
                    nhsNumber: structuredRecordRequest.NhsNumber?.Trim(),
                    dateOfBirth: structuredRecordRequest.DateOfBirth?.Trim(),
                    demographicsOnly: structuredRecordRequest.DemographicsOnly);

                return await this.httpBroker.PostJsonContentAsync(
                    this.patientConfiguration.GetStructuredRecordUrl.Trim(),
                    requestBody,
                    FhirJsonMediaType,
                    accessToken,
                    cancellationToken);
            });

        private async ValueTask<string> GetAccessTokenAsync(
            StructuredRecordCredentials structuredRecordCredentials,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var formValues = new Dictionary<string, string>
            {
                ["client_id"] = structuredRecordCredentials.ClientId,
                ["client_secret"] = structuredRecordCredentials.ClientSecret,
                ["scope"] = structuredRecordCredentials.Scope,
                ["grant_type"] = structuredRecordCredentials.GrantType
            };

            string tokenResponse = await this.httpBroker.PostFormUrlEncodedContentAsync(
                this.patientConfiguration.AuthUrl.Trim(),
                formValues,
                cancellationToken);

            return ReadAccessToken(tokenResponse);
        }

        /// <summary>
        /// Reads access_token out of the token response. Everything else in that payload -
        /// token_type, expires_in, ext_expires_in - is deliberately ignored: this is a single
        /// call with no caching, so an expiry there is nothing to hold on to.
        /// </summary>
        private static string ReadAccessToken(string tokenResponse)
        {
            if (string.IsNullOrWhiteSpace(tokenResponse))
            {
                throw new InvalidAccessTokenPatientServiceException(
                    message: "Authorisation response was empty, contact support.");
            }

            using JsonDocument jsonDocument = JsonDocument.Parse(tokenResponse);

            // Phrased as the rejection rather than as a match, so there is no local holding null
            // while the checks run. Every way the payload can fail to carry a usable token leaves
            // through the same exit, and accessTokenElement is only read once past it.
            if (jsonDocument.RootElement.ValueKind != JsonValueKind.Object
                || jsonDocument.RootElement.TryGetProperty(
                    propertyName: "access_token",
                    value: out JsonElement accessTokenElement) is false
                || accessTokenElement.ValueKind != JsonValueKind.String)
            {
                throw new InvalidAccessTokenPatientServiceException(
                    message: "Authorisation response carried no access token, contact support.");
            }

            string accessToken = accessTokenElement.GetString();

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new InvalidAccessTokenPatientServiceException(
                    message: "Authorisation response carried no access token, contact support.");
            }

            return accessToken;
        }

        /// <summary>
        /// The CareConnect GetStructuredRecord Parameters resource. Inactive patients are never
        /// requested from this screen - it is a diagnostic view of what a consumer would get, and
        /// the consumer-facing proxy does not ask for them either - so the parameter is always
        /// sent as false rather than being exposed as a control.
        /// </summary>
        internal virtual string CreateRequestBody(
            string nhsNumber,
            string dateOfBirth = "",
            bool demographicsOnly = false,
            bool includeInactivePatients = false)
        {
            var parameters = new object[]
            {
                new
                {
                    name = "patientNHSNumber",
                    valueIdentifier = new
                    {
                        system = "https://fhir.hl7.org.uk/Id/nhs-number",
                        value = nhsNumber
                    }
                },
                new
                {
                    name = "demographicsOnly",
                    part = new object[]
                    {
                        new
                        {
                            name = "includeDemographicsOnly",
                            valueBoolean = demographicsOnly
                        }
                    }
                },
                new
                {
                    name = "includeInactivePatients",
                    part = new object[]
                    {
                        new
                        {
                            name = "includeInactivePatients",
                            valueBoolean = includeInactivePatients
                        }
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(dateOfBirth))
            {
                var dateOfBirthParameter = new
                {
                    name = "patientDOB",
                    valueIdentifier = new
                    {
                        system = "https://fhir.hl7.org.uk/Id/dob",
                        value = dateOfBirth
                    }
                };

                parameters = parameters.Append(dateOfBirthParameter).ToArray();
            }

            var requestBody = new
            {
                meta = new
                {
                    profile = new string[] {
                        "https://fhir.hl7.org.uk/STU3/OperationDefinition/" +
                            "CareConnect-GetStructuredRecord-Operation-1"
                    }
                },
                resourceType = "Parameters",
                parameter = parameters
            };

            string jsonContent = JsonSerializer.Serialize(requestBody);

            return jsonContent;
        }
    }
}
