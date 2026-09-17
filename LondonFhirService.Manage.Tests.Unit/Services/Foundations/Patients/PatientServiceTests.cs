// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Manage.Brokers.Https;
using LondonFhirService.Manage.Models.Foundations.Patients;
using LondonFhirService.Manage.Services.Foundations.Patients;
using Moq;
using Tynamix.ObjectFiller;

namespace LondonFhirService.Manage.Tests.Unit.Services.Foundations.Patients
{
    public partial class PatientServiceTests
    {
        private readonly Mock<IHttpBroker> httpBrokerMock;
        private readonly PatientConfiguration patientConfiguration;
        private readonly PatientService patientService;

        public PatientServiceTests()
        {
            this.httpBrokerMock = new Mock<IHttpBroker>();
            this.patientConfiguration = CreateRandomPatientConfiguration();

            this.patientService = new PatientService(
                httpBroker: this.httpBrokerMock.Object,
                patientConfiguration: this.patientConfiguration);
        }

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static string GetRandomNhsNumber() =>
            new IntRange(min: 100000000, max: 999999999).GetValue().ToString() + "0";

        private static string GetRandomDateOfBirth() =>
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-GetRandomNumber() * 1000))
                .ToString("yyyy-MM-dd");

        private static PatientConfiguration CreateRandomPatientConfiguration() =>
            new PatientConfiguration
            {
                AuthUrl = $"https://{GetRandomString()}.example.nhs.uk/token",
                ClientId = GetRandomString(),
                ClientSecret = GetRandomString(),
                Scope = GetRandomString(),
                GrantType = GetRandomString(),
                GetStructuredRecordUrl = $"https://{GetRandomString()}.example.nhs.uk/$getstructuredrecord"
            };

        private static StructuredRecordRequest CreateRandomStructuredRecordRequest() =>
            new StructuredRecordRequest
            {
                ClientId = GetRandomString(),
                ClientSecret = GetRandomString(),
                Scope = GetRandomString(),
                GrantType = GetRandomString(),
                NhsNumber = GetRandomNhsNumber(),
                DateOfBirth = GetRandomDateOfBirth(),
                DemographicsOnly = false
            };

        /// <summary>
        /// The shape the authorisation server actually answers with, kept whole rather than
        /// reduced to the one property the service reads - a response carrying only access_token
        /// would not prove the extra members are tolerated.
        /// </summary>
        private static string CreateTokenResponse(string accessToken) =>
            JsonSerializer.Serialize(new
            {
                token_type = "Bearer",
                expires_in = 3599,
                ext_expires_in = 3599,
                access_token = accessToken
            });

        private static Dictionary<string, string> CreateExpectedFormValues(
            string clientId,
            string clientSecret,
            string scope,
            string grantType) =>
            new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["scope"] = scope,
                ["grant_type"] = grantType
            };

        private static bool SameFormValuesAs(
            IDictionary<string, string> actualFormValues,
            IDictionary<string, string> expectedFormValues)
        {
            if (actualFormValues is null || actualFormValues.Count != expectedFormValues.Count)
            {
                return false;
            }

            foreach (KeyValuePair<string, string> expectedFormValue in expectedFormValues)
            {
                if (actualFormValues.TryGetValue(expectedFormValue.Key, out string actualValue)
                    is false
                    || actualValue != expectedFormValue.Value)
                {
                    return false;
                }
            }

            return true;
        }

        public static TheoryData<Exception> DependencyExceptions()
        {
            string randomMessage = GetRandomString();

            return new TheoryData<Exception>
            {
                new HttpRequestException(randomMessage),
                new HttpRequestException(randomMessage, new Exception(randomMessage))
            };
        }

        public static TheoryData<Exception> TimeoutExceptions()
        {
            string randomMessage = GetRandomString();

            return new TheoryData<Exception>
            {
                new TimeoutException(randomMessage),

                new TaskCanceledException(
                    message: randomMessage,
                    innerException: new TimeoutException(randomMessage)),

                new OperationCanceledException(
                    message: randomMessage,
                    innerException: new TimeoutException(randomMessage))
            };
        }

        public static TheoryData<Exception> CancellationExceptions()
        {
            string randomMessage = GetRandomString();
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            return new TheoryData<Exception>
            {
                new OperationCanceledException(randomMessage),
                new OperationCanceledException(cancellationTokenSource.Token),
                new TaskCanceledException(randomMessage)
            };
        }

        public static TheoryData<string> InvalidTexts() =>
            new TheoryData<string>
            {
                null,
                string.Empty,
                " ",
                "   "
            };

        public static TheoryData<string> InvalidDatesOfBirth() =>
            new TheoryData<string>
            {
                "01-10-2002",
                "2002/10/01",
                "2002-13-01",
                "not-a-date"
            };

        /// <summary>
        /// Every response a token endpoint can give that this service cannot take a bearer token
        /// from, short of malformed JSON - which is covered separately, because it fails in the
        /// parser rather than in the read.
        /// </summary>
        public static TheoryData<string> UnusableTokenResponses() =>
            new TheoryData<string>
            {
                string.Empty,
                "   ",
                "{}",
                "{\"token_type\":\"Bearer\"}",
                "{\"access_token\":\"\"}",
                "{\"access_token\":\"   \"}",
                "{\"access_token\":null}",
                "{\"access_token\":12345}",
                "[]",
                "null"
            };
    }
}
