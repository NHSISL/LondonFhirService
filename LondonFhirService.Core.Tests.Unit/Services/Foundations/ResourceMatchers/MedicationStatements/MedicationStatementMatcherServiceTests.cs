// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.MedicationStatements;
using Moq;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.MedicationStatements
{
    public partial class MedicationStatementMatcherServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly MedicationStatementMatcherService medicationStatementMatcherService;

        public MedicationStatementMatcherServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.medicationStatementMatcherService =
                new MedicationStatementMatcherService(
                    loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static Dictionary<string, JsonElement> CreateResourceIndex() =>
            new();

        private static JsonElement CreateMedicationStatementResource(
            string snomedCode,
            string onsetDateTime,
            string id = "allergy-1")
        {
            string json = $$"""
          {
            "resourceType": "MedicationStatement",
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

        private static JsonElement CreateNonSnomedMedicationStatementResource(string onsetDateTime)
        {
            string json = $$"""
          {
            "resourceType": "MedicationStatement",
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
            "resourceType": "MedicationStatement",
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
            "resourceType": "MedicationStatement",
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

        private static JsonElement CreateMedicationStatementWithCodeableConcept(
            string snomedCode,
            string dateAsserted,
            string id = "med-stmt-1")
        {
            string json = $$"""
          {
            "resourceType": "MedicationStatement",
            "id": "{{id}}",
            "medicationCodeableConcept": {
              "coding": [
                {
                  "system": "http://snomed.info/sct",
                  "code": "{{snomedCode}}"
                }
              ]
            },
            "dateAsserted": "{{dateAsserted}}"
          }
          """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateMedicationStatementWithReference(
            string reference,
            string dateAsserted,
            string id = "med-stmt-1")
        {
            string json = $$"""
          {
            "resourceType": "MedicationStatement",
            "id": "{{id}}",
            "medicationReference": {
              "reference": "{{reference}}"
            },
            "dateAsserted": "{{dateAsserted}}"
          }
          """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateMedicationForIndex(
            string snomedCode,
            string id = "medication-1")
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
            }
          }
          """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateNonSnomedMedicationStatement(
            string dateAsserted,
            string id = "med-stmt-1")
        {
            string json = $$"""
          {
            "resourceType": "MedicationStatement",
            "id": "{{id}}",
            "medicationCodeableConcept": {
              "coding": [
                {
                  "system": "http://example.org/system",
                  "code": "12345"
                }
              ]
            },
            "dateAsserted": "{{dateAsserted}}"
          }
          """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateMedicationStatementWithCodeableConceptWithoutDateAsserted(
            string snomedCode,
            string id = "med-stmt-1")
        {
            string json = $$"""
          {
            "resourceType": "MedicationStatement",
            "id": "{{id}}",
            "medicationCodeableConcept": {
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

        private static JsonElement CreateMedicationStatementWithReferenceAndCodeableConcept(
            string reference,
            string snomedCode,
            string dateAsserted,
            string id = "med-stmt-1")
        {
            string json = $$"""
          {
            "resourceType": "MedicationStatement",
            "id": "{{id}}",
            "medicationReference": {
              "reference": "{{reference}}"
            },
            "medicationCodeableConcept": {
              "coding": [
                {
                  "system": "http://snomed.info/sct",
                  "code": "{{snomedCode}}"
                }
              ]
            },
            "dateAsserted": "{{dateAsserted}}"
          }
          """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateComprehensiveMedicationStatementResource(
            string medicationReference,
            string dateAsserted,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "MedicationStatement",
                "id": "{{id}}",
                "meta": {
                  "versionId": "1",
                  "lastUpdated": "2024-09-12T09:25:00+01:00",
                  "profile": [
                    "https://fhir.hl7.org.uk/STU3/StructureDefinition/CareConnect-MedicationStatement-1"
                  ]
                },
                "text": {
                  "status": "generated",
                  "div": "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Patient is currently taking Metformin 500mg MR twice daily.</p></div>"
                },
                "identifier": [
                  {
                    "use": "official",
                    "system": "https://fhir.nhs.uk/Id/medication-statement-id",
                    "value": "MS-5d432111"
                  },
                  {
                    "use": "usual",
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "MS-5d432111"
                  }
                ],
                "basedOn": [
                  {
                    "reference": "MedicationRequest/0b1f9520-fa14-499b-8c03-e9f128a6d401"
                  }
                ],
                "context": {
                  "reference": "Encounter/d9a5732b-3af5-4fa6-8cda-b91e76823cdd"
                },
                "status": "active",
                "category": {
                  "coding": [
                    {
                      "system": "http://hl7.org/fhir/medication-statement-category",
                      "code": "community",
                      "display": "Community"
                    }
                  ]
                },
                "medicationReference": {
                  "reference": "{{medicationReference}}"
                },
                "effectivePeriod": {
                  "start": "2022-04-12"
                },
                "dateAsserted": "{{dateAsserted}}",
                "informationSource": {
                  "reference": "Practitioner/8a77e616-2d12-4bc9-b0a1-2f1576ec1c05"
                },
                "subject": {
                  "reference": "Patient/6e6c53cb-fca3-4e34-9cbc-476c32f1eb3c"
                },
                "taken": "y",
                "reasonCode": [
                  {
                    "coding": [
                      {
                        "system": "http://snomed.info/sct",
                        "code": "44054006",
                        "display": "Type 2 diabetes mellitus (disorder)"
                      }
                    ]
                  }
                ],
                "note": [
                  {
                    "text": "Patient confirms taking medication regularly."
                  }
                ],
                "dosage": [
                  {
                    "sequence": 1,
                    "text": "One tablet twice daily with food",
                    "timing": {
                      "repeat": {
                        "frequency": 2,
                        "period": 1,
                        "periodUnit": "d"
                      }
                    },
                    "route": {
                      "coding": [
                        {
                          "system": "http://snomed.info/sct",
                          "code": "26643006",
                          "display": "Oral route (qualifier value)"
                        }
                      ]
                    }
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