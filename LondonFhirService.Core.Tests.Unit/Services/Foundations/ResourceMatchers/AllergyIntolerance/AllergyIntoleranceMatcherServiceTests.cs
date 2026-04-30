// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.AllergyIntolerances;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.AllergyIntolerances
{
    public partial class AllergyIntoleranceMatcherServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly AllergyIntoleranceMatcherService allergyIntoleranceMatcherService;

        public AllergyIntoleranceMatcherServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.allergyIntoleranceMatcherService =
                new AllergyIntoleranceMatcherService(
                    loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static Dictionary<string, JsonElement> CreateResourceIndex() =>
            new();

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static string GetRandomSnomedCode() =>
            new IntRange(min: 1000000, max: 9999999).GetValue().ToString();

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static JsonElement CreateAllergyIntoleranceResource(
          string snomedCode,
          string onsetDateTime)
        {
            string id = GetRandomString();

            string json = $$"""
                {
                    "resourceType": "AllergyIntolerance",
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


        private static JsonElement CreateNonSnomedAllergyIntoleranceResource(string onsetDateTime)
        {
            string json = $$"""
          {
            "resourceType": "AllergyIntolerance",
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
            "resourceType": "AllergyIntolerance",
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
            "resourceType": "AllergyIntolerance",
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

        private static JsonElement CreateComprehensiveAllergyIntoleranceResource(
            string snomedCode,
            string onsetDateTime)
        {
            string id = GetRandomString();

            string json = $$"""
              {
                "resourceType": "AllergyIntolerance",
                "id": "{{id}}",
                "meta": {
                  "versionId": "1",
                  "lastUpdated": "2024-09-12T09:25:00+01:00",
                  "profile": [
                    "https://fhir.hl7.org.uk/STU3/StructureDefinition/CareConnect-AllergyIntolerance-1"
                  ]
                },
                "text": {
                  "status": "generated",
                  "div": "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Penicillin allergy.</p></div>"
                },
                "identifier": [
                  {
                    "use": "official",
                    "system": "https://fhir.nhs.uk/Id/allergy-intolerance-id",
                    "value": "AI-a11e2912"
                  },
                  {
                    "use": "usual",
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "AI-a11e2912"
                  }
                ],
                "clinicalStatus": "active",
                "verificationStatus": "confirmed",
                "type": "allergy",
                "category": ["medication"],
                "criticality": "high",
                "code": {
                  "coding": [
                    {
                      "system": "http://snomed.info/sct",
                      "code": "{{snomedCode}}",
                      "display": "Allergy to penicillin (finding)"
                    }
                  ],
                  "text": "Allergy to penicillin"
                },
                "patient": {
                  "reference": "Patient/6e6c53cb-fca3-4e34-9cbc-476c32f1eb3c"
                },
                "onsetDateTime": "{{onsetDateTime}}",
                "assertedDate": "2018-06-20",
                "recorder": {
                  "reference": "Practitioner/8a77e616-2d12-4bc9-b0a1-2f1576ec1c05"
                },
                "asserter": {
                  "reference": "Practitioner/8a77e616-2d12-4bc9-b0a1-2f1576ec1c05"
                },
                "lastOccurrence": "2023-04-12",
                "note": [
                  {
                    "text": "Reaction reported during hospital admission."
                  }
                ],
                "reaction": [
                  {
                    "substance": {
                      "coding": [
                        {
                          "system": "http://snomed.info/sct",
                          "code": "373270004",
                          "display": "Penicillin"
                        }
                      ]
                    },
                    "manifestation": [
                      {
                        "coding": [
                          {
                            "system": "http://snomed.info/sct",
                            "code": "247472004",
                            "display": "Hives"
                          }
                        ]
                      }
                    ],
                    "severity": "moderate"
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