// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Patients;
using Moq;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Patients
{
  public partial class PatientMatcherServiceTests
  {
      private readonly Mock<ILoggingBroker> loggingBrokerMock;
      private readonly PatientMatcherService patientMatcherService;

      public PatientMatcherServiceTests()
      {
          this.loggingBrokerMock = new Mock<ILoggingBroker>();

          this.patientMatcherService =
              new PatientMatcherService(
                  loggingBroker: this.loggingBrokerMock.Object);
      }

      private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
          actualException => actualException.SameExceptionAs(expectedException);

      private static Dictionary<string, JsonElement> CreateResourceIndex() =>
          new();

      private static JsonElement CreatePatientResource(
          string snomedCode,
          string onsetDateTime,
          string id = "allergy-1")
      {
          string json = $$"""
          {
            "resourceType": "Patient",
            "id": "{{id}}",
            "code": {
              "coding": [
                {
                  "system": "http://snomed.info/sct",
                  "code": "{{snomedCode}}"
                }
              ]
            },
            "onsetDateTime": "{{onsetDateTime}}"
          }
          """;

          return ParseJsonElement(json);
      }

      private static JsonElement CreateNonSnomedPatientResource(string onsetDateTime)
      {
          string json = $$"""
          {
            "resourceType": "Patient",
            "id": "allergy-1",
            "code": {
              "coding": [
                {
                  "system": "http://example.org/system",
                  "code": "123456"
                }
              ]
            },
            "onsetDateTime": "{{onsetDateTime}}"
          }
          """;

          return ParseJsonElement(json);
      }

      private static JsonElement CreateResourceWithoutOnsetDateTime(string snomedCode)
      {
          string json = $$"""
          {
            "resourceType": "Patient",
            "id": "allergy-1",
            "code": {
              "coding": [
                {
                  "system": "http://snomed.info/sct",
                  "code": "{{snomedCode}}"
                }
              ]
            }
          }
          """;

          return ParseJsonElement(json);
      }

      private static JsonElement CreateMalformedCodingResource()
      {
          string json = """
          {
            "resourceType": "Patient",
            "id": "allergy-1",
            "code": {
              "coding": {
                "system": "http://snomed.info/sct",
                "code": "91936005"
              }
            },
            "onsetDateTime": "2024-01-01"
          }
          """;

          return ParseJsonElement(json);
      }

      private static JsonElement CreatePatientWithNhsNumber(string nhsNumber, string id = "patient-1")
      {
          string json = $$"""
          {
            "resourceType": "Patient",
            "id": "{{id}}",
            "identifier": [
              {
                "system": "https://fhir.hl7.org.uk/Id/nhs-number",
                "value": "{{nhsNumber}}"
              }
            ]
          }
          """;

          return ParseJsonElement(json);
      }

      private static JsonElement CreatePatientWithoutIdentifier(string id = "patient-1")
      {
          string json = $$"""
          {
            "resourceType": "Patient",
            "id": "{{id}}"
          }
          """;

          return ParseJsonElement(json);
      }

      private static JsonElement CreatePatientWithNonNhsIdentifier(string id = "patient-1")
      {
          string json = $$"""
          {
            "resourceType": "Patient",
            "id": "{{id}}",
            "identifier": [
              {
                "system": "http://example.org/system",
                "value": "12345"
              }
            ]
          }
          """;

          return ParseJsonElement(json);
      }

      private static JsonElement CreateComprehensivePatientResource(
          string nhsNumber,
          string id)
      {
          string json = $$"""
              {
                "resourceType": "Patient",
                "id": "{{id}}",
                "meta": {
                  "versionId": "1",
                  "lastUpdated": "2024-09-12T08:00:00+00:00",
                  "profile": [
                    "https://fhir.hl7.org.uk/STU3/StructureDefinition/CareConnect-Patient-1"
                  ]
                },
                "text": {
                  "status": "generated",
                  "div": "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Mr Alex James Scott.</p></div>"
                },
                "identifier": [
                  {
                    "use": "official",
                    "system": "https://fhir.hl7.org.uk/Id/nhs-number",
                    "value": "{{nhsNumber}}"
                  },
                  {
                    "use": "secondary",
                    "system": "https://fhir.nhs.uk/Id/local-patient-id",
                    "value": "LPID-987654"
                  }
                ],
                "active": true,
                "name": [
                  {
                    "use": "official",
                    "text": "Mr Alex James Scott Jr",
                    "prefix": ["Mr"],
                    "given": ["Alex", "James"],
                    "family": "Scott",
                    "suffix": ["Jr"]
                  }
                ],
                "telecom": [
                  {
                    "system": "phone",
                    "value": "01612224444",
                    "use": "home"
                  },
                  {
                    "system": "phone",
                    "value": "07700900042",
                    "use": "mobile"
                  }
                ],
                "gender": "male",
                "birthDate": "1979-02-28",
                "deceasedBoolean": false,
                "address": [
                  {
                    "use": "home",
                    "type": "physical",
                    "line": ["12 Castlefield Road"],
                    "city": "London",
                    "postalCode": "E1 6AN",
                    "country": "GB"
                  }
                ],
                "maritalStatus": {
                  "coding": [
                    {
                      "system": "http://hl7.org/fhir/v3/MaritalStatus",
                      "code": "M",
                      "display": "Married"
                    }
                  ],
                  "text": "Married"
                },
                "multipleBirthBoolean": false,
                "communication": [
                  {
                    "language": {
                      "coding": [
                        {
                          "system": "urn:ietf:bcp:47",
                          "code": "en",
                          "display": "English"
                        }
                      ],
                      "text": "English"
                    },
                    "preferred": true
                  }
                ],
                "generalPractitioner": [
                  {
                    "reference": "Practitioner/8a77e616-2d12-4bc9-b0a1-2f1576ec1c05"
                  },
                  {
                    "reference": "Organization/56000299-bbd7-4dfa-ad64-c2d8692ae20c"
                  }
                ],
                "managingOrganization": {
                  "reference": "Organization/56000299-bbd7-4dfa-ad64-c2d8692ae20c"
                }
              }
              """;

          return ParseJsonElement(json);
      }

      private static JsonElement ParseJsonElement(string json) =>
          JsonDocument.Parse(json).RootElement.Clone();
  }
}