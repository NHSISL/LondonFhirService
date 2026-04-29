// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.PractitionerRoles;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.PractitionerRoles
{
    public partial class PractitionerRoleMatcherServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly PractitionerRoleMatcherService practitionerRoleMatcherService;

        public PractitionerRoleMatcherServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.practitionerRoleMatcherService =
                new PractitionerRoleMatcherService(
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

        private static string GetRandomSdsRoleProfileIdValue() =>
            $"999999{new IntRange(min: 100000, max: 999999).GetValue()}-A1";

        private static JsonElement CreatePractitionerRoleResource(
            string sdsRoleProfileId,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "PractitionerRole",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "https://fhir.nhs.uk/Id/sds-role-profile-id",
                    "value": "{{sdsRoleProfileId}}"
                  }
                ],
                "active": true
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateNonSdsPractitionerRoleResource(string id)
        {
            string json = $$"""
              {
                "resourceType": "PractitionerRole",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "http://example.org/system",
                    "value": "PRR-1"
                  }
                ],
                "active": true
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreatePractitionerRoleResourceWithoutIdentifierProperty(string id)
        {
            string json = $$"""
              {
                "resourceType": "PractitionerRole",
                "id": "{{id}}",
                "active": true
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateComprehensivePractitionerRoleResource(
            string sdsRoleProfileId,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "PractitionerRole",
                "id": "{{id}}",
                "meta": {
                  "versionId": "1",
                  "lastUpdated": "2024-09-01T08:00:00+01:00",
                  "profile": [
                    "https://fhir.hl7.org.uk/STU3/StructureDefinition/CareConnect-PractitionerRole-1"
                  ]
                },
                "text": {
                  "status": "generated",
                  "div": "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>GP role at Castlefield Surgery.</p></div>"
                },
                "identifier": [
                  {
                    "use": "official",
                    "system": "https://fhir.nhs.uk/Id/sds-role-profile-id",
                    "value": "{{sdsRoleProfileId}}"
                  },
                  {
                    "use": "usual",
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "PRR-b1238872"
                  }
                ],
                "active": true,
                "period": {
                  "start": "2018-04-01"
                },
                "practitioner": {
                  "reference": "Practitioner/8a77e616-2d12-4bc9-b0a1-2f1576ec1c05"
                },
                "organization": {
                  "reference": "Organization/56000299-bbd7-4dfa-ad64-c2d8692ae20c"
                },
                "code": [
                  {
                    "coding": [
                      {
                        "system": "https://fhir.hl7.org.uk/STU3/CodeSystem/CareConnect-SDSJobRoleName-1",
                        "code": "R0260",
                        "display": "General Medical Practitioner"
                      }
                    ]
                  }
                ],
                "specialty": [
                  {
                    "coding": [
                      {
                        "system": "http://snomed.info/sct",
                        "code": "394814009",
                        "display": "General practice (specialty)"
                      }
                    ]
                  }
                ],
                "location": [
                  {
                    "reference": "Location/34ff7bfc-a44b-4e3f-b51c-0aed08d3b7d0"
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
                "availableTime": [
                  {
                    "daysOfWeek": [
                      "mon",
                      "tue",
                      "wed",
                      "thu",
                      "fri"
                    ],
                    "availableStartTime": "09:00:00",
                    "availableEndTime": "17:30:00"
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
