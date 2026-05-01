// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Practitioners;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Practitioners
{
    public partial class PractitionerMatcherServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly PractitionerMatcherService practitionerMatcherService;

        public PractitionerMatcherServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.practitionerMatcherService =
                new PractitionerMatcherService(
                    loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static Dictionary<string, JsonElement> CreateResourceIndex() =>
            new();

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static string GetRandomSdsUserIdValue() =>
            $"999999{new IntRange(min: 100000, max: 999999).GetValue()}";

        private static JsonElement CreatePractitionerResource(
            string sdsUserId,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "Practitioner",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "https://fhir.nhs.uk/Id/sds-user-id",
                    "value": "{{sdsUserId}}"
                  }
                ],
                "active": true
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateNonSdsPractitionerResource(string id)
        {
            string json = $$"""
              {
                "resourceType": "Practitioner",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "http://example.org/system",
                    "value": "PR-1"
                  }
                ],
                "active": true
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreatePractitionerResourceWithoutIdentifierProperty(string id)
        {
            string json = $$"""
              {
                "resourceType": "Practitioner",
                "id": "{{id}}",
                "active": true
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateComprehensivePractitionerResource(
            string sdsUserId,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "Practitioner",
                "id": "{{id}}",
                "meta": {
                  "versionId": "1",
                  "lastUpdated": "2024-09-01T08:00:00+01:00",
                  "profile": [
                    "https://fhir.hl7.org.uk/STU3/StructureDefinition/CareConnect-Practitioner-1"
                  ]
                },
                "text": {
                  "status": "generated",
                  "div": "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Dr Sarah Elizabeth Williams.</p></div>"
                },
                "identifier": [
                  {
                    "use": "official",
                    "system": "https://fhir.nhs.uk/Id/sds-user-id",
                    "value": "{{sdsUserId}}"
                  },
                  {
                    "use": "secondary",
                    "system": "https://fhir.hl7.org.uk/Id/gmc-number",
                    "value": "7000123"
                  },
                  {
                    "use": "secondary",
                    "system": "https://fhir.nhs.uk/Id/sds-role-profile-id",
                    "value": "{{sdsUserId}}-A1"
                  },
                  {
                    "use": "secondary",
                    "system": "https://fhir.nhs.uk/Id/employee-number",
                    "value": "EMP-001234"
                  },
                  {
                    "use": "secondary",
                    "system": "https://fhir.hl7.org.uk/Id/smartcard",
                    "value": "SC-2255-UK-A82817-001"
                  },
                  {
                    "use": "usual",
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "PR-8a77e616"
                  }
                ],
                "active": true,
                "name": [
                  {
                    "use": "official",
                    "text": "Dr Sarah Elizabeth Williams",
                    "family": "Williams",
                    "given": [
                      "Sarah",
                      "Elizabeth"
                    ],
                    "prefix": [
                      "Dr"
                    ]
                  }
                ],
                "telecom": [
                  {
                    "system": "phone",
                    "value": "020 7946 0010",
                    "use": "work"
                  },
                  {
                    "system": "email",
                    "value": "sarah.williams@castlefield.example.nhs.uk",
                    "use": "work"
                  }
                ],
                "address": [
                  {
                    "use": "work",
                    "line": [
                      "1 Castlefield Road"
                    ],
                    "city": "London",
                    "postalCode": "E1 6AN",
                    "country": "GB"
                  }
                ],
                "gender": "female",
                "birthDate": "1978-05-22",
                "qualification": [
                  {
                    "identifier": [
                      {
                        "system": "https://fhir.hl7.org.uk/Id/qualification-id",
                        "value": "MBBS-1999-001"
                      }
                    ],
                    "code": {
                      "coding": [
                        {
                          "system": "http://terminology.hl7.org/CodeSystem/v2-0360",
                          "code": "MD",
                          "display": "Doctor of Medicine"
                        }
                      ],
                      "text": "MBBS"
                    },
                    "period": {
                      "start": "1999-07-01"
                    }
                  }
                ],
                "communication": [
                  {
                    "coding": [
                      {
                        "system": "urn:ietf:bcp:47",
                        "code": "en",
                        "display": "English"
                      }
                    ]
                  }
                ]
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement ParseJsonElement(string json) =>
            JsonDocument.Parse(json).RootElement.Clone();
    }
}
