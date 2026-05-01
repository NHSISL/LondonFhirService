// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.MedicationRequests;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.MedicationRequests
{
    public partial class MedicationRequestMatcherServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly MedicationRequestMatcherService medicationRequestMatcherService;

        public MedicationRequestMatcherServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.medicationRequestMatcherService =
                new MedicationRequestMatcherService(
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

        private static string GetRandomDdsIdentifierValue() =>
            $"MR-{new IntRange(min: 1000, max: 9999).GetValue()}";

        private static JsonElement CreateMedicationRequestResource(
            string ddsIdentifierValue,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "MedicationRequest",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "{{ddsIdentifierValue}}"
                  }
                ],
                "status": "active"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateNonDdsMedicationRequestResource(string id)
        {
            string json = $$"""
              {
                "resourceType": "MedicationRequest",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "http://example.org/system",
                    "value": "MR-1"
                  }
                ],
                "status": "active"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateMedicationRequestResourceWithoutIdentifierProperty(string id)
        {
            string json = $$"""
              {
                "resourceType": "MedicationRequest",
                "id": "{{id}}",
                "status": "active"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateComprehensiveMedicationRequestResource(
            string ddsIdentifierValue,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "MedicationRequest",
                "id": "{{id}}",
                "meta": {
                  "versionId": "1",
                  "lastUpdated": "2024-09-12T10:30:00+01:00",
                  "profile": [
                    "https://fhir.hl7.org.uk/STU3/StructureDefinition/CareConnect-MedicationRequest-1"
                  ]
                },
                "text": {
                  "status": "generated",
                  "div": "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Metformin 500mg twice daily.</p></div>"
                },
                "identifier": [
                  {
                    "use": "official",
                    "system": "https://fhir.nhs.uk/Id/medication-request-id",
                    "value": "{{ddsIdentifierValue}}"
                  },
                  {
                    "use": "usual",
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "{{ddsIdentifierValue}}"
                  }
                ],
                "status": "active",
                "intent": "order",
                "category": {
                  "coding": [
                    {
                      "system": "http://hl7.org/fhir/medication-request-category",
                      "code": "outpatient",
                      "display": "Outpatient"
                    }
                  ]
                },
                "priority": "routine",
                "medicationReference": {
                  "reference": "Medication/4f04814c-9a09-4605-8254-a8adecbd4353"
                },
                "subject": {
                  "reference": "Patient/6e6c53cb-fca3-4e34-9cbc-476c32f1eb3c"
                },
                "context": {
                  "reference": "Encounter/d9a5732b-3af5-4fa6-8cda-b91e76823cdd"
                },
                "authoredOn": "2024-09-12T10:30:00+01:00",
                "requester": {
                  "agent": {
                    "reference": "Practitioner/8a77e616-2d12-4bc9-b0a1-2f1576ec1c05"
                  }
                },
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
                "dosageInstruction": [
                  {
                    "text": "500 mg orally twice a day with meals.",
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
                    },
                    "doseQuantity": {
                      "value": 500,
                      "unit": "mg",
                      "system": "http://unitsofmeasure.org",
                      "code": "mg"
                    }
                  }
                ],
                "dispenseRequest": {
                  "validityPeriod": {
                    "start": "2024-09-12",
                    "end": "2025-09-11"
                  },
                  "numberOfRepeatsAllowed": 6,
                  "quantity": {
                    "value": 60,
                    "unit": "tablet",
                    "system": "http://unitsofmeasure.org",
                    "code": "{tbl}"
                  }
                },
                "note": [
                  {
                    "text": "Monitor renal function quarterly."
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
