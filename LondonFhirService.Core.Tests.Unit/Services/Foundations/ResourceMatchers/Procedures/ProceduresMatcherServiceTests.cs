// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Procedures;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Procedures
{
    public partial class ProcedureMatcherServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly ProcedureMatcherService procedureMatcherService;

        public ProcedureMatcherServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.procedureMatcherService =
                new ProcedureMatcherService(
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
            $"PROC-{new IntRange(min: 1000, max: 9999).GetValue()}";

        private static JsonElement CreateProcedureResource(
            string ddsIdentifierValue,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "Procedure",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "{{ddsIdentifierValue}}"
                  }
                ],
                "status": "completed"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateNonDdsProcedureResource(string id)
        {
            string json = $$"""
              {
                "resourceType": "Procedure",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "http://example.org/system",
                    "value": "PROC-1"
                  }
                ],
                "status": "completed"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateProcedureResourceWithoutIdentifierProperty(string id)
        {
            string json = $$"""
              {
                "resourceType": "Procedure",
                "id": "{{id}}",
                "status": "completed"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateComprehensiveProcedureResource(
            string ddsIdentifierValue,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "Procedure",
                "id": "{{id}}",
                "meta": {
                  "versionId": "1",
                  "lastUpdated": "2024-09-12T11:30:00+01:00",
                  "profile": [
                    "https://fhir.hl7.org.uk/STU3/StructureDefinition/CareConnect-Procedure-1"
                  ]
                },
                "text": {
                  "status": "generated",
                  "div": "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Diabetes review.</p></div>"
                },
                "identifier": [
                  {
                    "use": "official",
                    "system": "https://fhir.nhs.uk/Id/procedure-id",
                    "value": "{{ddsIdentifierValue}}"
                  },
                  {
                    "use": "usual",
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "{{ddsIdentifierValue}}"
                  }
                ],
                "status": "completed",
                "category": {
                  "coding": [
                    {
                      "system": "http://snomed.info/sct",
                      "code": "103693007",
                      "display": "Diagnostic procedure"
                    }
                  ]
                },
                "code": {
                  "coding": [
                    {
                      "system": "http://snomed.info/sct",
                      "code": "386053000",
                      "display": "Evaluation procedure (procedure)"
                    }
                  ],
                  "text": "Annual diabetes review"
                },
                "subject": {
                  "reference": "Patient/6e6c53cb-fca3-4e34-9cbc-476c32f1eb3c"
                },
                "context": {
                  "reference": "Encounter/d9a5732b-3af5-4fa6-8cda-b91e76823cdd"
                },
                "performedDateTime": "2024-09-12T11:30:00+01:00",
                "performer": [
                  {
                    "actor": {
                      "reference": "Practitioner/8a77e616-2d12-4bc9-b0a1-2f1576ec1c05"
                    },
                    "role": {
                      "coding": [
                        {
                          "system": "http://hl7.org/fhir/v2/0912",
                          "code": "PRF",
                          "display": "Performer"
                        }
                      ]
                    }
                  }
                ],
                "location": {
                  "reference": "Location/34ff7bfc-a44b-4e3f-b51c-0aed08d3b7d0"
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
                "note": [
                  {
                    "text": "Patient managing well; HbA1c on target."
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
