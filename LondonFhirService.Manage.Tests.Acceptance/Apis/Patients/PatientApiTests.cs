// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Text.Json;

using Tynamix.ObjectFiller;

using LondonFhirService.Manage.Tests.Acceptance.Brokers;
using LondonFhirService.Manage.Tests.Acceptance.Models.Patients;

namespace LondonFhirService.Manage.Tests.Acceptance.Apis.Patients
{
    /// <summary>
    /// The structured record endpoint end to end, over real HTTP, with only the outbound transport
    /// replaced. Authorisation, model binding, the controller's exception mapping and
    /// PatientService's own validation all run for real - which is the point, because a unit test
    /// calls the action method directly and cannot see any of them.
    ///
    /// There is no seeding and nothing to tear down: this endpoint reads from a provider rather
    /// than from this host's database, so a test decides what it sees by telling the mocked
    /// IHttpBroker what to answer.
    /// </summary>
    [Collection(nameof(ApiTestCollection))]
    public partial class PatientApiTests
    {
        private readonly ApiBroker apiBroker;

        public PatientApiTests(ApiBroker apiBroker)
        {
            this.apiBroker = apiBroker;

            // The factory owns one mock for the whole collection, so each test starts from a known
            // state rather than inheriting the previous one's setups and recorded invocations.
            this.apiBroker.ResetHttpBroker();
        }

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static string GetRandomNhsNumber() =>
            new IntRange(min: 100000000, max: 999999999).GetValue().ToString() + "0";

        private static string GetRandomDateOfBirth() =>
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-GetRandomNumber() * 1000))
                .ToString("yyyy-MM-dd");

        private static StructuredRecordRequest CreateRandomStructuredRecordRequest() =>
            new StructuredRecordRequest
            {
                ClientId = GetRandomString(),
                ClientSecret = GetRandomString(),
                Scope = GetRandomString(),
                GrantType = "client_credentials",
                NhsNumber = GetRandomNhsNumber(),
                DateOfBirth = GetRandomDateOfBirth(),
                DemographicsOnly = false
            };

        private static string CreateTokenResponse(string accessToken) =>
            JsonSerializer.Serialize(new
            {
                token_type = "Bearer",
                expires_in = 3599,
                ext_expires_in = 3599,
                access_token = accessToken
            });

        private static string CreateStructuredRecordResponse(string nhsNumber) =>
            JsonSerializer.Serialize(new
            {
                resourceType = "Bundle",
                type = "collection",

                entry = new object[]
                {
                    new
                    {
                        resource = new
                        {
                            resourceType = "Patient",

                            identifier = new object[]
                            {
                                new
                                {
                                    system = "https://fhir.nhs.uk/Id/nhs-number",
                                    value = nhsNumber
                                }
                            }
                        }
                    }
                }
            });
    }
}
