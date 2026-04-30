// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Medications;
using Moq;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Medications
{
    public partial class MedicationMatcherServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly MedicationMatcherService medicationMatcherService;

        public MedicationMatcherServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.medicationMatcherService =
                new MedicationMatcherService(
                    loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static Dictionary<string, JsonElement> CreateResourceIndex() =>
            new();

        private static JsonElement CreateMedicationResource(
            string snomedCode,
            string onsetDateTime,
            string id = "allergy-1")
        {
            string json = $$"""
          {
            "resourceType": "Medication",
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

        private static JsonElement CreateNonSnomedMedicationResource(string onsetDateTime)
        {
            string json = $$"""
          {
            "resourceType": "Medication",
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
            "resourceType": "Medication",
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
            "resourceType": "Medication",
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

        private static string GetRandomSnomedCode() =>
            Guid.NewGuid().ToString("N").Substring(0, 9);

        private static JsonElement CreateMedicationResourceWithNoCodeProperty()
        {
            string json = """
              {
                "resourceType": "Medication",
                "id": "medication-1"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateMedicationResourceWithNoCodingProperty()
        {
            string json = """
              {
                "resourceType": "Medication",
                "id": "medication-1",
                "code": {}
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateComprehensiveMedicationResource(
            string snomedCode,
            string medicationId)
        {
            string json = $$"""
              {
                "resourceType": "Medication",
                "id": "{{medicationId}}",
                "meta": {
                  "versionId": "1",
                  "lastUpdated": "2024-09-12T08:00:00+00:00",
                  "profile": [
                    "https://fhir.hl7.org.uk/STU3/StructureDefinition/CareConnect-Medication-1"
                  ]
                },
                "text": {
                  "status": "generated",
                  "div": "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Metformin 500mg MR.</p></div>"
                },
                "code": {
                  "coding": [
                    {
                      "system": "http://snomed.info/sct",
                      "code": "{{snomedCode}}",
                      "display": "Metformin 500mg modified-release tablets"
                    }
                  ],
                  "text": "Metformin 500mg MR"
                },
                "status": "active",
                "isBrand": false,
                "isOverTheCounter": false,
                "manufacturer": {
                  "reference": "Organization/56000299-bbd7-4dfa-ad64-c2d8692ae20c"
                },
                "form": {
                  "coding": [
                    {
                      "system": "http://snomed.info/sct",
                      "code": "385055001",
                      "display": "Tablet"
                    }
                  ]
                },
                "ingredient": [
                  {
                    "itemCodeableConcept": {
                      "coding": [
                        {
                          "system": "http://snomed.info/sct",
                          "code": "109081006",
                          "display": "Metformin"
                        }
                      ]
                    },
                    "isActive": true,
                    "amount": {
                      "numerator": {
                        "value": 500,
                        "unit": "mg",
                        "system": "http://unitsofmeasure.org",
                        "code": "mg"
                      },
                      "denominator": {
                        "value": 1,
                        "unit": "tablet",
                        "system": "http://unitsofmeasure.org",
                        "code": "{tbl}"
                      }
                    }
                  }
                ],
                "package": {
                  "container": {
                    "coding": [
                      {
                        "system": "http://snomed.info/sct",
                        "code": "419672006",
                        "display": "Bottle"
                      }
                    ]
                  }
                }
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement ParseJsonElement(string json) =>
            JsonDocument.Parse(json).RootElement.Clone();
    }
}